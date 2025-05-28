using Confluent.Kafka;
using Kafka.Exstentions;
using Microsoft.Extensions.Options;

namespace Kafka.Producer;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>, IDisposable
{
    // private ProducerConfig _producerConfig;
    private readonly KafkaProducerOption _kafkaProducerOption;

    // private ProducerBuilder<TKey, TValue> _producerBuilder;
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaProducer(IOptions<KafkaOption> kafkaOption, IOptions<KafkaProducerOption> kafkaProducerOption, ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
    {
        _kafkaProducerOption = kafkaProducerOption.Value;
        var producerConfig = new ProducerConfig { BootstrapServers = kafkaOption.Value.Host };
        _producer = new ProducerBuilder<TKey, TValue>(producerConfig)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
    }

    public async Task ProduceAsync(IEnumerable<KafkaMessage<TKey, TValue>> messages, CancellationToken cancellationToken)
    {
        foreach (KafkaMessage<TKey, TValue> message in messages)
        {
            var message1 = new Message<TKey, TValue> { Key = message.Key, Value = message.Value };
            await _producer.ProduceAsync(_kafkaProducerOption.Topic, message1, cancellationToken);
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}