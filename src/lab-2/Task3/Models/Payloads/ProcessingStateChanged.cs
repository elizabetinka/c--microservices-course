namespace Task3.Models.Payloads;

public record ProcessingStateChanged(long OrderId, string State) : PayloadBaseModel(OrderId);