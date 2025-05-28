using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Task3.Models.ForDatabase;
using Task3.Persistence.Migrations;
using Task3.Repository;
using Task3.Repository.Interfaces;
using Task3.Services;
using Task3.Services.Interfaces;

namespace Task3.Persistence;

public static class PersistenceExtension
{
    public static IServiceCollection AddPersistence(this IServiceCollection collection)
    {
        collection
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(p => p.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString)
                .ScanIn(typeof(InitialMigration).Assembly)
                .For.Migrations());

        collection.AddHostedService<BackgroudMigrationRunner>();

        collection.AddSingleton(p =>
        {
            var dataSourceBuilder =
                new NpgsqlDataSourceBuilder(p.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString);
            Console.WriteLine(p.GetRequiredService<IOptions<PostgresOptions>>().Value.ConnectionString);
            dataSourceBuilder.MapEnum<OrderStatus>(pgName: "order_state");
            dataSourceBuilder.MapEnum<HistoryType>(pgName: "order_history_item_kind");
            dataSourceBuilder.EnableDynamicJson();
            return dataSourceBuilder.Build();
        });
        collection.AddScoped<IProductsRepository, ProductRepository>();
        collection.AddScoped<IOrderRepository, OrderRepository>();
        collection.AddScoped<IHistoryOrderRepository, HistoryOrderRepository>();
        collection.AddScoped<IOrderItemsRepository, OrderItemsRepository>();
        return collection;
    }

    public static IServiceCollection AddServices(this IServiceCollection collection)
    {
        collection.AddScoped<IOrderService, OrderService>();
        collection.AddScoped<IProductService, ProductService>();
        return collection;
    }

    public static async Task RunMigrations(this IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}