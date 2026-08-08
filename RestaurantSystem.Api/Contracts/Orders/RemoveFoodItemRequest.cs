
namespace RestaurantSystem.Api.Contracts.Orders;

public sealed record RemoveFoodItemRequest(Guid MenuItemId, int Quantity);