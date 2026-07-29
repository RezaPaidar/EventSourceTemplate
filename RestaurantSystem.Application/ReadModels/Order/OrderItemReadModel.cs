namespace RestaurantSystem.Application.ReadModels.Order
{
public class OrderItemReadModel
{
    public Guid MenuItemId { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}
}