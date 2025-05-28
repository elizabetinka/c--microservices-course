using Npgsql;
using System.Runtime.CompilerServices;
using Task3.Models.ForDatabase;
using Task3.Repository.Interfaces;

namespace Task3.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        try
        {
            string sql = """
                         insert into orders (order_state, order_created_at, order_created_by) 
                         VALUES (:state, :createdAt, :createdBy)
                         RETURNING order_id;
                         """;
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add(new NpgsqlParameter("state", order.State));
            command.Parameters.Add(new NpgsqlParameter("createdAt", order.CreatedAt));
            command.Parameters.Add(new NpgsqlParameter("createdBy", order.CreatedBy));

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                return order with { Id = reader.GetInt64(reader.GetOrdinal("order_id")) };
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await connection.CloseAsync();
        return order;
    }

    public async IAsyncEnumerable<Order> AddAsync(IEnumerable<Order> order, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEnumerable<Order> enumerable = order.ToList();
        int n = enumerable.Count();
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string sql = """
                         insert into orders (order_state, order_created_at, order_created_by)
                         select state, createdAt, createdBy from unnest(:states, :createdAts, :createdBys ) as source(state, createdAt, createdBy)
                         RETURNING order_id;                            
                     """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("states", enumerable.Select(x => x.State).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("createdAts", enumerable.Select(x => x.CreatedAt).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("createdBys", enumerable.Select(x => x.CreatedBy).ToArray()));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int idx = 0;
        while (idx < n && await reader.ReadAsync(cancellationToken))
        {
            yield return enumerable.ElementAt(idx) with { Id = reader.GetInt64(reader.GetOrdinal("order_id")) };
            idx += 1;
        }

        await connection.CloseAsync();
    }

    public async Task ChangeStatusByIdAsync(long orderId, OrderStatus orderStatus, CancellationToken cancellationToken)
    {
        string sql = "UPDATE orders SET order_state = @status WHERE order_id = @OrderId;";

        await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("OrderId", orderId);
        command.Parameters.AddWithValue("status", orderStatus);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ChangeStatusByIdAsync(long[] orderId, OrderStatus orderStatus, CancellationToken cancellationToken)
    {
        string sql = """ 
                         update orders 
                         set order_state = :status
                         from (select * from unnest(:ids)) as source(id) 
                         where order_id = source.id
                     """;
        await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter("ids", orderId));
        command.Parameters.Add(new NpgsqlParameter("status", orderStatus));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Order?> FindByIdAsync(
        long id, CancellationToken cancellationToken)
    {
        Order? ans = null;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            string sql = """ 
                         select order_id, order_state, order_created_at, order_created_by
                         from orders
                         where  order_id = :id;
                         """;
            await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
            command.Parameters.Add(new NpgsqlParameter("id", id));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                ans = new Order(
                    await reader.GetFieldValueAsync<OrderStatus>(reader.GetOrdinal("order_state"), cancellationToken),
                    reader.GetDateTime(reader.GetOrdinal("order_created_at")),
                    reader.GetString(reader.GetOrdinal("order_created_by")),
                    reader.GetInt64(reader.GetOrdinal("order_id")));
            }
        }
        finally
        {
            await connection.CloseAsync();
        }

        return ans;
    }

    public async IAsyncEnumerable<Order> FindByFiltersAsync(
        long[] id,
        OrderStatus[] statuses,
        string[] authors,
        int cursor,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            string sql = """ 
                         select order_id, order_state, order_created_at, order_created_by
                         from orders
                         where 
                         (order_id > :cursor) 
                          and (cardinality(:ids) = 0 or order_id = any (:ids)) 
                          and (cardinality(:status) = 0 or order_state = any(:status)) 
                          and (cardinality(:author) = 0 or order_created_by = any(:author)) 
                         order by order_id 
                         limit :page_size;

                         """;
            await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
            command.Parameters.Add(new NpgsqlParameter("cursor", cursor));
            command.Parameters.Add(new NpgsqlParameter("page_size", pageSize));
            command.Parameters.Add(new NpgsqlParameter("ids", id));
            command.Parameters.Add(new NpgsqlParameter("status", statuses));
            command.Parameters.Add(new NpgsqlParameter("author", authors));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return new Order(
                    await reader.GetFieldValueAsync<OrderStatus>(reader.GetOrdinal("order_state"), cancellationToken),
                    reader.GetDateTime(reader.GetOrdinal("order_created_at")),
                    reader.GetString(reader.GetOrdinal("order_created_by")),
                    reader.GetInt64(reader.GetOrdinal("order_id")));
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}