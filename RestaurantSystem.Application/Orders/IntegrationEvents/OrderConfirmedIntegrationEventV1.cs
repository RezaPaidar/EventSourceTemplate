
namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed record OrderConfirmedIntegrationEventV1
(
    Guid EventId,
    Guid OrderId,
    DateTime OccurredOnUtc
);