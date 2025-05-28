namespace Kafka;

public record KafkaMessage<TKey, TValue>(TKey Key, TValue Value);