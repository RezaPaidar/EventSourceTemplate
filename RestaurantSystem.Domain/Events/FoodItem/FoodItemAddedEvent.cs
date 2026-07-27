using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Events.FoodItem;

public sealed class FoodItemAddedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public Guid OrderId { get; init; }

    public Guid MenuItemId { get; init; }

    public string Name { get; init; } = default!;

    public decimal Price { get; init; }

    public int Quantity { get; init; }

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}