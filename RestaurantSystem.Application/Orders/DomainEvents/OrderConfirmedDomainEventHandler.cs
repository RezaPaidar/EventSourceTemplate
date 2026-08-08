using RestaurantSystem.Application.Abstractions.Events;
using RestaurantSystem.Application.Abstractions.Notifications;
using RestaurantSystem.Domain.Aggregates.Order.Events;

namespace RestaurantSystem.Application.Orders.DomainEvents;

public sealed class OrderConfirmedDomainEventHandler
    : IDomainEventHandler<OrderConfirmedDomainEvent>
{
    private readonly IKitchenOrderNotifier _kitchenOrderNotifier;
    public OrderConfirmedDomainEventHandler(IKitchenOrderNotifier kitchenOrderNotifier)
    {
        _kitchenOrderNotifier = kitchenOrderNotifier;
    }
    public async Task HandleAsync(
        OrderConfirmedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _kitchenOrderNotifier.NotifyOrderConfirmedAsync(domainEvent.OrderId, cancellationToken);

    }
}
