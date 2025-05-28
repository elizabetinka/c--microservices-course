using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kafka.Consumer;

public class BackgroudKafkaConsumer<TKey, TValue> : BackgroundService, IDisposable
{
    // private readonly IKafkaConsumer<TKey, TValue> _consumer;
    // private readonly IConsumerHandler<TKey, TValue> _handler;
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroudKafkaConsumer(IServiceScopeFactory scopeFactory)
    {
        // _consumer = kafkaConsumer;
        // _handler = consumerHandler;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

        IConsumerHandler<TKey, TValue> handler = scope.ServiceProvider
            .GetRequiredService<IConsumerHandler<TKey, TValue>>();
        IKafkaConsumer<TKey, TValue> consumer = scope.ServiceProvider
            .GetRequiredService<IKafkaConsumer<TKey, TValue>>();

        while (stoppingToken.IsCancellationRequested is false)
        {
            IList<KafkaMessage<TKey, TValue>> messages = await consumer.Consume(stoppingToken);
            await handler.HandleAsync(messages, stoppingToken);
        }
    }

    // private async Task ExecuteSingleAsync(CancellationToken cancellationToken)
    // {
    //     List<Message<TKey, TValue>> messages = [];
    //     while (cancellationToken.IsCancellationRequested is false or messages.Count() < _kafkaConsumerOption.BufferSize)
    //     {
    //         ConsumeResult<TKey, TValue> result = _consumer.Consume(cancellationToken);
    //         messages.Add(result.Message);
    //     }
    //
    //     await _handler.HandleAsync(messages, cancellationToken);
    // }
}