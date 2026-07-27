using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events.Order;

public sealed class OrderConfirmed : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}