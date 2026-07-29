
using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public record OrderConfirmed : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}