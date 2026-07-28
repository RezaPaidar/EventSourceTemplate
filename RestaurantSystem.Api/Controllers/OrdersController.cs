using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestaurantSystem.Api.Contracts.FoodItems;
using RestaurantSystem.Application.Orders.Commands;

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
}

