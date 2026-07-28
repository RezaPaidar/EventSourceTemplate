using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Configurations;

public class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        // 1. Table and Schema
        builder.ToTable("StoredEvents", "EventStore");

        // 2. Primary Key
        builder.HasKey(e => e.Id);

        // 3. Properties Configuration
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(e => e.EventId)
            .IsRequired();
        
        builder.Property(e => e.AggregateId)
            .IsRequired();

        // Consistent naming: AggregateType (based on your model)
        builder.Property(e => e.AggregateType)
            .HasMaxLength(128)
            .IsRequired();
        
        builder.Property(e => e.EventType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Version)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("NOW()") // Assuming Postgres for automatic timestamping
            .IsRequired();

        // 4. JSON Data Storage (using PostgreSQL JSONB type for performance)
        builder.Property(e => e.Data)
            .HasColumnType("jsonb")
            .IsRequired();
        
        // Metadata can also be stored as JSONB if it contains structured data
        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb") // Storing metadata as JSONB for flexibility
            .IsRequired();

        // 5. Indexing for efficient Aggregate loading
        // Index on AggregateId and Version ensures fast loading of all events for a specific aggregate
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique();

        // Optional: Index for reporting/filtering
        builder.HasIndex(e => e.AggregateType);
        builder.HasIndex(e => e.EventType);
    }
}
