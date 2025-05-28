using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Task2.Library;

namespace Task2.Extensions;

public static class CustomConfigurationExtensions
{
    private const string ConfigurationProviderKey = "ConfigurationProvider";

    public static IConfigurationBuilder AddConfigurationService(this IConfigurationBuilder builder)
    {
        builder.Properties[ConfigurationProviderKey] = new CustomConfigurationProvider();

        return builder.Add(new CustomConfigurationSource(builder.GetConfigurationProvider()));
    }

    public static CustomConfigurationProvider GetConfigurationProvider(this IConfigurationBuilder builder)
    {
        if (builder.Properties.TryGetValue(ConfigurationProviderKey, out object? provider))
        {
            return (CustomConfigurationProvider)provider;
        }

        builder.Properties[ConfigurationProviderKey] = new CustomConfigurationProvider();

        return (CustomConfigurationProvider)builder.Properties[ConfigurationProviderKey];
    }

    public static IServiceCollection AddConfigurationService(this IServiceCollection services)
    {
        services.AddHostedService<ConfigurationService>();

        return services;
    }
}