// RestaurantSystem.Infrastructure/ReadStore/OrderReadStore.cs
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Orders.Queries.GetOrderById;

namespace RestaurantSystem.Infrastructure.ReadStore;

// Order read-side adapter over ReadDbContext
public class OrderReadStore : IOrderReadStore
{
    private readonly ReadDbContext _context;

    public OrderReadStore(ReadDbContext context)
    {
        _context = context;
    }

    public async Task<OrderSummaryDto?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        // Load order summary with items from read store
        var summary = await _context.OrderSummaries
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

        if (summary is null)
        {
            return null;
        }

        // Map EF read model to Application DTO
        return new OrderSummaryDto
        {
            OrderId = summary.OrderId,
            TableNumber = summary.TableNumber,
            Status = summary.Status,
            TotalPrice = summary.TotalPrice,
            LastUpdatedAt = summary.LastUpdatedAt,
            Items = summary.Items
                .Select(item => new OrderItemDto
                {
                    MenuItemId = item.MenuItemId,
                    Name = item.Name,
                    Price = item.Price
                    // Quantity is not part of DTO by current design
                })
                .ToList()
        };
    }
}
