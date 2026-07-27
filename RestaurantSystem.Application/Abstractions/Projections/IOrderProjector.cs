using RestaurantSystem.Domain.Core;

namespace RestaurantSystem.Application.Abstractions.Projections;

public interface IOrderProjector
{
    Task ProjectAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}