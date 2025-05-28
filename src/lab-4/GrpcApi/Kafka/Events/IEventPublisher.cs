namespace GrpcApi.Kafka.Events;

public interface IEventPublisher
{
    Task NotifyAsync(IEnumerable<ServiceEventBase> eventBaseMessage, CancellationToken cancellationToken);
}