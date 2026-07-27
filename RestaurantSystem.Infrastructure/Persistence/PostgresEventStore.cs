using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Domain.Core;
using RestaurantSystem.Domain.Events.FoodItem;
using RestaurantSystem.Domain.Events.Order;
using System.Text.Json;
using IEventStore = RestaurantSystem.Application.Abstractions.Persistence.IEventStore;



namespace RestaurantSystem.Infrastructure.Persistence;

public sealed class PostgresEventStore : IEventStore
{
    private readonly EventStoreDbContext _dbContext;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        { typeof(FoodItemAdded).Name, typeof(FoodItemAdded) },
        { typeof(OrderConfirmed).Name, typeof(OrderConfirmed) }
    };

    public PostgresEventStore(
        EventStoreDbContext dbContext,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _dbContext = dbContext;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task SaveEventsAsync(
        Guid aggregateId,
        string aggregateType,
        IReadOnlyCollection<IDomainEvent> events,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        if (events.Count == 0)
            return;

        var currentVersion = await _dbContext.StoredEvents
            .Where(x => x.AggregateId == aggregateId)
            .Select(x => (int?)x.Version)
            .MaxAsync(cancellationToken) ?? 0;

        if (currentVersion != expectedVersion)
            throw new InvalidOperationException(
                $"Concurrency conflict for aggregate '{aggregateId}'. Expected version {expectedVersion}, but current version is {currentVersion}.");

        var version = expectedVersion;

        foreach (var domainEvent in events)
        {
            version++;

            var serializedPayload = JsonSerializer.Serialize(
                domainEvent,
                domainEvent.GetType(),
                _jsonSerializerOptions);

            var storedEvent = new StoredEventModel
            {
                EventId = domainEvent.EventId,
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                Version = version,
                EventType = domainEvent.GetType().Name,
                Data = serializedPayload,
                Metadata = "{}",
                CreatedAt = domainEvent.OccurredOnUtc
            };

            _dbContext.StoredEvents.Add(storedEvent);

            var outboxMessage = new OutboxMessageModel
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                Type = domainEvent.GetType().Name,
                Payload = serializedPayload
            };

            _dbContext.OutboxMessages.Add(outboxMessage);
        }
    }


    public async Task<IReadOnlyList<IDomainEvent>> LoadEventsAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var storedEvents = await _dbContext.StoredEvents
            .Where(x => x.AggregateId == aggregateId)
            .OrderBy(x => x.Version)
            .ToListAsync(cancellationToken);

        var domainEvents = new List<IDomainEvent>(storedEvents.Count);

        foreach (var storedEvent in storedEvents)
        {
            if (!EventTypeMap.TryGetValue(storedEvent.EventType, out var eventType))
                throw new InvalidOperationException(
                    $"Unknown event type '{storedEvent.EventType}' for aggregate '{aggregateId}'.");

            var domainEvent = JsonSerializer.Deserialize(
                storedEvent.Data,
                eventType,
                _jsonSerializerOptions) as IDomainEvent;

            if (domainEvent is null)
                throw new InvalidOperationException(
                    $"Failed to deserialize event '{storedEvent.EventType}' for aggregate '{aggregateId}'.");

            domainEvents.Add(domainEvent);
        }

        return domainEvents;
    }
}
