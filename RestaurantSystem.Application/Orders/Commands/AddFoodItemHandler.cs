using MediatR;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Domain.Aggregates.OrderAggregate;

namespace RestaurantSystem.Application.Orders.Commands;

public class AddFoodItemHandler : IRequestHandler<AddFoodItemCommand>
{
    private readonly IEventStore _eventStore;
    private readonly IUnitOfWork _unitOfWork;

    public AddFoodItemHandler(IEventStore eventStore, IUnitOfWork unitOfWork)
    {
        _eventStore = eventStore;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddFoodItemCommand command, CancellationToken ct)
    {
        // 1. Load Event Stream
        var events = await _eventStore.LoadEventsAsync(command.OrderId, ct);

        // 2. Rehydrate Aggregate
        var order = new Order();
        order.LoadFromHistory(events);

        // 3. Execute Domain Behavior
        order.AddItem(command.MenuItemId, command.Name, command.Price, command.Quantity);

        // 4. Save to Store
        var uncommittedEvents = order.GetUncommittedEvents().ToList();
        var expectedVersion = order.Version - uncommittedEvents.Count;
        
        await _eventStore.SaveEventsAsync(
            order.Id,
            nameof(Order),
            uncommittedEvents,
            expectedVersion,
            ct);
        
        // 5. Commit Transaction
        await _unitOfWork.SaveChangesAsync(ct);

        // 6. Clear uncommitted events
        order.ClearUncommittedEvents();
    }
}