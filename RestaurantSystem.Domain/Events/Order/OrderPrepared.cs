using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events;


public sealed class OrderPrepared : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}