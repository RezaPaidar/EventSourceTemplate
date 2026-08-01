using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;
using System.Text.Json;

namespace RestaurantSystem.Infrastructure.Messaging.Kafka.Consumers;

public class OrderIntegrationEventConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;

    public OrderIntegrationEventConsumer(IServiceScopeFactory scopeFactory, IConsumer<string, string> consumer)
    {
        _scopeFactory = scopeFactory;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("resturant-topic");

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();

            await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            try
            {
                var messageBody = result.Message.Value;
                var eventData = JsonSerializer.Deserialize<dynamic>(result.Message.Value);
                var messageId = Guid.Parse(result.Message.Key);

                using var doc = JsonDocument.Parse(messageBody);
                var messageType = "FoodItemAdded";

                // Idempotency
                if (await dbContext.InboxMessages.FindAsync(messageId) != null) continue;

                dbContext.InboxMessages.Add(new InboxMessage
                {
                    Id = messageId,
                    Type = messageType,
                    Payload = messageBody,
                    OccurredOnUtc = DateTime.UtcNow
                });

                // ۳. اینجا هندلر مربوطه را فراخوانی کن
                // await _handler.Handle(eventData);

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                // error log
            }
        }
    }
}
