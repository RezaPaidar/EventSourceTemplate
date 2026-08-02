using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Domain.Core;
using System.Text.Json;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;
using IEventStore = RestaurantSystem.Application.Abstractions.Persistence.IEventStore;
using RestaurantSystem.Domain.Aggregates.Order.Events;



namespace RestaurantSystem.Infrastructure.Persistence.EventStore;
// Event store implementation for loading aggregate history and saving new events atomically.
public sealed class PostgresEventStore : IEventStore
{
    private readonly EventStoreDbContext _dbContext;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private static readonly Dictionary<string, Type> EventTypeMap = new()
    {
        { typeof(FoodItemAdded).Name, typeof(FoodItemAdded) },
        { typeof(OrderConfirmed).Name, typeof(OrderConfirmed) }
    };

    public PostgresEventStore(EventStoreDbContext dbContext, JsonSerializerOptions jsonSerializerOptions)
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

            var storedEvent = new StoredEvent
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

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                Type = GetOutboxType(domainEvent),
                Payload = serializedPayload
            };

            _dbContext.OutboxMessages.Add(outboxMessage);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    private static readonly Dictionary<Type, string> OutboxTypeMap = new()
    {
        { typeof(FoodItemAdded), "order.food-item-added.v1" },
    };
    private string GetOutboxType(IDomainEvent domainEvent)
    {
        if (OutboxTypeMap.TryGetValue(domainEvent.GetType(), out var outboxType))
        {
            return outboxType;
        }
        // اگر نیاز به Publish نیست، یا یک Type پیش‌فرض بده، یا خطا
        return domainEvent.GetType().Name;
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
