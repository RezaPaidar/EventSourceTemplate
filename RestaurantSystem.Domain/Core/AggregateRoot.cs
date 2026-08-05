// Path: RestaurantSystem.Domain/Core/AggregateRoot.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RestaurantSystem.Domain.Core;

public abstract class AggregateRoot
{
    private readonly List<IEventSourcedEvent> _uncommittedEvents = new();
    public Guid Id { get; protected set; }
    // Version represents the last applied event index.
    // New aggregate starts with -1.
    public int Version { get; protected set; } = -1;

    public IReadOnlyCollection<IEventSourcedEvent> UncommittedEvents =>
        new ReadOnlyCollection<IEventSourcedEvent>(_uncommittedEvents);

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void RaiseEvent(IEventSourcedEvent @event)
    {
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
    }

    public void LoadFromHistory(IEnumerable<IEventSourcedEvent> history)
    {
        foreach (var e in history)
        {
            ApplyEvent(e, isFromHistory: true);
            Version++;
        }
    }

    protected abstract void When(IEventSourcedEvent @event);

    private void ApplyEvent(IEventSourcedEvent @event, bool isFromHistory = false)
    {
        When(@event);

        if (!isFromHistory)
        {
            Version++;
        }
    }
}