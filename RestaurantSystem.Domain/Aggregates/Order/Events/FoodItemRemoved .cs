using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public record FoodItemRemoved : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public Guid MenuItemId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}