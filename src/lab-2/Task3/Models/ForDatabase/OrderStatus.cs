using NpgsqlTypes;
using System.Text.Json.Serialization;

namespace Task3.Models.ForDatabase;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    [PgName("created")]
    OrderCreated,
    [PgName("processing")]
    InProcessing,
    [PgName("completed")]
    OrderCompleted,
    [PgName("cancelled")]
    OrderCancelled,
}