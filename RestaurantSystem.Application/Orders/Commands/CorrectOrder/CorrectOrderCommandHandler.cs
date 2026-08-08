using MediatR;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Domain.Aggregates.Order;

namespace RestaurantSystem.Application.Orders.Commands.CorrectOrder;

public sealed class CorrectOrderCommandHandler : IRequestHandler<CorrectOrderCommand>
{
    private readonly IAggregateStore<Order> _aggregateStore;

    public CorrectOrderCommandHandler(IAggregateStore<Order> aggregateStore)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task Handle(CorrectOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _aggregateStore.LoadAsync(request.OrderId, cancellationToken);

        order.CorrectItemQuantity(
            request.MenuItemId,
            request.NewQuantity,
            request.OriginalEventId,
            request.Reason);

        await _aggregateStore.SaveAsync(order, cancellationToken);
    }
}
