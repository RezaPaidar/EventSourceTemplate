namespace RestaurantSystem.Domain.Core;

public interface IEventStore
{
    Task<IReadOnlyList<EventEnvelope>> LoadAsync(
        Guid aggregateId,
        string aggregateType,
        CancellationToken cancellationToken = default);

    Task AppendAsync(
        Guid aggregateId,
        string aggregateType,
        IReadOnlyList<EventEnvelope> events,
        int expectedStreamPosition,
        CancellationToken cancellationToken = default);
}
