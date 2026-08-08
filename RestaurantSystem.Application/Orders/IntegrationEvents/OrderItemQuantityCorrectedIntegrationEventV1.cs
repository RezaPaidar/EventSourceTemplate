namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public record OrderItemQuantityCorrectedIntegrationEventV1(
    Guid OrderId,
    Guid MenuItemId,
    int NewQuantity,
    string Reason);
