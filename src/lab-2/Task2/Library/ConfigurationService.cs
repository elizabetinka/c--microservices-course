using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Task1.Library;
using Task1.Model;
using Task2.Extensions;

namespace Task2.Library;

public class ConfigurationService : BackgroundService
{
    private readonly CustomConfigurationProvider _provider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PeriodicTimer _timer;

    public ConfigurationService(CustomConfigurationProvider provider, IOptions<ConfigurationOptions> options, IServiceScopeFactory scopeFactory)
    {
        _provider = provider;
        _scopeFactory = scopeFactory;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.UpdateIntervalSec));
    }

    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }

    public async Task Load(CancellationToken ct = default)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        IServiceClient serviceClient = scope.ServiceProvider.GetRequiredService<IServiceClient>();
        IAsyncEnumerable<IEnumerable<ConfigurationItemDto>> configurations = serviceClient.RequestAsync(ct);
        bool update = true;
        await foreach (IEnumerable<ConfigurationItemDto> configuration in configurations)
        {
            IEnumerable<KeyValuePair<string, string?>> data = configuration.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value));
            _provider.LoadDataFromConfiguration(data, update);
            update = false;
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await Load(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await Load(stoppingToken);
        }
    }
}