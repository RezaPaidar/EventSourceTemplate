using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence.EventStore;
// DbContext for the write side, responsible for persisting domain events and outbox messages.
public sealed class EventStoreDbContext : DbContext, IUnitOfWork
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            assembly: typeof(EventStoreDbContext).Assembly,
            predicate: type => type.Namespace == "RestaurantSystem.Infrastructure.Persistence.EventStore.Configurations");

        base.OnModelCreating(modelBuilder);
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
