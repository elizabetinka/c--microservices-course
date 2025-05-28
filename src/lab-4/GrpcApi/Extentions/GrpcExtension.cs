using GrpcApi.Interceptor;
using GrpcApi.Services;
using Task2.Extensions;
using Task3;
using Task3.Persistence;

namespace GrpcApi.Extentions;

public static class GrpcExtension
{
    public static WebApplicationBuilder AddGrpcServers(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true).AddConfigurationService();

        builder.Services
            .Configure<PostgresOptions>(builder.Configuration.GetSection("Persistence:Postgres"));

        builder.Services
            .AddPersistence()
            .AddServices();

        builder.Services.AddGrpc(options =>
        {
            {
                options.Interceptors.Add<ErrorHandlerInterceptor>();
                options.EnableDetailedErrors = true;
            }
        });

        builder.Services.AddGrpcReflection();

        return builder;
    }

    public static WebApplication AddGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<ProductsController>();
        app.MapGrpcService<OrderController>();
        app.MapGrpcReflectionService();
        return app;
    }
}