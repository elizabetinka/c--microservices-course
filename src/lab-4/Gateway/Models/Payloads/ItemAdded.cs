namespace Gateway.Models.Payloads;

public record ItemAdded(long OrderId, long ProductId, int Quantity = 1) : PayloadBaseModel(OrderId);