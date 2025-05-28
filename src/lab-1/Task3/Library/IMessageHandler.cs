namespace Task3.Library;

public interface IMessageHandler
{
    ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken);
}