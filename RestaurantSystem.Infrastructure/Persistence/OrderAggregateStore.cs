
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order;

namespace RestaurantSystem.Infrastructure.Persistence;

public class OrderAggregateStore(IEventStore eventStore, IOrderProjector projector) : IAggregateStore<Order>
{
    public async Task SaveAsync(Order aggregate, CancellationToken ct = default)
    {
        var events = aggregate.GetUncommittedEvents().ToList();
        if (!events.Count.Equals(0))
        {
            await eventStore.SaveEventsAsync(
                aggregate.Id,
                nameof(Order),
                events,
                aggregate.Version,
                ct);

            await projector.ProjectAsync(events, ct);

            aggregate.ClearUncommittedEvents();
        }
    }
    public async Task<Order> LoadAsync(Guid id, CancellationToken ct = default)
    {
        var events = await eventStore.LoadEventsAsync(id, ct);
        return Order.LoadFromHistory(id, events.ToList());
    }

}
