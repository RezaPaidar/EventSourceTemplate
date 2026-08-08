using MediatR;
using RestaurantSystem.Application.Abstractions.Events;
using RestaurantSystem.Application.Abstractions.Orders;

namespace RestaurantSystem.Application.Orders.Commands;

public sealed class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand>
{
    private readonly IOrderAggregateStore _orderAggregateStore;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public ConfirmOrderHandler(IOrderAggregateStore orderAggregateStore, IDomainEventDispatcher domainEventDispatcher)
    {
        _orderAggregateStore = orderAggregateStore;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task Handle(ConfirmOrderCommand command, CancellationToken ct)
    {
        // 1. Load & Rehydrate
        var order = await _orderAggregateStore.LoadAsync(command.OrderId, ct);
        if (order is null)
            throw new InvalidOperationException($"Order '{command.OrderId}' not found.");

        // 2. Domain Logic
        var occurredOnUtc = DateTime.UtcNow;
        order.Confirm(occurredOnUtc);

        // 3. Persist
        await _orderAggregateStore.SaveAsync(order, ct);

        //4. Distributes Domain-Events (for side-effects)
        if (order.DomainEvents.Any())
        {
            await _domainEventDispatcher.DispatchAsync(order.DomainEvents, ct);

            //5. Clear list to prevent reprocessing
            order.ClearDomainEvents();
        }
    }
}
