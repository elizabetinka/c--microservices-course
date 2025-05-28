using GrpcApi.Kafka.Events;

namespace GrpcApi.Extentions;

public static class EventExtension
{
    public static IServiceCollection AddEvents(this IServiceCollection builder)
    {
        builder.AddSingleton<IEventHandler, ProducerEventHandler>();
        builder.AddSingleton<IEventPublisher, EventPublisher>(provider =>
        {
            IEventHandler handler = provider.GetRequiredService<IEventHandler>();
            return new EventPublisher(new List<IEventHandler>(new List<IEventHandler> { handler }));
        });

        return builder;
    }
}