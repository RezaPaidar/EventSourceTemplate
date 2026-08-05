using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSystem.Infrastructure.ReadStore.Models;

namespace RestaurantSystem.Infrastructure.ReadStore.Configurations;

public sealed class OrderProjectionCheckpointConfiguration : IEntityTypeConfiguration<OrderProjectionCheckpoint>
{
    public void Configure(EntityTypeBuilder<OrderProjectionCheckpoint> builder)
    {
        builder.ToTable("order_projection_checkpoints", "ReadStore");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProjectorName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.ProjectorName)
            .IsUnique();
    }
}
