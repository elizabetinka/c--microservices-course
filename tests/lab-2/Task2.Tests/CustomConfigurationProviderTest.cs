using Microsoft.Extensions.Configuration;
using Task2.Library;

namespace Task2.Tests;

public class CustomConfigurationProviderTest
{
    private readonly CustomConfigurationProvider _provider;
    private readonly IConfigurationBuilder _builder;

    public CustomConfigurationProviderTest()
    {
        _provider = new CustomConfigurationProvider();
        _builder = new ConfigurationBuilder().Add(new CustomConfigurationSource(_provider));
    }

    [Fact]
    public void Test1()
    {
        IEnumerable<KeyValuePair<string, string?>> configurations = [new KeyValuePair<string, string?>("test", "assert")];
        Microsoft.Extensions.Primitives.IChangeToken changeToken = _provider.GetReloadToken();
        _provider.LoadDataFromConfiguration(configurations);
        IConfiguration conf = _builder.Build();

        Assert.True(changeToken.HasChanged);
        Assert.Single(conf.AsEnumerable());
        Assert.Equal("assert", conf["test"]);
    }

    [Fact]
    public void Test2()
    {
        IEnumerable<KeyValuePair<string, string?>> configurations = [new KeyValuePair<string, string?>("test", "assert")];
        _provider.LoadDataFromConfiguration(configurations);
        IConfiguration conf = _builder.Build();

        Microsoft.Extensions.Primitives.IChangeToken changeToken = _provider.GetReloadToken();
        _provider.LoadDataFromConfiguration(configurations);

        Assert.False(changeToken.HasChanged);
        Assert.Single(conf.AsEnumerable());
        Assert.Equal("assert", conf["test"]);
    }

    [Fact]
    public void Test3()
    {
        IEnumerable<KeyValuePair<string, string?>> configurations = [new KeyValuePair<string, string?>("test", "assert")];
        _provider.LoadDataFromConfiguration(configurations);
        IConfiguration conf = _builder.Build();

        configurations = [new KeyValuePair<string, string?>("test", "not-assert")];
        Microsoft.Extensions.Primitives.IChangeToken changeToken = _provider.GetReloadToken();
        _provider.LoadDataFromConfiguration(configurations);

        Assert.True(changeToken.HasChanged);
        Assert.Single(conf.AsEnumerable());
        Assert.Equal("not-assert", conf["test"]);
    }

    [Fact]
    public void Test4()
    {
        IEnumerable<KeyValuePair<string, string?>> configurations = [new KeyValuePair<string, string?>("test", "assert")];
        _provider.LoadDataFromConfiguration(configurations);
        IConfiguration conf = _builder.Build();

        configurations = [];
        Microsoft.Extensions.Primitives.IChangeToken changeToken = _provider.GetReloadToken();
        _provider.LoadDataFromConfiguration(configurations);

        Assert.True(changeToken.HasChanged);
        Assert.Empty(conf.AsEnumerable());
    }
}