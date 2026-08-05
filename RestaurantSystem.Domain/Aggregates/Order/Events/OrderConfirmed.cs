using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public sealed record OrderConfirmed : IEventSourcedEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public DateTime OccurredOnUtc { get; init; }

    public int EventVersion => 1;
}
