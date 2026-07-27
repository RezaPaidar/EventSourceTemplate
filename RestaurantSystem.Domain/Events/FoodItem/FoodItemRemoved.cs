using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events.FoodItem;


public sealed class FoodItemRemoved : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public Guid MenuItemId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}