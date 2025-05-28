// See https://aka.ms/new-console-template for more information

using Task3;
using Task3.Library;

#pragma warning disable CA1859

var config = new Config(8, 3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
var implementation = new MessageProcessor([new ConsoleMessageHandler()], config);

IMessageProcessor processor = implementation;
IMessageSender sender = implementation;

Task result = processor.ProcessAsync(default);
IEnumerable<Message> messages =
[
    new("Самба", "Олег"), new("Ча-ча-ча", "Арина"), new("Румба", "Саша"),
    new("Пасодобль", "Эмиль"), new("Джайв", "Вася")
];

await Parallel.ForEachAsync(messages, async (message, cancellationToken) => await sender.SendAsync(message, cancellationToken));

processor.Complete();

await result;
