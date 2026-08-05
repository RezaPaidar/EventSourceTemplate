using MediatR;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed record ConfirmOrderCommand(
    Guid OrderId
) : IRequest;
