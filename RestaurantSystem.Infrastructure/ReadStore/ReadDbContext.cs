using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Infrastructure.ReadStore.Models;

namespace RestaurantSystem.Infrastructure.ReadStore;

public class ReadDbContext : DbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
    }

    public DbSet<OrderSummaryReadModel> OrderSummaries => Set<OrderSummaryReadModel>();
    public DbSet<OrderItemReadModel> OrderItems => Set<OrderItemReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderSummaryReadModel>(entity =>
        {
            entity.ToTable("order_summaries");
            entity.HasKey(x => x.OrderId);

            entity.Property(x => x.Status).IsRequired().HasMaxLength(50);
            entity.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)");

            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemReadModel>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Price).HasColumnType("numeric(18,2)");

            entity.HasIndex(x => new { x.OrderId, x.MenuItemId });
        });
    }
}