using RestaurantSystem.Application.ReadModels.Order;

namespace RestaurantSystem.Application.ReadModels;

public class OrderReadModel
{
    public Guid Id { get; init; }
    public int TableNumber { get; init; }
    public string Status { get; set; } = "Started";
    public List<OrderItemReadModel> Items { get; init; } = new();
}