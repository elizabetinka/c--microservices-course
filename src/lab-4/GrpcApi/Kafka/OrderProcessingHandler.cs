using Kafka;
using Kafka.Consumer;
using Orders.Kafka.Contracts;
using Task3.Models.Payloads;
using Task3.Services.Interfaces;

namespace GrpcApi.Kafka;

public class OrderProcessingHandler : IConsumerHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IOrderService _orderService;

    public OrderProcessingHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task HandleAsync(
        IList<KafkaMessage<OrderProcessingKey, OrderProcessingValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (KafkaMessage<OrderProcessingKey, OrderProcessingValue> message in messages)
        {
            await HandleAsync(message, cancellationToken);
        }
    }

    private async Task HandleAsync(KafkaMessage<OrderProcessingKey, OrderProcessingValue> message, CancellationToken cancellationToken)
    {
        var payload = new ProcessingStateChanged(message.Key.OrderId, message.Value.EventCase.ToString());
        await _orderService.AddHistoryItemAsync(payload, cancellationToken);
        if (IsFail(message))
        {
            await _orderService.ToCreatedStatusAsync([message.Key.OrderId], cancellationToken);
            await _orderService.ToCancelledAsync(message.Key.OrderId, cancellationToken);
        }

        if (message.Value.EventCase == OrderProcessingValue.EventOneofCase.DeliveryFinished && message.Value.PackingFinished.IsFinishedSuccessfully)
        {
            await _orderService.ToCompletedAsync(message.Key.OrderId, cancellationToken);
        }
    }

    private bool IsFail(KafkaMessage<OrderProcessingKey, OrderProcessingValue> message)
    {
        OrderProcessingValue value = message.Value;
        return value.EventCase switch
        {
            OrderProcessingValue.EventOneofCase.ApprovalReceived => !value.ApprovalReceived.IsApproved,
            OrderProcessingValue.EventOneofCase.PackingFinished => !value.PackingFinished.IsFinishedSuccessfully,
            OrderProcessingValue.EventOneofCase.DeliveryFinished => !value.DeliveryFinished.IsFinishedSuccessfully,
            OrderProcessingValue.EventOneofCase.None => false,
            OrderProcessingValue.EventOneofCase.PackingStarted => false,
            OrderProcessingValue.EventOneofCase.DeliveryStarted => false,
            _ => false,
        };
    }
}