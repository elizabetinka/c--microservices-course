namespace GrpcApi.Kafka.Events;

public interface IEventHandler
{
    Task HandleAsync(IEnumerable<ServiceEventBase> eventBaseMessage, CancellationToken cancellationToken = default);
}