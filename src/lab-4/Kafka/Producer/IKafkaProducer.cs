namespace Kafka.Producer;

public interface IKafkaProducer<TKey, TValue>
{
    Task ProduceAsync(
        IEnumerable<KafkaMessage<TKey, TValue>> messages,
        CancellationToken cancellationToken);
}