using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Orders.IntegrationEvents;

namespace RestaurantSystem.Infrastructure.Messaging.Kafka;

public sealed class IntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly Dictionary<string, Type> EventTypes = new()
    {
        { "order.food-item-added.v1", typeof(FoodItemAddedIntegrationEventV1) }
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
        var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(payload,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException("Failed to deserialize integration event envelope.");

        var integrationEvent = JsonSerializer.Deserialize(envelope.PayloadJson, eventType, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize integration event.");

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
