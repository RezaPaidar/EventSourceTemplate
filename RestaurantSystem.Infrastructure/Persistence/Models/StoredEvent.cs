namespace RestaurantSystem.Infrastructure.Persistence.Models;

public class StoredEvent
{
    public long Id { get; set; } // Sequence for physical ordering
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = default!;
    public int Version { get; set; }
    public string EventType { get; set; } = default!;
    public string Data { get; set; } = default!; // JSONB
    public string Metadata { get; set; } = default!; // JSONB
    public DateTime CreatedAt { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}

public class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string ConsumerName { get; set; } = default!;
    public DateTime ProcessedAt { get; set; }
}