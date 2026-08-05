using RestaurantSystem.Domain.Aggregates.Order.Events;
using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order;

public sealed class Order : AggregateRoot
{
    private bool _isConfirmed;
    private readonly List<OrderItem> _items = new();

    public int TableNumber { get; private set; }

    private Order() { }

    public static Order Start(Guid orderId, int tableNumber, DateTime occurredOnUtc)
    {
        var order = new Order();

        var @event = new OrderStarted
        {
            EventId = Guid.NewGuid(),
            OrderId = orderId,
            TableNumber = tableNumber,
            OccurredOnUtc = occurredOnUtc
        };

        order.RaiseEvent(@event);
        return order;
    }

    public void AddItem(Guid menuItemId, string name, decimal price, int quantity, DateTime occurredOnUtc)
    {
        if (_isConfirmed)
            throw new InvalidOperationException("Cannot add items to a confirmed order.");

        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        if (price <= 0)
            throw new InvalidOperationException("Price must be greater than zero.");

        var @event = (new FoodItemAdded
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            MenuItemId = menuItemId,
            Name = name,
            Price = price,
            Quantity = quantity,
            OccurredOnUtc = occurredOnUtc
        });

        RaiseEvent(@event);
    }

    public void RemoveItem(Guid menuItemId, int quantity, DateTime occurredOnUtc)
    {
        if (_isConfirmed)
            throw new InvalidOperationException("Cannot remove items from a confirmed order.");

        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(x => x.MenuItemId == menuItemId);
        if (existingItem is null)
            throw new InvalidOperationException("Item not found in order.");

        if (quantity > existingItem.Quantity)
            throw new InvalidOperationException("Cannot remove more quantity than exists in the order.");

        var @event = new FoodItemRemoved
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            MenuItemId = menuItemId,
            Quantity = quantity,
            OccurredOnUtc = occurredOnUtc
        };

        RaiseEvent(@event);
    }


    public void Confirm(DateTime occurredOnUtc)
    {
        if (_isConfirmed)
            return;

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an empty order.");

        var @event = new OrderConfirmed
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            OccurredOnUtc = occurredOnUtc
        };

        RaiseEvent(@event);
    }

    protected override void When(IEventSourcedEvent @event)
    {
        switch (@event)
        {
            case OrderStarted e:
                Apply(e);
                break;

            case FoodItemAdded e:
                Apply(e);
                break;

            case FoodItemRemoved e:
                Apply(e);
                break;

            case OrderConfirmed e:
                Apply(e);
                break;

            default:
                throw new InvalidOperationException($"Unsupported event type: {@event.GetType().Name}");
        }
    }
    private void Apply(OrderStarted e)
    {
        Id = e.OrderId;
        TableNumber = e.TableNumber;
        _isConfirmed = false;
        _items.Clear();
    }
    private void Apply(FoodItemAdded e)
    {
        _items.Add(new OrderItem(e.MenuItemId, e.Name, e.Price, e.Quantity));
    }

    private void Apply(FoodItemRemoved e)
    {
        var existingItem = _items.FirstOrDefault(x => x.MenuItemId == e.MenuItemId);
        if (existingItem is null)
            return;

        var remainingQuantity = existingItem.Quantity - e.Quantity;

        _items.Remove(existingItem);

        if (remainingQuantity > 0)
        {
            _items.Add(existingItem with { Quantity = remainingQuantity });
        }
    }

    private void Apply(OrderConfirmed e)
    {
        _isConfirmed = true;
    }
    public static Order LoadFromHistory(Guid id, IReadOnlyList<IEventSourcedEvent> history)
    {
        var order = new Order();
        order.Id = id;
        order.LoadFromHistory(history);
        return order;
    }
}

public record OrderItem(Guid MenuItemId, string Name, decimal Price, int Quantity);

