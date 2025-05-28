using Task3.Models.Payloads;

namespace Task3.Models.ForDatabase;

public record HistoryItem(long OrderId, DateTime CreatedAt, HistoryType Type, PayloadBaseModel Payload, long HistoryItemId = 0);