namespace RestaurantSystem.Application.Orders.Queries.GetOrderById;

public class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}