namespace RestaurantSystem.Infrastructure.ReadModels.Models;

public class OrderSummaryReadModel
{
    public Guid OrderId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = "Started";
    public decimal TotalPrice { get; set; }
    public DateTime LastUpdatedAt { get; set; }

    public List<OrderItemReadModel> Items { get; set; } = new();
}