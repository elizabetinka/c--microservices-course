using Confluent.Kafka;
using Kafka.Exstentions;
using Microsoft.Extensions.Options;

namespace Kafka.Consumer;

public class BatchKafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue>, IDisposable
{
    private readonly KafkaConsumerOption _kafkaConsumerOption;
    private readonly IConsumer<TKey, TValue> _consumer;

    public BatchKafkaConsumer(IOptions<KafkaConsumerOption> kafkaConsumerOption, IOptions<KafkaOption> kafkaOption, IDeserializer<TKey> keyDeserializer, IDeserializer<TValue> valueDeserializer)
    {
        _kafkaConsumerOption = kafkaConsumerOption.Value;
        var consumerConfig = new ConsumerConfig { BootstrapServers = kafkaOption.Value.Host, GroupId = kafkaConsumerOption.Value.Group, GroupInstanceId = kafkaConsumerOption.Value.InstanceId };
        _consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig)
            .SetKeyDeserializer(keyDeserializer)
            .SetValueDeserializer(valueDeserializer)
            .Build();
        _consumer.Subscribe(_kafkaConsumerOption.Topic);
    }

    public async Task<IList<KafkaMessage<TKey, TValue>>> Consume(CancellationToken cancellationToken)
    {
        await Task.Yield();
        List<KafkaMessage<TKey, TValue>> messages = [];
        while (cancellationToken.IsCancellationRequested is false)
        {
            ConsumeResult<TKey, TValue> result = _consumer.Consume(cancellationToken);
            messages.Add(new KafkaMessage<TKey, TValue>(result.Message.Key, result.Message.Value));
            _consumer.Commit(result);
            if (messages.Count >= _kafkaConsumerOption.BufferSize)
            {
                break;
            }
        }

        return messages;
    }

    public void Dispose()
    {
        _consumer.Close();
    }
}