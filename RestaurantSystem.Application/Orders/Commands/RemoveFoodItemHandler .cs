using MediatR;
using RestaurantSystem.Application.Abstractions.Orders;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed class RemoveFoodItemHandler : IRequestHandler<RemoveFoodItemCommand>
{
    private readonly IOrderAggregateStore _orderAggregateStore;

    public RemoveFoodItemHandler(IOrderAggregateStore orderAggregateStore)
    {
        _orderAggregateStore = orderAggregateStore;
    }

    public async Task Handle(RemoveFoodItemCommand command, CancellationToken ct)
    {
        var order = await _orderAggregateStore.LoadAsync(command.OrderId, ct);
        if (order is null)
            throw new InvalidOperationException($"Order '{command.OrderId}' not found.");

        var occurredOnUtc = DateTime.UtcNow;
        order.RemoveItem(command.MenuItemId, command.Quantity, occurredOnUtc);

        await _orderAggregateStore.SaveAsync(order, ct);
    }
}
