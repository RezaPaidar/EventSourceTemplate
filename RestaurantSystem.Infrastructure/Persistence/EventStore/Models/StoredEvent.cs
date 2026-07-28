namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

public sealed class StoredEvent
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public int Version { get; set; }
    public string Data { get; set; } = default!;
    public string Metadata { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}