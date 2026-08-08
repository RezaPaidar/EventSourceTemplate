namespace RestaurantSystem.Application.Orders.Queries.GetOrderById;

public interface IOrderReadStore
{
    Task<OrderSummaryDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
}
