using Task3.Models.ForDatabase;

namespace Task3.Models.Payloads;

public record StateChanged(long OrderId, OrderStatus OrderStatus) : PayloadBaseModel(OrderId);