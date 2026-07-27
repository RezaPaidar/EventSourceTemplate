// This ensures atomic commit of Events and Outbox in Infrastructure
namespace RestaurantSystem.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}