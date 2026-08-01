namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed record FoodItemAddedIntegrationEventV1(
    Guid EventId,
    Guid OrderId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity,
    DateTime OccurredOnUtc);