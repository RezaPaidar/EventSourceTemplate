namespace RestaurantSystem.Application.Abstractions.Messaging;

public interface IIntegrationEventDispatcher
{
    Task DispatchAsync(
        string messageType,
        string key,
        string payload,
        CancellationToken cancellationToken);
}