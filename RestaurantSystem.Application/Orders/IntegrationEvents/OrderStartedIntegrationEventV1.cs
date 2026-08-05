
namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed record OrderStartedIntegrationEventV1(
    Guid EventId,
    Guid OrderId,
    int TableNumber,
    DateTime OccurredOnUtc
);