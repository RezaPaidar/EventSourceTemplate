namespace RestaurantSystem.Domain.Core;

public sealed class EventEnvelope
{
    public Guid EventId { get; init; }
    public Guid AggregateId { get; init; }
    public string AggregateType { get; init; } = default!;
    public int AggregateVersion { get; init; }

    public string EventType { get; init; } = default!;
    public IEventSourcedEvent Event { get; init; } = default!;

    public DateTime OccurredOnUtc { get; init; }

    public int StreamPosition { get; init; }
}
