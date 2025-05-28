namespace Task3.Models.Payloads;

public record CreatedOrder(long OrderId, DateTime CreatedAt, string CreatedBy) : PayloadBaseModel(OrderId);