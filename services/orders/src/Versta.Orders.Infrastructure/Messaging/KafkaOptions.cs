namespace Versta.Orders.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string OrdersTopic { get; set; } = "orders.events";
}
