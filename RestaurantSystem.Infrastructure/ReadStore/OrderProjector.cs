using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;
using RestaurantSystem.Domain.Core;
using RestaurantSystem.Infrastructure.ReadStore.Models;

namespace RestaurantSystem.Infrastructure.ReadStore;

public class OrderProjector : IOrderProjector
{
    private readonly ReadDbContext _readDbContext;

    public OrderProjector(ReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task ProjectAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            switch (domainEvent)
            {
                case OrderStarted orderStarted:
                    await Apply(orderStarted, cancellationToken);
                    break;

                case FoodItemAdded foodItemAdded:
                    await Apply(foodItemAdded, cancellationToken);
                    break;

                case FoodItemRemoved foodItemRemoved:
                    await Apply(foodItemRemoved, cancellationToken);
                    break;

                case OrderConfirmed orderConfirmed:
                    await Apply(orderConfirmed, cancellationToken);
                    break;
            }
        }

        await _readDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(OrderStarted @event, CancellationToken cancellationToken)
    {
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

    private async Task Apply(FoodItemAdded @event, CancellationToken cancellationToken)
    {
        var summary = await _readDbContext.OrderSummaries
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (summary is null)
        {
            throw new InvalidOperationException("Order summary was not found for projection.");
        }

        summary.Items.Add(new OrderItemReadModel
        {
            Id = Guid.NewGuid(),
            OrderId = @event.OrderId,
            MenuItemId = @event.MenuItemId,
            Name = @event.Name,
            Price = @event.Price
        });

        summary.TotalPrice += @event.Price;
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

        summary.TotalPrice -= item.Price;
        summary.LastUpdatedAt = @event.OccurredOnUtc;

        _readDbContext.OrderItems.Remove(item);
    }

    private async Task Apply(OrderConfirmed @event, CancellationToken cancellationToken)
    {
        var summary = await _readDbContext.OrderSummaries
            .FirstOrDefaultAsync(x => x.OrderId == @event.OrderId, cancellationToken);

        if (summary is null)
        {
            throw new InvalidOperationException("Order summary was not found for projection.");
        }

        summary.Status = "Confirmed";
        summary.LastUpdatedAt = @event.OccurredOnUtc;
    }
}
