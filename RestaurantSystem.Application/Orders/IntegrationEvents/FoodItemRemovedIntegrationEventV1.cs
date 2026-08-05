
namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed record FoodItemRemovedIntegrationEventV1(
    Guid EventId,
    Guid OrderId,
    Guid MenuItemId,
    int Quantity,
    DateTime OccurredOnUtc
);