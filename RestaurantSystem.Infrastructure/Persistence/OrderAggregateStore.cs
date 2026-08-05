using RestaurantSystem.Application.Abstractions.Orders;
using RestaurantSystem.Domain.Aggregates.Order;
using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Infrastructure.Persistence.Projections;

public sealed class OrderAggregateStore : IOrderAggregateStore
{
    private readonly IEventStore _eventStore;

    public OrderAggregateStore(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Order?> LoadAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var envelopes = await _eventStore.LoadAsync(
            orderId,
            aggregateType: typeof(Order).FullName!,
            cancellationToken);

        if (envelopes.Count == 0)
            return null;

        var history = envelopes
            .OrderBy(e => e.StreamPosition)
            .Select(e => e.Event)
            .ToList()
            .AsReadOnly();

        var order = Order.LoadFromHistory(orderId, history);
        return order;
    }

    public async Task SaveAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        var uncommitted = order.UncommittedEvents.ToList();
        if (uncommitted.Count == 0)
            return;

        var expectedStreamPosition = order.Version - uncommitted.Count;

        var envelopes = new List<EventEnvelope>(uncommitted.Count);
        var aggregateType = typeof(Order).FullName!;
        var aggregateId = order.Id;

        var nextStreamPosition = expectedStreamPosition + 1;

        foreach (var domainEvent in uncommitted)
        {
            var envelope = new EventEnvelope
            {
                EventId = domainEvent.EventId,
                AggregateId = aggregateId,
                AggregateType = aggregateType,
                AggregateVersion = order.Version,
                EventType = domainEvent.GetType().AssemblyQualifiedName!,
                Event = domainEvent,
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                StreamPosition = nextStreamPosition
            };

            nextStreamPosition++;
            envelopes.Add(envelope);
        }

        await _eventStore.AppendAsync(
            aggregateId,
            aggregateType,
            envelopes,
            expectedStreamPosition,
            cancellationToken);

        order.ClearUncommittedEvents();
    }
}
