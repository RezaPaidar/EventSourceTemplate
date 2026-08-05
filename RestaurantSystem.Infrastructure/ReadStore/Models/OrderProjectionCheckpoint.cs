namespace RestaurantSystem.Infrastructure.ReadStore.Models;

public sealed class OrderProjectionCheckpoint
{
    public Guid Id { get; set; }
    public string ProjectorName { get; set; } = default!;
    public long LastProcessedVersion { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
