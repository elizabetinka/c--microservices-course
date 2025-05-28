// See https://aka.ms/new-console-template for more information
#pragma warning disable CA1506
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task1.Extensions;
using Task2.Extensions;
using Task3;
using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Persistence;
using Task3.Services.Interfaces;
using Xunit;

var services = new ServiceCollection();
var builder = new ConfigurationBuilder();

IConfigurationRoot configuration = builder
    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
    .AddConfigurationService()
    .Build();

services.Configure<ConfigurationOptions>(configuration.GetSection("Configuration"))
    .Configure<ServiceOptions>(configuration.GetSection("Server"))
    .Configure<PostgresOptions>(configuration.GetSection("Persistence:Postgres"));

services.AddRefitServiceClient()
    .AddSingleton(builder.GetConfigurationProvider())
    .AddConfigurationService()
    .AddPersistence()
    .AddServices();

await using ServiceProvider serviceProvider = services.BuildServiceProvider();

using var cts = new CancellationTokenSource();
CancellationToken cancellationToken = cts.Token;

// ConfigurationService service = serviceProvider.GetRequiredService<ConfigurationService>();
// await service.Load(cancellationToken);
//
// Task unused = service.StartAsync(cancellationToken);
await serviceProvider.RunMigrations();

IOrderService orderService = serviceProvider.GetRequiredService<IOrderService>();
IProductService productService = serviceProvider.GetRequiredService<IProductService>();

Product product1 = await productService.AddAsync(new AddProduct("TestProduct1", 2.1m), cancellationToken);
Assert.NotNull(product1.Id);
Assert.Equal(2.1m, product1.Price);
Assert.Equal("TestProduct1", product1.Name);

Product product2 = await productService.AddAsync(new AddProduct("TestProduct2", 5.1m), cancellationToken);
Assert.NotNull(product2.Id);
Assert.Equal(5.1m, product2.Price);
Assert.Equal("TestProduct2", product2.Name);

Order order = await orderService.CreateAsync(new CreateOrder(DateTime.Now, "Liza"), cts.Token);
Assert.NotNull(order.Id);
Assert.Equal("Liza", order.CreatedBy);

IEnumerable<AddProduct> someProducts = new List<AddProduct>()
{
    new("liza2", 2.0m),
    new("liza3", 3.0m),
};

IAsyncEnumerable<Product> productsAsyncEnumerable = productService.AddAsync(someProducts, cts.Token);
IEnumerable<Product> products2 = await productsAsyncEnumerable.ToListAsync(cts.Token);
Assert.NotNull(products2);
Assert.Equal(2, products2.Count());
Assert.Equal("liza2", products2.ElementAt(0).Name);
Assert.Equal("liza3", products2.ElementAt(1).Name);
Assert.Equal(2.0m, products2.ElementAt(0).Price);
Assert.Equal(3.0m, products2.ElementAt(1).Price);

IEnumerable<AddItem> someItems = new List<AddItem>()
{
    new(order.Id, product1.Id, 5),
    new(order.Id, product2.Id, 5),
};

await orderService.AddProductAsync(someItems, cts.Token).ToListAsync();

await orderService.DeleteProductAsync(
    [
    new RemoveItem(order.Id, product1.Id, 3),
    ],
    cts.Token);

await orderService.ToProcessingStatusAsync(order.Id, cts.Token);

await orderService.ToCompletedAsync(order.Id, cts.Token);

cts.CancelAfter(TimeSpan.FromSeconds(10));

await foreach (HistoryItem history in orderService.FindInHistoryAsync(order.Id, [],  cts.Token))
{
    Console.WriteLine(history);
}