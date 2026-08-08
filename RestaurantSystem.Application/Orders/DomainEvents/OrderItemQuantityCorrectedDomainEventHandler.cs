using RestaurantSystem.Application.Abstractions.Events;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Domain.Aggregates.Order.Events;
using RestaurantSystem.Application.Orders.IntegrationEvents;
using System.Text.Json;

namespace RestaurantSystem.Application.Orders.DomainEvents;

public class OrderItemQuantityCorrectedDomainEventHandler(
    IIntegrationEventPublisher publisher)
    : IDomainEventHandler<OrderItemQuantityCorrected>
{
    public async Task HandleAsync(OrderItemQuantityCorrected notification, CancellationToken ct)
    {
        // Transform domain event to integration event
        var integrationEvent = new OrderItemQuantityCorrectedIntegrationEventV1(
            notification.OrderId,
            notification.MenuItemId,
            notification.NewQuantity,
            notification.Reason);

        var envelope = new IntegrationEventEnvelope(
            MessageId: Guid.NewGuid(),
            Type: nameof(OrderItemQuantityCorrectedIntegrationEventV1),
            PayloadJson: JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc: notification.OccurredOnUtc);

        await publisher.PublishAsync(envelope, ct);
    }
}
