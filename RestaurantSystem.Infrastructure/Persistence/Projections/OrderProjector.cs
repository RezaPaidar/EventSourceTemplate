using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order.Events;
using RestaurantSystem.Domain.Core;
using RestaurantSystem.Infrastructure.ReadModels;
using RestaurantSystem.Infrastructure.ReadModels.Models;

namespace RestaurantSystem.Infrastructure.Persistence.Projections;

public class OrderProjector(ReadDbContext dbContext) : IOrderProjector
{
    private readonly ReadDbContext _dbContext = dbContext;
    private async Task ApplyOrderStarted(OrderStarted e, CancellationToken ct)
    {
        var summary = new OrderSummaryReadModel
        {
            OrderId = e.OrderId,
            TableNumber = e.TableNumber,
            Status = "Started",
            TotalPrice = 0
        };

        _dbContext.OrderSummaries.Add(summary);
    }

    private async Task ApplyFoodItemAdded(FoodItemAdded e, CancellationToken ct)
    {
        var summary = await _dbContext.OrderSummaries.FindAsync([e.OrderId], ct);
        if (summary == null) return;

        var existingItem = await _dbContext.OrderItems
                .FirstOrDefaultAsync(x => x.OrderId == e.OrderId && x.MenuItemId == e.MenuItemId, ct);


        if (existingItem != null)
        {
            existingItem.Quantity += e.Quantity;
        }
        else
        {
            var item = new OrderItemReadModel
            {
                Id = Guid.NewGuid(),
                OrderId = e.OrderId,
                MenuItemId = e.MenuItemId,
                Name = e.Name,
                Price = e.Price,
                Quantity = e.Quantity
            };
            _dbContext.OrderItems.Add(item);
        }

        summary.TotalPrice += (e.Price * e.Quantity);
    }
    private async Task ApplyFoodItemRemoved(FoodItemRemoved e, CancellationToken ct)
    {
        var item = await _dbContext.OrderItems
            .FirstOrDefaultAsync(x => x.OrderId == e.OrderId && x.MenuItemId == e.MenuItemId, ct);

        if (item == null) return;

        var summary = await _dbContext.OrderSummaries.FindAsync([e.OrderId], ct);
        var quantityToRemove = Math.Min(item.Quantity, e.Quantity);

        if (summary != null)
        {
            summary.TotalPrice -= (item.Price * quantityToRemove);
        }

        if (item.Quantity <= e.Quantity)
        {
            _dbContext.OrderItems.Remove(item);
        }
        else
        {
            item.Quantity -= e.Quantity;
        }
    }
    private async Task ApplyOrderConfirmed(OrderConfirmed e, CancellationToken ct)
    {
        var summary = await _dbContext.OrderSummaries.FindAsync([e.OrderId], ct);
        if (summary != null) summary.Status = "Confirmed";
    }
    public async Task ProjectAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            switch (@event)
            {
                case OrderStarted e: await ApplyOrderStarted(e, cancellationToken); break;
                case FoodItemAdded e: await ApplyFoodItemAdded(e, cancellationToken); break;
                case FoodItemRemoved e: await ApplyFoodItemRemoved(e, cancellationToken); break;
                case OrderConfirmed e: await ApplyOrderConfirmed(e, cancellationToken); break;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
