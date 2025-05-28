using Confluent.Kafka;
using Google.Protobuf;
using Kafka.Consumer;
using Kafka.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Exstentions;

public static class KafkaExtensions
{
    public static IServiceCollection ConfigureKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOption>(configuration.GetSection("Kafka"));
        services.Configure<KafkaProducerOption>(configuration.GetSection("Kafka:Producer"));
        services.Configure<KafkaConsumerOption>(configuration.GetSection("Kafka:Consumer"));

        // services.AddSingleton<ProducerConfig>(provider =>
        // {
        //     KafkaOption kafkaOption = provider.GetRequiredService<IOptions<KafkaOption>>().Value;
        //     return new ProducerConfig { BootstrapServers = kafkaOption.Host };
        // });
        // services.AddSingleton<ConsumerConfig>(provider =>
        // {
        //     KafkaOption kafkaOption = provider.GetRequiredService<IOptions<KafkaOption>>().Value;
        //     KafkaConsumerOption kafkaConsumerOption = provider.GetRequiredService<IOptions<KafkaConsumerOption>>().Value;
        //     return new ConsumerConfig { BootstrapServers = kafkaOption.Host, GroupId = kafkaConsumerOption.Group, GroupInstanceId = kafkaConsumerOption.InstanceId };
        // });
        return services;
    }

    public static IServiceCollection AddConsumer<TKey, TValue, THandler>(this IServiceCollection services)
        where THandler : class, IConsumerHandler<TKey, TValue>
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.AddScoped<IConsumerHandler<TKey, TValue>, THandler>();
        services.AddScoped<IKafkaConsumer<TKey, TValue>, BatchKafkaConsumer<TKey, TValue>>();
        services.AddHostedService<BackgroudKafkaConsumer<TKey, TValue>>();
        services.AddSingleton<IDeserializer<TKey>, ProtobufSerializer<TKey>>();
        services.AddSingleton<IDeserializer<TValue>, ProtobufSerializer<TValue>>();
        return services;
    }

    public static IServiceCollection AddProducer<TKey, TValue>(this IServiceCollection services)
        where TKey : IMessage<TKey>, new()
        where TValue : IMessage<TValue>, new()
    {
        services.AddSingleton<IKafkaProducer<TKey, TValue>, KafkaProducer<TKey, TValue>>();
        services.AddSingleton<ISerializer<TKey>, ProtobufSerializer<TKey>>();
        services.AddSingleton<ISerializer<TValue>, ProtobufSerializer<TValue>>();
        return services;
    }
}