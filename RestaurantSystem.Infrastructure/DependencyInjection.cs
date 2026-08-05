using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Domain.Aggregates.Order;
using RestaurantSystem.Infrastructure.Messaging.Kafka;
using RestaurantSystem.Infrastructure.Messaging.Kafka.Consumers;
using RestaurantSystem.Infrastructure.Messaging.Outbox;
using RestaurantSystem.Infrastructure.Persistence;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.ReadStore;
using RestaurantSystem.Infrastructure.Serialization;

namespace RestaurantSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
                               ?? throw new InvalidOperationException("Connection string 'Database' not found.");

        services.AddDbContext<EventStoreDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
                o.MigrationsHistoryTable("__EFMigrationsHistory", "EventStore")));


        services.AddDbContext<ReadDbContext>(options =>
            options.UseNpgsql(connectionString, o =>
                o.MigrationsHistoryTable("__EFMigrationsHistory_Read", "ReadStore")));

        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventStoreDbContext>());

        services.AddSingleton(new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        services.AddSingleton<ISerializer, EventSerializer>();

        services.AddScoped<IOrderProjector, RestaurantSystem.Infrastructure.Persistence.Projections.OrderProjector>();
        services.AddScoped<IAggregateStore<Order>, OrderAggregateStore>();

        services.Configure<OutboxDispatcherOptions>(
            configuration.GetSection(nameof(OutboxDispatcherOptions)));
        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();
        services.AddHostedService<OutboxDispatcherBackgroundService>();

        services.Configure<KafkaPublisherOptions>(configuration.GetSection(nameof(KafkaPublisherOptions)));
        services.AddScoped<IIntegrationEventPublisher, KafkaEventPublisher>();

        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["KafkaPublisherOptions:BootstrapServers"],
                GroupId = configuration["KafkaPublisherOptions:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            return new ConsumerBuilder<string, string>(config).Build();
        });
        services.AddHostedService<OrderIntegrationEventConsumer>();

        return services;
    }
}