using Microsoft.Extensions.Logging;
using RestaurantSystem.Application.Abstractions.Notifications;

namespace RestaurantSystem.Infrastructure.Notifications;

public sealed class KitchenOrderNotifier : IKitchenOrderNotifier
{
    private readonly ILogger<KitchenOrderNotifier> _logger;

    public KitchenOrderNotifier(ILogger<KitchenOrderNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyOrderConfirmedAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} confirmed. Kitchen notification placeholder executed.", orderId);
        return Task.CompletedTask;
    }
}
