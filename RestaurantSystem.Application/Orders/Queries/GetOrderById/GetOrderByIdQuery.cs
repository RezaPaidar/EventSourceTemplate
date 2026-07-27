using MediatR;

namespace RestaurantSystem.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderSummaryDto?>;