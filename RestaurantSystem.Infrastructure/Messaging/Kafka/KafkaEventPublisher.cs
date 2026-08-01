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

    public async Task PublishAsync(
        IntegrationEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        // key را از MessageId می‌گیریم تا ordering per-key حفظ شود
        var key = envelope.MessageId.ToString();

        // خود envelope را به JSON تبدیل می‌کنیم (نه فقط PayloadJson)
        var value = JsonSerializer.Serialize(envelope, _jsonOptions);

        var message = new Message<string, string>
        {
            Key = key,
            Value = value,
            Headers = ToKafkaHeaders(envelope.Headers)
        };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deliveryResult = await _producer.ProduceAsync(_options.Topic, message);

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

    private static Headers? ToKafkaHeaders(IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return null;
        }

        var kafkaHeaders = new Headers();
        foreach (var kv in headers)
        {
            // Header value باید byte[] باشد
            kafkaHeaders.Add(kv.Key, System.Text.Encoding.UTF8.GetBytes(kv.Value));
        }

        return kafkaHeaders;
    }
}
