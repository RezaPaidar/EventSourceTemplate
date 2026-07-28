using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence;
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
        modelBuilder.Entity<StoredEvent>(builder =>
        {
            builder.ToTable("stored_events");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventId).IsRequired();
            builder.Property(x => x.AggregateId).IsRequired();
            builder.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.Data).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.Metadata).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => x.EventId).IsUnique();
            builder.HasIndex(x => new { x.AggregateId, x.Version }).IsUnique();
            builder.HasIndex(x => new { x.AggregateId, x.CreatedAt });
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.OccurredOnUtc).IsRequired();

            builder.HasIndex(x => x.ProcessedOnUtc);
            builder.HasIndex(x => x.OccurredOnUtc);
        });
    }
}
