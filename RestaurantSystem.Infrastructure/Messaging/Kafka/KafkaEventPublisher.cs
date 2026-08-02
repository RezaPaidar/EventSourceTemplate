using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantSystem.Application.Abstractions.Messaging;

namespace RestaurantSystem.Infrastructure.Messaging.Kafka;

public sealed class KafkaEventPublisher : IIntegrationEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaPublisherOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IOptions<KafkaPublisherOptions> options,
        JsonSerializerOptions jsonOptions,
        ILogger<KafkaEventPublisher> logger)
    {
        _options = options.Value;
        _jsonOptions = jsonOptions;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            // ClientId = "restaurant-system-publisher"
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(IntegrationEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var key = envelope.MessageId.ToString();

        var value = JsonSerializer.Serialize(envelope, _jsonOptions);

        var message = new Message<string, string>
        {
            Key = key,
            Value = value,
            Headers = ToKafkaHeaders(envelope)
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deliveryResult = await _producer.ProduceAsync(_options.Topic, message, cancellationToken);

            _logger.LogInformation(
                "Integration event envelope {Type} with id {MessageId} published to Kafka topic {Topic} at offset {Offset}.",
                envelope.Type,
                envelope.MessageId,
                deliveryResult.Topic,
                deliveryResult.Offset);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Publishing integration event envelope {Type} with id {MessageId} was canceled.",
                envelope.Type,
                envelope.MessageId);
            throw;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Kafka produce error for integration event envelope {Type} with id {MessageId}. Reason: {Reason}",
                envelope.Type,
                envelope.MessageId,
                ex.Error.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while publishing integration event envelope {Type} with id {MessageId}.",
                envelope.Type,
                envelope.MessageId);
            throw;
        }
    }

    private static Headers? ToKafkaHeaders(IntegrationEventEnvelope envelope)
    {
        var kafkaHeaders = new Headers();

        if (envelope.Headers is not null)
        {
            foreach (var header in envelope.Headers)
            {
                kafkaHeaders.Add(
                    header.Key,
                    Encoding.UTF8.GetBytes(header.Value));
            }
        }

        if (!kafkaHeaders.Any(h => h.Key == "message-type"))
        {
            kafkaHeaders.Add(
                "message-type",
                Encoding.UTF8.GetBytes(envelope.Type));
        }

        return kafkaHeaders;
    }
}
