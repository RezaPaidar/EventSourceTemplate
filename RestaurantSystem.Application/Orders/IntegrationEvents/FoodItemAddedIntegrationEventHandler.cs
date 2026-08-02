using RestaurantSystem.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed class FoodItemAddedIntegrationEventHandler
    : IIntegrationEventHandler<FoodItemAddedIntegrationEventV1>
{
    private readonly ILogger<FoodItemAddedIntegrationEventHandler> _logger;

    public FoodItemAddedIntegrationEventHandler(
        ILogger<FoodItemAddedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(
        FoodItemAddedIntegrationEventV1 integrationEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Food item added integration event handled. EventId: {EventId}, OrderId: {OrderId}, MenuItemId: {MenuItemId}",
            integrationEvent.EventId,
            integrationEvent.OrderId,
            integrationEvent.MenuItemId);

        return Task.CompletedTask;
    }
}