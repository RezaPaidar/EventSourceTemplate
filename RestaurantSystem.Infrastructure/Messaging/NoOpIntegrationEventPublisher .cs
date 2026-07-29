using RestaurantSystem.Application.Abstractions.Messaging;

namespace RestaurantSystem.Infrastructure.Messaging;

public sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken ct)
        => Task.CompletedTask;
}
