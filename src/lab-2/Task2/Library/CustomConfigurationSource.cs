using Microsoft.Extensions.Configuration;

namespace Task2.Library;

public class CustomConfigurationSource : IConfigurationSource
{
    private readonly CustomConfigurationProvider _provider;

    public CustomConfigurationSource(CustomConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }
}