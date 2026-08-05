using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Domain.Core;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence.EventStore;

public sealed class PostgresEventStore : IEventStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly EventStoreDbContext _dbContext;

    public PostgresEventStore(EventStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventEnvelope>> LoadAsync(
        Guid aggregateId,
        string aggregateType,
        CancellationToken cancellationToken = default)
    {
        var storedEvents = await _dbContext.StoredEvents
            .Where(e => e.AggregateId == aggregateId && e.AggregateType == aggregateType)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        var result = new List<EventEnvelope>(storedEvents.Count);

        foreach (var stored in storedEvents)
        {
            var eventType = Type.GetType(stored.EventType, throwOnError: true)!;

            if (!typeof(IEventSourcedEvent).IsAssignableFrom(eventType))
                throw new InvalidOperationException(
                    $"Stored event type '{stored.EventType}' does not implement IEventSourcedEvent.");

            var domainEvent = (IEventSourcedEvent)JsonSerializer.Deserialize(
                stored.Data,
                eventType,
                SerializerOptions)!;

            var envelope = new EventEnvelope
            {
                EventId = stored.EventId,
                AggregateId = stored.AggregateId,
                AggregateType = stored.AggregateType,
                AggregateVersion = stored.Version,
                EventType = stored.EventType,
                Event = domainEvent,
                OccurredOnUtc = stored.CreatedAt,
                StreamPosition = stored.Version
            };

            result.Add(envelope);
        }

        return result;
    }

    public async Task AppendAsync(
        Guid aggregateId,
        string aggregateType,
        IReadOnlyList<EventEnvelope> events,
        int expectedStreamPosition,
        CancellationToken cancellationToken = default)
    {
        int currentStreamPosition = await _dbContext.StoredEvents
            .Where(e => e.AggregateId == aggregateId && e.AggregateType == aggregateType)
            .Select(e => (int?)e.Version)
            .MaxAsync(cancellationToken) ?? -1;

        if (currentStreamPosition != expectedStreamPosition)
        {
            throw new ConcurrencyException(
                $"Expected stream position {expectedStreamPosition} but was {currentStreamPosition}.");
        }

        var nextVersion = currentStreamPosition + 1;

        foreach (var envelope in events)
        {
            var eventType = envelope.Event.GetType();

            var data = JsonSerializer.Serialize(envelope.Event, eventType, SerializerOptions);
            var metadata = JsonSerializer.Serialize(new
            {
                envelope.EventId,
                envelope.AggregateId,
                envelope.AggregateType,
                envelope.AggregateVersion,
                envelope.EventType,
                envelope.OccurredOnUtc
            }, SerializerOptions);

            var stored = new StoredEvent
            {
                EventId = envelope.EventId,
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                EventType = eventType.AssemblyQualifiedName!,
                Version = nextVersion,
                Data = data,
                Metadata = metadata,
                CreatedAt = envelope.OccurredOnUtc
            };

            nextVersion++;

            await _dbContext.StoredEvents.AddAsync(stored, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
