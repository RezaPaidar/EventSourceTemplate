namespace RestaurantSystem.Infrastructure.ReadStore.Models;

public sealed class OrderProjectionProcessedEvent
{
    public Guid Id { get; set; }
    public string ProjectorName { get; set; } = default!;
    public Guid EventId { get; set; }
    public long EventVersion { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
