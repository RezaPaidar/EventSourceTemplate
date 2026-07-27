using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events;


public sealed class OrderStarted : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public int TableNumber { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}