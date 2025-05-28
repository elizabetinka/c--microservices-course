using Task3.Library;

namespace Task3;

public class ConsoleMessageHandler : IMessageHandler
{
    public ValueTask HandleAsync(IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        string combinedMessage = string.Join(Environment.NewLine, messages.Select(m => $"{m.Title}: {m.Text}"));

        Console.WriteLine(combinedMessage);
        return ValueTask.CompletedTask;
    }
}