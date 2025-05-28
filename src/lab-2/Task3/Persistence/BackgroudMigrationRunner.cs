using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Task3.Persistence;

public class BackgroudMigrationRunner : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroudMigrationRunner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteSingleAsync(stoppingToken);
    }

    private async Task ExecuteSingleAsync(CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        Console.WriteLine(scope.ServiceProvider.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString);
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}