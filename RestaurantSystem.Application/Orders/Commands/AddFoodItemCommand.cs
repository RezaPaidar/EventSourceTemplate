using MediatR;

namespace RestaurantSystem.Application.Orders.Commands;


public sealed record AddFoodItemCommand(
    Guid OrderId,
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity) : IRequest;