using Google.Protobuf.WellKnownTypes;
using Kafka;
using Kafka.Producer;
using Orders.Kafka.Contracts;

namespace GrpcApi.Kafka.Events;

public class ProducerEventHandler : IEventHandler
{
    private readonly IKafkaProducer<OrderCreationKey, OrderCreationValue> _producer;

    public ProducerEventHandler(IKafkaProducer<OrderCreationKey, OrderCreationValue> producer)
    {
        _producer = producer;
    }

    public async Task HandleAsync(IEnumerable<ServiceEventBase> eventBaseMessage, CancellationToken cancellationToken)
    {
        await HandleToCreateAsync(eventBaseMessage.OfType<ServiceEventToCreate>(), cancellationToken);
        await HandleToProcessingAsync(eventBaseMessage.OfType<ServiceEventToProcessing>(), cancellationToken);
    }

    private async Task HandleToCreateAsync(IEnumerable<ServiceEventToCreate> eventBaseMessage, CancellationToken cancellationToken)
    {
        IList<KafkaMessage<OrderCreationKey, OrderCreationValue>> messages = [];
        foreach (ServiceEventToCreate eventToCreate in eventBaseMessage)
        {
            var key = new OrderCreationKey { OrderId = eventToCreate.OrderId };

            var orderCreated = new OrderCreationValue.Types.OrderCreated
                { OrderId = eventToCreate.OrderId, CreatedAt = Timestamp.FromDateTime(eventToCreate.CreatedAt.ToUniversalTime()) };

            var value = new OrderCreationValue { OrderCreated = orderCreated };
            var message = new KafkaMessage<OrderCreationKey, OrderCreationValue>(key, value);
            messages.Add(message);
        }

        await _producer.ProduceAsync(messages, cancellationToken);
    }

    private async Task HandleToProcessingAsync(IEnumerable<ServiceEventToProcessing> eventBaseMessage, CancellationToken cancellationToken)
    {
        IList<KafkaMessage<OrderCreationKey, OrderCreationValue>> messages = [];
        foreach (ServiceEventToProcessing eventToProcessing in eventBaseMessage)
        {
            var key = new OrderCreationKey { OrderId = eventToProcessing.OrderId };

            var orderCreated = new OrderCreationValue.Types.OrderProcessingStarted
                { OrderId = eventToProcessing.OrderId, StartedAt = Timestamp.FromDateTime(eventToProcessing.StartedAt.ToUniversalTime()) };

            var value = new OrderCreationValue { OrderProcessingStarted = orderCreated };
            var message = new KafkaMessage<OrderCreationKey, OrderCreationValue>(key, value);
            messages.Add(message);
        }

        await _producer.ProduceAsync(messages, cancellationToken);
    }
}