using GrpcApi.Extentions;
using GrpcApi.Kafka;
using Kafka.Exstentions;
using Lab3.Middleware;
using Orders.Kafka.Contracts;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);
builder.AddGrpcServers();
builder.Services.ConfigureKafka(builder.Configuration);
builder.Services
    .AddProducer<OrderCreationKey, OrderCreationValue>()
    .AddConsumer<OrderProcessingKey, OrderProcessingValue, OrderProcessingHandler>()
    .AddEvents();

WebApplication app = builder.Build();
app.AddGrpcServices();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();