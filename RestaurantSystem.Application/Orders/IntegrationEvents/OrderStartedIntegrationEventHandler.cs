using Microsoft.Extensions.Logging;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;

namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed class OrderStartedIntegrationEventHandler
    : IIntegrationEventHandler<OrderStartedIntegrationEventV1>
{
    private readonly IOrderProjector _orderProjector;
    private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

    public OrderStartedIntegrationEventHandler(
        IOrderProjector orderProjector,
        ILogger<OrderStartedIntegrationEventHandler> logger)
    {
        _orderProjector = orderProjector;
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderStartedIntegrationEventV1 integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var domainEvent = new OrderStarted
        {
            EventId = integrationEvent.EventId,
            OrderId = integrationEvent.OrderId,
            TableNumber = integrationEvent.TableNumber,
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };

        await _orderProjector.ProjectAsync(new[] { domainEvent }, cancellationToken);
    }
}

