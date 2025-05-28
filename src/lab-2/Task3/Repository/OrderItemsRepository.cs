using Npgsql;
using System.Runtime.CompilerServices;
using Task3.Models.ForDatabase;
using Task3.Repository.Interfaces;

namespace Task3.Repository;

public class OrderItemsRepository : IOrderItemsRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrderItemsRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<OrderItem> AddAsync(OrderItem item, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        try
        {
            string sql = """
                            insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted) 
                            VALUES (:orderId, :productId, :quantity, :deleted)
                            RETURNING order_item_id;
                         """;
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add(new NpgsqlParameter("orderId", item.OrderId));
            command.Parameters.Add(new NpgsqlParameter("productId", item.ProductId));
            command.Parameters.Add(new NpgsqlParameter("quantity", item.Quantity));
            command.Parameters.Add(new NpgsqlParameter("deleted", item.Deleted));

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                return item with { Id = reader.GetInt64(reader.GetOrdinal("order_item_id")) };
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await connection.CloseAsync();
        return item;
    }

    public async IAsyncEnumerable<OrderItem> AddAsync(IEnumerable<OrderItem> item, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IEnumerable<OrderItem?> ans = [];
        IEnumerable<OrderItem> items = item.ToList();
        int n = items.Count();
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string sql = """
                        insert into order_items (order_id, product_id, order_item_quantity, order_item_deleted)
                        select orderId, productId, quantity, deleted from unnest(:orderIds, :productIds, :quantitys, :deleteds) as source(orderId, productId, quantity, deleted)
                        RETURNING order_item_id;
                     """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("orderIds", items.Select(x => x.OrderId).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("productIds", items.Select(x => x.ProductId).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("quantitys", items.Select(x => x.Quantity).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("deleteds", items.Select(x => x.Deleted).ToArray()));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int idx = 0;

        while (idx < n && await reader.ReadAsync(cancellationToken))
        {
            yield return items.ElementAt(idx) with { Id = reader.GetInt64(reader.GetOrdinal("order_item_id")) };
            idx += 1;
        }

        await connection.CloseAsync();
    }

    public async Task SafeDeleteAsync(long itemId, long orderId, CancellationToken cancellationToken)
    {
        string sql = "UPDATE order_items SET order_item_deleted = true WHERE order_id = @OrderId and product_id = @ItemId;";
        await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("OrderId", orderId);
        command.Parameters.AddWithValue("ItemId", itemId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SafeDeleteAsync(long[] itemId, long orderId, CancellationToken cancellationToken)
    {
        string sql = """ 
                         update order_items
                         set order_item_deleted = true
                         from (select * from unnest(:ids)) as source(id) 
                         where product_id = source.id and order_id = :orderId;
                     """;
        await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter("ids", itemId));
        command.Parameters.Add(new NpgsqlParameter("orderId", orderId));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(long productId, int quantity, CancellationToken cancellationToken)
    {
        string sql = """ 
                         update order_items
                         set order_item_quantity = :quantity
                         where product_id = :id;
                     """;
        await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
        command.Parameters.Add(new NpgsqlParameter("id", productId));
        command.Parameters.Add(new NpgsqlParameter("quantity", quantity));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<OrderItem> FindByFiltersAsync(
        long[] goodIds,
        long[] orderIds,
        bool deleted,
        int cursor,
        int pageSize,
        int atLeastQuantity = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            string sql = """ 
                         select order_item_id, order_id, product_id, order_item_quantity, order_item_deleted
                         from order_items
                         where 
                         (order_item_id > :cursor) 
                          and (cardinality(:goodIds) = 0 or product_id = any (:goodIds)) 
                          and (cardinality(:orderIds) = 0 or order_id = any (:orderIds)) 
                          and (order_item_deleted = :deleted)
                          and (order_item_quantity >= :quantity)
                         order by order_item_id
                         limit :page_size;

                         """;
            await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
            command.Parameters.Add(new NpgsqlParameter("cursor", cursor));
            command.Parameters.Add(new NpgsqlParameter("page_size", pageSize));
            command.Parameters.Add(new NpgsqlParameter("goodIds", goodIds));
            command.Parameters.Add(new NpgsqlParameter("orderIds", orderIds));
            command.Parameters.Add(new NpgsqlParameter("deleted", deleted));
            command.Parameters.Add(new NpgsqlParameter("quantity", atLeastQuantity));
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return new OrderItem(
                    reader.GetInt64(reader.GetOrdinal("order_id")),
                    reader.GetInt64(reader.GetOrdinal("product_id")),
                    reader.GetInt32(reader.GetOrdinal("order_item_quantity")),
                    reader.GetBoolean(reader.GetOrdinal("order_item_deleted")),
                    reader.GetInt64(reader.GetOrdinal("order_item_id")));
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}