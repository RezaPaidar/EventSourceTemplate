using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantSystem.Infrastructure.Persistence.EventStore;
using RestaurantSystem.Infrastructure.Persistence.EventStore.Models;
using System.Text;

namespace RestaurantSystem.Infrastructure.Messaging.Kafka.Consumers;

public class OrderIntegrationEventConsumer : BackgroundService
{
    private readonly ILogger<OrderIntegrationEventConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic;

    public OrderIntegrationEventConsumer(
        ILogger<OrderIntegrationEventConsumer> logger,
        IServiceScopeFactory scopeFactory,
        IConsumer<string, string> consumer,
        IOptions<KafkaPublisherOptions> kafkaOptions)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _consumer = consumer;
        _topic = kafkaOptions.Value.Topic
                 ?? throw new InvalidOperationException("KafkaPublisherOptions:Topic is not configured.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IIntegrationEventDispatcher>();

            await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            try
            {
                var messageId = Guid.Parse(result.Message.Key);

                var typeHeader = result.Message.Headers.FirstOrDefault(h => h.Key == "message-type");
                var messageType = typeHeader != null
                    ? Encoding.UTF8.GetString(typeHeader.GetValueBytes())
                    : "Unknown";

                if (messageType == "Unknown")
                {
                    _logger.LogWarning("Skipping message with Key: {Key}. Missing 'message-type' header.", result.Message.Key);

                    _consumer.Commit(result);
                    continue;
                }

                if (await dbContext.InboxMessages.FindAsync(new object[] { messageId }, stoppingToken) is not null)
                    continue;

                await dispatcher.DispatchAsync(
                    messageType,
                    result.Message.Key,
                    result.Message.Value,
                    stoppingToken);

                dbContext.InboxMessages.Add(new InboxMessage
                {
                    Id = messageId,
                    Type = messageType,
                    Payload = result.Message.Value,
                    OccurredOnUtc = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                _consumer.Commit(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message with Key: {Key}. Rolling back transaction.", result.Message.Key);

                await transaction.RollbackAsync(stoppingToken);
                if (ex is System.NotSupportedException)
                {
                    return;
                }
                throw;
            }
        }
    }
}
