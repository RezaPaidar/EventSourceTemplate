using MediatR;

namespace RestaurantSystem.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderSummaryDto?>
{
    private readonly IOrderReadStore _readStore;

    public GetOrderByIdQueryHandler(IOrderReadStore readStore)
    {
        _readStore = readStore;
    }

    public Task<OrderSummaryDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Delegates read access to the application abstraction.
        return _readStore.GetByIdAsync(request.OrderId, cancellationToken);
    }
}
