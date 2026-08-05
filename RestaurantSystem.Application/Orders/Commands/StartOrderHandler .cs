using MediatR;
using RestaurantSystem.Application.Abstractions.Orders;
using RestaurantSystem.Domain.Aggregates.Order;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed class StartOrderHandler : IRequestHandler<StartOrderCommand>
{
    private readonly IOrderAggregateStore _orderAggregateStore;

    public StartOrderHandler(IOrderAggregateStore orderAggregateStore)
    {
        _orderAggregateStore = orderAggregateStore;
    }

    public async Task Handle(StartOrderCommand command, CancellationToken ct)
    {
        var occurredOnUtc = DateTime.UtcNow;

        var order = Order.Start(
            command.OrderId,
            command.CustomerId,
            occurredOnUtc);

        await _orderAggregateStore.SaveAsync(order, ct);
    }
}
