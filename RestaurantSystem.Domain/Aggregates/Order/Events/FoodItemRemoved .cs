using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public sealed record FoodItemRemoved : IEventSourcedEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public Guid MenuItemId { get; init; }
    public DateTime OccurredOnUtc { get; init; }
    public int Quantity { get; set; }
    public int EventVersion => 1;
}
