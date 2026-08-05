using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Orders.IntegrationEvents;
using RestaurantSystem.Domain.Aggregates.Order.Events;
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

    public async Task AppendAsync(Guid aggregateId, string aggregateType, IReadOnlyList<EventEnvelope> events, int expectedStreamPosition, CancellationToken cancellationToken = default)
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
            var outboxMessage = CreateOutboxMessage(envelope);

            if (outboxMessage is not null)
            {
                await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    private static OutboxMessage? CreateOutboxMessage(EventEnvelope envelope)
    {
        object? integrationEvent = envelope.Event switch
        {
            OrderStarted e => new OrderStartedIntegrationEventV1(
                envelope.EventId,
                envelope.AggregateId,
                e.TableNumber,
                envelope.OccurredOnUtc),

            FoodItemAdded e => new FoodItemAddedIntegrationEventV1(
                envelope.EventId,
                envelope.AggregateId,
                e.MenuItemId,
                e.Name,
                e.Price,
                e.Quantity,
                envelope.OccurredOnUtc),

            FoodItemRemoved e => new FoodItemRemovedIntegrationEventV1(
                envelope.EventId,
                envelope.AggregateId,
                e.MenuItemId,
                e.Quantity,
                envelope.OccurredOnUtc),

            OrderConfirmed e => new OrderConfirmedIntegrationEventV1(
                envelope.EventId,
                envelope.AggregateId,
                envelope.OccurredOnUtc),

            _ => null
        };


        if (integrationEvent is null)
            return null;

        var messageType = integrationEvent switch
        {
            OrderStartedIntegrationEventV1 => "order.started.v1",
            FoodItemAddedIntegrationEventV1 => "order.food-item-added.v1",
            FoodItemRemovedIntegrationEventV1 => "order.food-item-removed.v1",
            OrderConfirmedIntegrationEventV1 => "order.confirmed.v1",
            _ => throw new InvalidOperationException("Unsupported integration event.")
        };

        return new OutboxMessage
        {
            Id = envelope.EventId,
            Type = messageType,
            Payload = JsonSerializer.Serialize(
                integrationEvent,
                integrationEvent.GetType(),
                SerializerOptions),
            OccurredOnUtc = envelope.OccurredOnUtc
        };
    }

}
