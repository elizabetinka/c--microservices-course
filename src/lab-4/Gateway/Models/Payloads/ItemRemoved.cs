namespace Gateway.Models.Payloads;

public record ItemRemoved(long OrderId, long ProductId, int Quantity = 1) : PayloadBaseModel(OrderId);