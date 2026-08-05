using Microsoft.Extensions.Logging;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;

namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed class FoodItemRemovedIntegrationEventHandler
    : IIntegrationEventHandler<FoodItemRemovedIntegrationEventV1>
{
    private readonly IOrderProjector _orderProjector;
    private readonly ILogger<FoodItemRemovedIntegrationEventHandler> _logger;

    public FoodItemRemovedIntegrationEventHandler(
        IOrderProjector orderProjector,
        ILogger<FoodItemRemovedIntegrationEventHandler> logger)
    {
        _orderProjector = orderProjector;
        _logger = logger;
    }

    public async Task HandleAsync(
        FoodItemRemovedIntegrationEventV1 integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var domainEvent = new FoodItemRemoved
        {
            EventId = integrationEvent.EventId,
            OrderId = integrationEvent.OrderId,
            MenuItemId = integrationEvent.MenuItemId,
            Quantity = integrationEvent.Quantity,
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };

        await _orderProjector.ProjectAsync(new[] { domainEvent }, cancellationToken);
    }
}
