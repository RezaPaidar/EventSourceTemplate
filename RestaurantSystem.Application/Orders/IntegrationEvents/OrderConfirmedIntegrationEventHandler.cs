using Microsoft.Extensions.Logging;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;

namespace RestaurantSystem.Application.Orders.IntegrationEvents;

public sealed class OrderConfirmedIntegrationEventHandler
    : IIntegrationEventHandler<OrderConfirmedIntegrationEventV1>
{
    private readonly IOrderProjector _orderProjector;
    private readonly ILogger<OrderConfirmedIntegrationEventHandler> _logger;

    public OrderConfirmedIntegrationEventHandler(
        IOrderProjector orderProjector,
        ILogger<OrderConfirmedIntegrationEventHandler> logger)
    {
        _orderProjector = orderProjector;
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderConfirmedIntegrationEventV1 integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var domainEvent = new OrderConfirmed
        {
            EventId = integrationEvent.EventId,
            OrderId = integrationEvent.OrderId,
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };

        await _orderProjector.ProjectAsync(new[] { domainEvent }, cancellationToken);
    }
}
