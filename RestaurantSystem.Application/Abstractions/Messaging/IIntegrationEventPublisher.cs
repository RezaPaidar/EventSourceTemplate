namespace RestaurantSystem.Application.Abstractions.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct);
}