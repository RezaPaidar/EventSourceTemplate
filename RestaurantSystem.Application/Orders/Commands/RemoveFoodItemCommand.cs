using MediatR;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed record RemoveFoodItemCommand(
    Guid OrderId,
    Guid MenuItemId,
    int Quantity
) : IRequest;
