namespace RestaurantSystem.Infrastructure.Messaging.Outbox
{
    public interface IOutboxDispatcher
    {
        Task DispatchPendingAsync(CancellationToken ct);
    }
}