using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Domain.Aggregates.Order.Events;

public sealed record OrderItemQuantityCorrected(
    Guid EventId,
    Guid OrderId,
    Guid MenuItemId,
    int NewQuantity,
    Guid OriginalEventId,
    string Reason,
    DateTime OccurredOnUtc) : IEventSourcedEvent
{
    public int EventVersion => 1;
}
