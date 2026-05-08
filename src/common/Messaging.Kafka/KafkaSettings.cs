namespace Messaging.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string GroupId { get; set; }
    public int BatchSize { get; set; } = 0;
    public int FlushIntervalMs { get; set; } = 0;
}