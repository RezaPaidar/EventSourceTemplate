using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Api.Contracts.FoodItems;
using RestaurantSystem.Api.Contracts.Orders;
using RestaurantSystem.Application.Orders.Commands;
using RestaurantSystem.Application.Orders.Commands.CorrectOrder;
using RestaurantSystem.Application.Orders.Queries.GetOrderById;

namespace RestaurantSystem.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartOrder(
        [FromBody] StartOrderRequest request,
        CancellationToken ct)
    {
        var orderId = Guid.NewGuid();

        var command = new StartOrderCommand(
            orderId,
            request.CustomerId);

        await _mediator.Send(command, ct);

        return Accepted(new { OrderId = orderId });
    }

    [HttpPost("{orderId:guid}/items")]
    public async Task<IActionResult> AddFoodItem(
        Guid orderId,
        [FromBody] AddFoodItemRequest request,
        CancellationToken ct)
    {
        var command = new AddFoodItemCommand(
            orderId,
            request.MenuItemId,
            request.Name,
            request.Price,
            request.Quantity);

        await _mediator.Send(command, ct);

        return Accepted();
    }

    [HttpPost("{orderId:guid}/confirm")]
    public async Task<IActionResult> ConfirmOrder(
        Guid orderId,
        CancellationToken ct)
    {
        var command = new ConfirmOrderCommand(orderId);

        await _mediator.Send(command, ct);

        return Accepted();
    }

    [HttpPost("{orderId:guid}/items/remove")]
    public async Task<IActionResult> RemoveFoodItem(
        Guid orderId,
        [FromBody] RemoveFoodItemRequest request,
        CancellationToken ct)
    {
        var command = new RemoveFoodItemCommand(
            orderId,
            request.MenuItemId,
            request.Quantity);

        await _mediator.Send(command, ct);

        return Accepted();
    }

    [HttpPut("{orderId:guid}/items/{menuItemId:guid}/quantity")]
    public async Task<IActionResult> CorrectItemQuantity(
    [FromRoute] Guid orderId,
    [FromRoute] Guid menuItemId,
    [FromBody] CorrectOrderRequest request,
    CancellationToken cancellationToken)
    {
        await _mediator.Send(new CorrectOrderCommand(
            orderId,
            menuItemId,
            request.NewQuantity,
            request.OriginalEventId,
            request.Reason), cancellationToken);

        return NoContent();
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(
       Guid orderId,
       CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
