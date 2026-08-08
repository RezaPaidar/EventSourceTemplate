using MediatR;

namespace RestaurantSystem.Application.Orders.Commands.CorrectOrder;

public sealed record CorrectOrderCommand(
    Guid OrderId,
    Guid MenuItemId,
    int NewQuantity,
    Guid OriginalEventId,
    string Reason) : IRequest;
