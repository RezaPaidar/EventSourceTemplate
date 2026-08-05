using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Orders.IntegrationEvents;

namespace RestaurantSystem.Application.Abstractions.Messaging.
Kafka;

public sealed class IntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly Dictionary<string, Type> EventTypes = new()
    {
        { "order.started.v1", typeof(OrderStartedIntegrationEventV1) },
        { "order.food-item-added.v1", typeof(FoodItemAddedIntegrationEventV1) },
        { "order.food-item-removed.v1", typeof(FoodItemRemovedIntegrationEventV1) },
        { "order.confirmed.v1", typeof(OrderConfirmedIntegrationEventV1) }
    };
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    public IntegrationEventDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task DispatchAsync(
        string messageType,
        string key,
        string payload,
        CancellationToken cancellationToken)
    {
        if (!EventTypes.TryGetValue(messageType, out var eventType))
        {
            throw new NotSupportedException($"Unknown message type: {messageType}");
        }
        var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(payload, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize integration event envelope.");

        var integrationEvent = JsonSerializer.Deserialize(envelope.PayloadJson, eventType, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize integration event.");


        using var scope = _scopeFactory.CreateScope();

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

        var handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(
            nameof(IIntegrationEventHandler<object>.HandleAsync))
            ?? throw new InvalidOperationException(
                $"HandleAsync was not found on {handlerType.Name}.");

        await (Task)method!.Invoke(handler, new object[] { integrationEvent, cancellationToken })!;
    }
}
