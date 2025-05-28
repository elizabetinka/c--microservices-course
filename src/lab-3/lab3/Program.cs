using Lab3.Middleware;
using System.Text.Json.Serialization;
using Task1.Extensions;
using Task2.Extensions;
using Task3;
using Task3.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true).AddConfigurationService();

builder.Services.Configure<ConfigurationOptions>(builder.Configuration.GetSection("Configuration"))
    .Configure<ServiceOptions>(builder.Configuration.GetSection("Server"))
    .Configure<PostgresOptions>(builder.Configuration.GetSection("Persistence:Postgres"));

builder.Services.AddRefitServiceClient()
    .AddSingleton(builder.Configuration.GetConfigurationProvider())
    .AddConfigurationService()
    .AddPersistence()
    .AddServices();

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseSwagger().UseSwaggerUI();
app.MapControllers();

app.Run();
