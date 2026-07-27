using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Application.Abstractions.Persistence;

public interface IEventStore
{
    Task SaveEventsAsync(
        Guid aggregateId,
        string aggregateType,
        IReadOnlyCollection<IDomainEvent> events,
        int expectedVersion,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IDomainEvent>> LoadEventsAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);
}
