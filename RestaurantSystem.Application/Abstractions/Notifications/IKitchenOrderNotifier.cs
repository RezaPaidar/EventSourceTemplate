namespace RestaurantSystem.Application.Abstractions.Notifications;

public interface IKitchenOrderNotifier
{
    Task NotifyOrderConfirmedAsync(Guid orderId, CancellationToken cancellationToken = default);
}