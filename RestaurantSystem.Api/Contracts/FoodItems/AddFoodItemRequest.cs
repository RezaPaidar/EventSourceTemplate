namespace RestaurantSystem.Api.Contracts.FoodItems;

public sealed record AddFoodItemRequest(
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity);