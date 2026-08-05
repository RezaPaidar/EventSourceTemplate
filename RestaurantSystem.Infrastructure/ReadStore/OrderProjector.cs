using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;
using RestaurantSystem.Domain.Core;
using RestaurantSystem.Infrastructure.ReadStore.Models;

namespace RestaurantSystem.Infrastructure.ReadStore;

public class OrderProjector : IOrderProjector
{
    private readonly ReadDbContext _readDbContext;
    private const string ProjectorName = "OrderProjector";
    public OrderProjector(ReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task ProjectAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        using var transaction = await _readDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var domainEvent in events)
            {
                var alreadyProcessed = await _readDbContext.ProcessedProjectionEvents
                        .AnyAsync(x => x.ProjectorName == ProjectorName && x.EventId == domainEvent.EventId, cancellationToken);

                if (alreadyProcessed) continue;

                await ApplyEvent(domainEvent, cancellationToken);

                _readDbContext.ProcessedProjectionEvents.Add(new OrderProjectionProcessedEvent
                {
                    Id = Guid.NewGuid(),
                    ProjectorName = ProjectorName,
                    EventId = domainEvent.EventId,
                    ProcessedAtUtc = DateTime.UtcNow
                });
            }

            await _readDbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

    }
    private async Task ApplyEvent(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        switch (domainEvent)
        {
            case OrderStarted e: await Apply(e, cancellationToken); break;
            case FoodItemAdded e: await Apply(e, cancellationToken); break;
            case FoodItemRemoved e: await Apply(e, cancellationToken); break;
            case OrderConfirmed e: await Apply(e, cancellationToken); break;
        }
    }
    private async Task Apply(FoodItemAdded @event, CancellationToken cancellationToken)
    {
        var summary = await _readDbContext.OrderSummaries
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (summary is null)
        {
            throw new InvalidOperationException("Order summary was not found for projection.");
        }

        var item = summary.Items.FirstOrDefault(x => x.MenuItemId == @event.MenuItemId);

        if (item is null)
        {
            summary.Items.Add(new OrderItemReadModel
            {
                Id = Guid.NewGuid(),
                OrderId = @event.OrderId,
                MenuItemId = @event.MenuItemId,
                Name = @event.Name,
                Price = @event.Price,
                Quantity = @event.Quantity
            });
        }
        else
        {
            item.Quantity += @event.Quantity;
        }

        summary.TotalPrice += @event.Price * @event.Quantity;
        summary.LastUpdatedAt = @event.OccurredOnUtc;
    }


    private async Task Apply(FoodItemRemoved @event, CancellationToken cancellationToken)
    {
        var summary = await _readDbContext.OrderSummaries
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (summary is null)
        {
            throw new InvalidOperationException("Order summary was not found for projection.");
        }

        var item = summary.Items.FirstOrDefault(x => x.MenuItemId == @event.MenuItemId);
        if (item is null)
        {
            return;
        }

        var removedQuantity = Math.Min(item.Quantity, @event.Quantity);

        item.Quantity -= removedQuantity;
        summary.TotalPrice -= item.Price * removedQuantity;
        summary.LastUpdatedAt = @event.OccurredOnUtc;

        if (item.Quantity == 0)
        {
            _readDbContext.OrderItems.Remove(item);
        }
    }


    private async Task Apply(OrderStarted @event, CancellationToken cancellationToken)
    {
        var exists = await _readDbContext.OrderSummaries
            .AnyAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (exists)
        {
            return;
        }

        var summary = new OrderSummaryReadModel
        {
            OrderId = @event.OrderId,
            TableNumber = @event.TableNumber,
            Status = "Started",
            TotalPrice = 0,
            LastUpdatedAt = @event.OccurredOnUtc
        };

        await _readDbContext.OrderSummaries.AddAsync(summary, cancellationToken);
    }
    private async Task Apply(OrderConfirmed @event, CancellationToken cancellationToken)
    {
        var summary = await _readDbContext.OrderSummaries
            .FirstOrDefaultAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (summary is null)
        {
            throw new InvalidOperationException("Order summary was not found for projection.");
        }

        if (summary.Status == "Confirmed")
        {
            summary.LastUpdatedAt = @event.OccurredOnUtc;
            return;
        }

        summary.Status = "Confirmed";
        summary.LastUpdatedAt = @event.OccurredOnUtc;
    }

}
