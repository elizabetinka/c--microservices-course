namespace Kafka.Exstentions;

public class KafkaConsumerOption
{
    public string Topic { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string InstanceId { get; set; } = string.Empty;

    public int BufferSize { get; set; } = 10;
}