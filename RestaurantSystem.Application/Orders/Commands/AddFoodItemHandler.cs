using MediatR;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Domain.Aggregates.Order;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed class AddFoodItemHandler : IRequestHandler<AddFoodItemCommand>
{
    private readonly IAggregateStore<Order> _aggregateStore;

    public AddFoodItemHandler(IAggregateStore<Order> aggregateStore)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task Handle(AddFoodItemCommand command, CancellationToken ct)
    {
        // 1. Load & Rehydrate (AggregateStore handles fetching history + rebuilding state)
        var order = await _aggregateStore.LoadAsync(command.OrderId, ct);

        // 2. Execute Domain Logic
        order.AddItem(command.MenuItemId, command.Name, command.Price, command.Quantity);

        // 3. Persist (SaveAsync handles EventStore append + ProjectAsync + ClearEvents)
        await _aggregateStore.SaveAsync(order, ct);
    }
}
