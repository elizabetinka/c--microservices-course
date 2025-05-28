using Npgsql;
using System.Runtime.CompilerServices;
using Task3.Models.ForDatabase;
using Task3.Repository.Interfaces;

namespace Task3.Repository;

public class ProductRepository : IProductsRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProductRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        try
        {
            string sql = """
                            insert into products (product_name, product_price) 
                            VALUES (:name, :price)
                            RETURNING product_id;
                         """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add(new NpgsqlParameter("name", product.Name));
            command.Parameters.Add(new NpgsqlParameter("price", product.Price));

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                return product with { Id = reader.GetInt64(reader.GetOrdinal("product_id")) };
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await connection.CloseAsync();
        return product;
    }

    public async IAsyncEnumerable<Product> AddAsync(IEnumerable<Product> product, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEnumerable<Product> enumerable = product.ToList();
        int n = enumerable.Count();
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string sql = """
                        insert into products (product_name, product_price) 
                        select name, price from unnest(:names, :prices) as source(name, price)
                        RETURNING product_id;
                     """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("names", enumerable.Select(x => x.Name).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("prices", enumerable.Select(x => x.Price).ToArray()));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int idx = 0;
        while (idx < n && await reader.ReadAsync(cancellationToken))
        {
            yield return enumerable.ElementAt(idx) with { Id = reader.GetInt64(reader.GetOrdinal("product_id")) };
            idx += 1;
        }

        await connection.CloseAsync();
    }

    public async IAsyncEnumerable<Product> FindByFiltersAsync(
        long[] id,
        string? pattern,
        decimal? minCost,
        decimal? maxCost,
        int cursor,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            string sql = """ 
                         select product_id, product_name, product_price
                         from products
                         where 
                         (product_id > :cursor) 
                          and (cardinality(:ids) = 0 or product_id = any (:ids)) 
                          and (:pattern is null or product_name like :pattern) 
                          and (:min_cost is null or product_price >= :min_cost) 
                           and (:max_cost is null or product_price <= :max_cost) 
                         order by product_id
                         limit :page_size;

                         """;
            await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
            command.Parameters.Add(new NpgsqlParameter("cursor", cursor));
            command.Parameters.Add(new NpgsqlParameter("page_size", pageSize));
            command.Parameters.Add(new NpgsqlParameter("ids", id));
            command.Parameters.Add(new NpgsqlParameter("pattern", pattern));
            command.Parameters.Add(new NpgsqlParameter("min_cost", minCost));
            command.Parameters.Add(new NpgsqlParameter("max_cost", maxCost));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return new Product(
                    reader.GetString(reader.GetOrdinal("product_name")),
                    reader.GetDecimal(reader.GetOrdinal("product_price")),
                    reader.GetInt64(reader.GetOrdinal("product_id")));
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}