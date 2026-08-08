using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public class OrderConfirmedDomainEvent : IDomainEvent
{
    public OrderConfirmedDomainEvent(Guid orderId)
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
        OrderId = orderId;
    }
    public Guid EventId { get; }
    public DateTime OccurredOnUtc { get; }
    public Guid OrderId { get; }
}