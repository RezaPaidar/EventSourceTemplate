namespace RestaurantSystem.Infrastructure.Messaging.Kafka;

public interface IIntegrationEventDispatcher
{
    // Inputs are raw Kafka payload details
    Task DispatchAsync(
        string messageType,
        string key,
        string payload,
        CancellationToken cancellationToken);
}