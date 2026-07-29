namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public Guid? LockId { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}