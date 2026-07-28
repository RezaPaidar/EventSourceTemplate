namespace RestaurantSystem.Infrastructure.ReadModels.Models;

public class OrderItemReadModel
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public OrderSummaryReadModel Order { get; set; } = default!;
}