using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Application.Abstractions.Persistence
{
    public interface IAggregateStore<T> where T : AggregateRoot
    {
        Task SaveAsync(T aggregate, CancellationToken cancellationToken = default);
        Task<T> LoadAsync(Guid id, CancellationToken cancellationToken = default);
    }
}