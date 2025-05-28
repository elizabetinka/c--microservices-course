namespace Gateway.Models.Payloads;

public record CreatedOrder(long OrderId, DateTime CreatedAt, string CreatedBy) : PayloadBaseModel(OrderId);