namespace RestaurantSystem.Infrastructure.Messaging.Outbox;

public sealed class OutboxDispatcherOptions
{
    public int BatchSize { get; init; } = 10;
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(5);
}
