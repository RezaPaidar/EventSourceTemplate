namespace RestaurantSystem.Domain.Core;

public interface IEventSourcedEvent : IDomainEvent
{
    int EventVersion { get; }
}
