using MediatR;
using RestaurantSystem.Application.Abstractions.Orders;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Domain.Aggregates.Order;

namespace RestaurantSystem.Application.Orders.Commands.CorrectOrder;

public sealed class CorrectOrderCommandHandler : IRequestHandler<CorrectOrderCommand>
{
    private readonly IOrderAggregateStore _orderAggregateStore;

    public CorrectOrderCommandHandler(IOrderAggregateStore orderAggregateStore /* other deps */)
    {
        _orderAggregateStore = orderAggregateStore;
    }

    public async Task Handle(CorrectOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderAggregateStore.LoadAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            // Fail fast if aggregate does not exist
            throw new InvalidOperationException("Order aggregate was not found.");
        }

        order.CorrectItemQuantity(
            request.MenuItemId,
            request.NewQuantity,
            request.OriginalEventId,
            request.Reason);

        await _orderAggregateStore.SaveAsync(order, cancellationToken);
    }
}
