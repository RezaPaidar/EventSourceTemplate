namespace RestaurantSystem.Application.Abstractions.Orders;

using RestaurantSystem.Domain.Aggregates.Order;

public interface IOrderAggregateStore
{
    Task<Order?> LoadAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);
}
