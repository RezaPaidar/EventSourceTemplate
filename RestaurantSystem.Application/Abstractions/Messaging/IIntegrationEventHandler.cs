namespace RestaurantSystem.Application.Abstractions.Messaging;

public interface IIntegrationEventHandler<in TIntegrationEvent>
{
    Task HandleAsync(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken);
}