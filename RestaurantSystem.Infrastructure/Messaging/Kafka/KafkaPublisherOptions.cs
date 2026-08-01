namespace RestaurantSystem.Infrastructure.Messaging.Kafka;

public sealed class KafkaPublisherOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string Topic { get; set; } = null!;
}
