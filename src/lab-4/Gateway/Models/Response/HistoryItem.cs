using Gateway.Models.Payloads;

namespace Gateway.Models.Response;

public record HistoryItem(long OrderId, DateTime CreatedAt, HistoryType Type, PayloadBaseModel Payload, long HistoryItemId = 0);