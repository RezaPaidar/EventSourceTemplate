using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSystem.Infrastructure.ReadStore.Models;

namespace RestaurantSystem.Infrastructure.ReadStore.Configurations;

public sealed class OrderProjectionProcessedEventConfiguration : IEntityTypeConfiguration<OrderProjectionProcessedEvent>
{
    public void Configure(EntityTypeBuilder<OrderProjectionProcessedEvent> builder)
    {
        builder.ToTable("order_projection_processed_events", "ReadStore");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectorName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => new { x.ProjectorName, x.EventId })
            .IsUnique();
    }
}
