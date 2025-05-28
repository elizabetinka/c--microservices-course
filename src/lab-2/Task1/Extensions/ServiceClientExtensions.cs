using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Task1.Library;

namespace Task1.Extensions;

public static class ServiceClientExtensions
{
    public static IServiceCollection AddHttpServiceClient(
        this IServiceCollection services)
    {
        services.AddHttpClient<IServiceClient, HttpServiceClient>()
            .ConfigureHttpClient(
            (provider, c)
                => c.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ServiceOptions>>().Value.ConfigurationServerPath));

        services.AddScoped<IServiceClient, HttpServiceClient>();
        return services;
    }

    public static IServiceCollection AddRefitServiceClient(
        this IServiceCollection services)
    {
        services
            .AddRefitClient<IConfiguratinRefitApi>()
            .ConfigureHttpClient(
                (provider, c)
                    => c.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ServiceOptions>>().Value.ConfigurationServerPath));

        services.AddScoped<IServiceClient, RefitServiceClient>();
        return services;
    }
}