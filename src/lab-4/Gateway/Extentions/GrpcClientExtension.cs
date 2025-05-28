using Gateway.Options;
using Gateway.Services;
using GrpcApi;
using Microsoft.Extensions.Options;

namespace Gateway.Extentions;

public static class GrpcClientExtension
{
    public static WebApplicationBuilder AddGrpcClients(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("config.json", optional: true, reloadOnChange: true);

        builder.Services.Configure<GrpcClientOptions>(builder.Configuration.GetSection("GrpcClient"));
        builder.Services.AddGrpcClient<OrderService.OrderServiceClient>((p, o) =>
        {
            IOptions<GrpcClientOptions> options = p.GetRequiredService<IOptions<GrpcClientOptions>>();
            o.Address = new Uri(options.Value.Address);
        });

        builder.Services.AddGrpcClient<ProductService.ProductServiceClient>((sp, o) =>
        {
            IOptions<GrpcClientOptions> options = sp.GetRequiredService<IOptions<GrpcClientOptions>>();
            o.Address = new Uri(options.Value.Address);
        });
        builder.Services.AddSingleton<MediatorGrpcOrderClient>();
        builder.Services.AddSingleton<MediatorGrpcProductClient>();
        return builder;
    }
}