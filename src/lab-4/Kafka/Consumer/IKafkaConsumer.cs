namespace Kafka.Consumer;

public interface IKafkaConsumer<TKey, TValue>
{
    Task<IList<KafkaMessage<TKey, TValue>>> Consume(CancellationToken cancellationToken);
}