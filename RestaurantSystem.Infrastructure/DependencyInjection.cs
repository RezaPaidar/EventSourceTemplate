using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantSystem.Application.Abstractions.Events;
using RestaurantSystem.Application.Abstractions.Messaging;
using RestaurantSystem.Application.Abstractions.Messaging.Kafka;
using RestaurantSystem.Application.Abstractions.Notifications;
using RestaurantSystem.Application.Abstractions.Orders;
using RestaurantSystem.Application.Abstractions.Persistence;
using RestaurantSystem.Application.Abstractions.Projections;
using RestaurantSystem.Application.Orders.Queries.GetOrderById;
using RestaurantSystem.Infrastructure.Events;
using RestaurantSystem.Infrastructure.Messaging.Kafka;
using RestaurantSystem.Infrastructure.Messaging.Kafka.Consumers;
using RestaurantSystem.Infrastructure.Messaging.Outbox;
using RestaurantSystem.Infrastructure.Notifications;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.Persistence.Projections;
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

        services.AddScoped<RestaurantSystem.Domain.Core.IEventStore, PostgresEventStore>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventStoreDbContext>());

        services.AddSingleton(new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        services.AddSingleton<ISerializer, EventSerializer>();

        services.AddScoped<IOrderProjector, RestaurantSystem.Infrastructure.Persistence.Projections.OrderProjector>();

        services.AddScoped<IOrderAggregateStore, OrderAggregateStore>();

        services.Configure<OutboxDispatcherOptions>(
            configuration.GetSection(nameof(OutboxDispatcherOptions)));

        services.AddScoped<IOutboxDispatcher, OutboxDispatcher>();

        services.AddHostedService<OutboxDispatcherBackgroundService>();

        services.Configure<KafkaPublisherOptions>(configuration.GetSection(nameof(KafkaPublisherOptions)));

        services.AddScoped<IIntegrationEventPublisher, KafkaEventPublisher>();

        services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        services.AddScoped<IKitchenOrderNotifier, KitchenOrderNotifier>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IOrderReadStore, OrderReadStore>();


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