using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;

namespace RestaurantSystem.Infrastructure.Persistence.EventStore.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "EventStore");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();

        builder.Property(x => x.ProcessedOnUtc)
            .IsRequired(false);

        builder.Property(x => x.Error)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(e => e.Headers)
        .HasColumnType("jsonb")
        .IsRequired(false);

        builder.HasIndex(x => x.ProcessedOnUtc);
        builder.HasIndex(x => x.OccurredOnUtc);
        builder.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
    }
}