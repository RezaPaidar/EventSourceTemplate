using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events.Order;

public sealed class OrderDelivered : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}