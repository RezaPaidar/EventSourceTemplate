using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Infrastructure.Persistence.Models;

namespace RestaurantSystem.Infrastructure.Persistence;

public sealed class EventStoreDbContext : DbContext
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

            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.AggregateId).IsRequired();
            builder.Property(x => x.AggregateType).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.Metadata).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.OccurredOnUtc).IsRequired();

            builder.HasIndex(x => x.Id).IsUnique();
            builder.HasIndex(x => new { x.AggregateId, x.Version }).IsUnique();
            builder.HasIndex(x => new { x.AggregateId, x.OccurredOnUtc });
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