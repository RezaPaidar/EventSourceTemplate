using MediatR;
using RestaurantSystem.Application.Abstractions.Orders;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IOrderAggregateStore _orderAggregateStore;

    public ConfirmOrderHandler(IOrderAggregateStore orderAggregateStore)
    {
        _orderAggregateStore = orderAggregateStore;
    }

    public async Task Handle(ConfirmOrderCommand command, CancellationToken ct)
    {
        // 1. Load & Rehydrate
        var order = await _orderAggregateStore.LoadAsync(command.OrderId, ct);
        if (order is null)
            throw new InvalidOperationException($"Order '{command.OrderId}' not found.");

        // 2. Domain Logic
        var occurredOnUtc = DateTime.UtcNow;
        order.Confirm(occurredOnUtc);

        // 3. Persist
        await _orderAggregateStore.SaveAsync(order, ct);
    }
}
