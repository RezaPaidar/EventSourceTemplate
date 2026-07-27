namespace RestaurantSystem.Domain.Core;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc  { get; }
}
