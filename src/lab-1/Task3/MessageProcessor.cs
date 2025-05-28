using Itmo.Dev.Platform.Common.Extensions;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Task3.Library;

namespace Task3;

public class MessageProcessor(IEnumerable<IMessageHandler> handlers, Config config) : IMessageProcessor, IMessageSender
{
    private readonly Channel<Message> _channel = Channel.CreateBounded<Message>(
        new BoundedChannelOptions(config.Capacity)
            { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        ConfiguredCancelableAsyncEnumerable<IReadOnlyList<Message>> messages = _channel.Reader
            .ReadAllAsync(cancellationToken)
            .ChunkAsync(config.ChunkSize, config.Timeout, config.TimeoutChunkSpan)
            .WithCancellation(cancellationToken);

        await foreach (IReadOnlyList<Message> message in messages)
        {
            foreach (IMessageHandler handler in handlers)
            {
                await handler.HandleAsync(message, cancellationToken);
            }
        }
    }

    public void Complete()
    {
        _channel.Writer.TryComplete();
    }

    public async ValueTask SendAsync(Message message, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(message, cancellationToken);
    }
}