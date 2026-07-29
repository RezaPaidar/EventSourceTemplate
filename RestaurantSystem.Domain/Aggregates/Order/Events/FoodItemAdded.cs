using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public record FoodItemAdded : IDomainEvent
{
    public Guid EventId { get; init; }
    public Guid OrderId { get; init; }
    public Guid MenuItemId { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}