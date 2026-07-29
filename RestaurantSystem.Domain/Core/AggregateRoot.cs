// Path: RestaurantSystem.Domain/Core/AggregateRoot.cs
using System.Collections.Generic;

namespace RestaurantSystem.Domain.Core;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    // Version represents the last applied event index.
    // New aggregate starts with -1.
    public int Version { get; protected set; } = -1;

    private readonly List<IDomainEvent> _uncommittedEvents = new();

    public IEnumerable<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void RaiseEvent(IDomainEvent @event)
    {
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
        Version++;
    }

    public void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            ApplyEvent(@event);
            Version++;
        }
    }

    protected abstract void ApplyEvent(IDomainEvent @event);
}