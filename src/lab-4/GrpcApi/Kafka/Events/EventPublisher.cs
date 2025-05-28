namespace GrpcApi.Kafka.Events;

public class EventPublisher : IEventPublisher
{
    private readonly IReadOnlyList<IEventHandler> _handlers;

    public EventPublisher(IReadOnlyList<IEventHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task NotifyAsync(IEnumerable<ServiceEventBase> eventBaseMessage, CancellationToken cancellationToken = default)
    {
        foreach (IEventHandler handler in _handlers)
        {
            await handler.HandleAsync(eventBaseMessage, cancellationToken);
        }
    }
}