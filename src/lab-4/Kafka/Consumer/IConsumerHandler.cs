namespace Kafka.Consumer;

public interface IConsumerHandler<TKey, TValue>
{
    Task HandleAsync(IList<KafkaMessage<TKey, TValue>> messages, CancellationToken cancellationToken);
}