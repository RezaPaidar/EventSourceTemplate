using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order;
using RestaurantSystem.Infrastructure.Persistence;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.Persistence.Projections;
using RestaurantSystem.Infrastructure.Serialization;

namespace RestaurantSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
                               ?? throw new InvalidOperationException("Connection string 'Database' not found.");

        services.AddDbContext<EventStoreDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
                o.MigrationsHistoryTable("__EFMigrationsHistory_EventStore")));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
                o.MigrationsHistoryTable("__EFMigrationsHistory_Read")));

        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventStoreDbContext>());

        services.AddSingleton(new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        services.AddSingleton<ISerializer, EventSerializer>();

        services.AddScoped<IOrderProjector, OrderProjector>();
        services.AddScoped<IAggregateStore<Order>, OrderAggregateStore>();

        return services;
    }
}