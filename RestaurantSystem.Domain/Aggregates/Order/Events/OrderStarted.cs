using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public record OrderStarted : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public int TableNumber { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}