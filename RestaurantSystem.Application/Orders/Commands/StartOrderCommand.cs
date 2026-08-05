using MediatR;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed record StartOrderCommand(
    Guid OrderId,
    int CustomerId
) : IRequest;
