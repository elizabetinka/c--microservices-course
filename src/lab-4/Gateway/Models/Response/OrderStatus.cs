using System.Text.Json.Serialization;

namespace Gateway.Models.Response;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    OrderCreated,
    InProcessing,
    OrderCompleted,
    OrderCancelled,
}