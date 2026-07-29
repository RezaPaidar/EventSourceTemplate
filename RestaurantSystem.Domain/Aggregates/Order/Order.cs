using RestaurantSystem.Domain.Core;
using RestaurantSystem.Domain.Events;
using RestaurantSystem.Domain.Events.FoodItem;
using RestaurantSystem.Domain.Events.Order;

namespace RestaurantSystem.Domain.Aggregates.Order;

public class Order : AggregateRoot
{
    private bool _isConfirmed;
    private readonly List<OrderItem> _items = new();

    public int TableNumber { get; private set; }

    public Order()
    {
    }

    public static Order Start(Guid id, int tableNumber)
    {
        var order = new Order();
        order.RaiseEvent(new OrderStarted
        {
            EventId = Guid.NewGuid(),
            OrderId = id,
            TableNumber = tableNumber,
            OccurredOnUtc = DateTime.UtcNow
        });
        return order;
    }

    public void AddItem(Guid menuItemId, string name, decimal price, int quantity)
    {
        if (_isConfirmed)
            throw new InvalidOperationException("Cannot add items to a confirmed order.");

        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        if (price <= 0)
            throw new InvalidOperationException("Price must be greater than zero.");

        RaiseEvent(new FoodItemAdded
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            MenuItemId = menuItemId,
            Name = name,
            Price = price,
            Quantity = quantity,
            OccurredOnUtc = DateTime.UtcNow
        });
    }

    public void RemoveItem(Guid menuItemId)
    {
        if (_isConfirmed)
            throw new InvalidOperationException("Cannot remove items from a confirmed order.");

        if (_items.All(x => x.MenuItemId != menuItemId))
            throw new InvalidOperationException("Item not found in order.");

        RaiseEvent(new FoodItemRemoved
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            MenuItemId = menuItemId,
            OccurredOnUtc = DateTime.UtcNow
        });
    }

    public void Confirm()
    {
        if (_isConfirmed)
            return;

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an empty order.");

        RaiseEvent(new OrderConfirmed
        {
            EventId = Guid.NewGuid(),
            OrderId = Id,
            OccurredOnUtc = DateTime.UtcNow
        });
    }

    protected override void ApplyEvent(IDomainEvent @event)
    {
        switch (@event)
        {
            case OrderStarted e:
                Id = e.OrderId;
                TableNumber = e.TableNumber;
                _isConfirmed = false;
                break;

            case FoodItemAdded e:
                _items.Add(new OrderItem(e.MenuItemId, e.Name, e.Price));
                break;

            case FoodItemRemoved e:
                var item = _items.FirstOrDefault(x => x.MenuItemId == e.MenuItemId);
                if (item is not null)
                    _items.Remove(item);
                break;

            case OrderConfirmed:
                _isConfirmed = true;
                break;

            default:
                throw new InvalidOperationException($"Unsupported event type: {@event.GetType().Name}");
        }
    }
    public static Order LoadFromHistory(Guid id, IReadOnlyList<IDomainEvent> history)
    {
        var order = new Order();
        order.Id = id;
        order.LoadFromHistory(history);
        return order;
    }
}

public record OrderItem(Guid MenuItemId, string Name, decimal Price);
