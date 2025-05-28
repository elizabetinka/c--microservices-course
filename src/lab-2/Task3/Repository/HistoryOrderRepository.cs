using Npgsql;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Task3.Models.ForDatabase;
using Task3.Models.Payloads;
using Task3.Repository.Interfaces;

namespace Task3.Repository;

public class HistoryOrderRepository : IHistoryOrderRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public HistoryOrderRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<HistoryItem> AddAsync(HistoryItem item, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        try
        {
            string sql = """
                            insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload) 
                            VALUES (:orderId, :createdAt, :type, (CAST(:payload AS json)))
                            RETURNING order_history_item_id;
                                                        
                         """;
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.Add(new NpgsqlParameter("orderId", item.OrderId));
            command.Parameters.Add(new NpgsqlParameter("createdAt", item.CreatedAt));
            command.Parameters.Add(new NpgsqlParameter("type", item.Type));
            command.Parameters.Add(new NpgsqlParameter("payload", JsonSerializer.Serialize(item.Payload)));

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                return item with { HistoryItemId = reader.GetInt64(reader.GetOrdinal("order_history_item_id")) };
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        await connection.CloseAsync();
        return item;
    }

    public async IAsyncEnumerable<HistoryItem> AddAsync(IEnumerable<HistoryItem> item, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var items = item.ToList();
        int n = items.Count;
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        string sql = """
                        insert into order_history (order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload)
                        select orderId, createdAt, type, (CAST(payload AS json)) from unnest(:orderIds, :createdAts, :types, :payloads ) as source(orderId, createdAt, type, payload)
                        RETURNING order_history_item_id;
                     """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(new NpgsqlParameter("orderIds", items.Select(x => x.OrderId).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("createdAts", items.Select(x => x.CreatedAt).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("types", items.Select(x => x.Type).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("payloads", items.Select(x => JsonSerializer.Serialize(x.Payload)).ToArray()));
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        int idx = 0;
        while (idx < n && await reader.ReadAsync(cancellationToken))
        {
            yield return items.ElementAt(idx) with { HistoryItemId = reader.GetInt64(reader.GetOrdinal("order_history_item_id")) };
            idx += 1;
        }

        await connection.CloseAsync();
    }

    public async IAsyncEnumerable<HistoryItem> FindByFiltersAsync(
        long id,
        HistoryType[] historyTypes,
        int cursor,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        try
        {
            string sql = """ 
                         select order_history_item_id, order_id, order_history_item_created_at, order_history_item_kind, order_history_item_payload
                         from order_history
                         where 
                         (order_history_item_id > :cursor) 
                          and order_id = :ids
                          and (cardinality(:types) = 0 or order_history_item_kind = any (:types)) 
                         order by order_history_item_id
                         limit :page_size;
                         """;

            await using NpgsqlCommand command = _dataSource.CreateCommand(sql);
            command.Parameters.Add(new NpgsqlParameter("cursor", cursor));
            command.Parameters.Add(new NpgsqlParameter("page_size", pageSize));
            command.Parameters.Add(new NpgsqlParameter("ids", id));
            command.Parameters.Add(new NpgsqlParameter("types", historyTypes));

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                yield return new HistoryItem(
                    reader.GetInt64(reader.GetOrdinal("order_id")),
                    reader.GetDateTime(reader.GetOrdinal("order_history_item_created_at")),
                    await reader.GetFieldValueAsync<HistoryType>(reader.GetOrdinal("order_history_item_kind"), cancellationToken),
                    JsonSerializer.Deserialize<PayloadBaseModel>(reader.GetString(reader.GetOrdinal("order_history_item_payload"))) ?? throw new InvalidOperationException(),
                    reader.GetInt64(reader.GetOrdinal("order_history_item_id")));
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}