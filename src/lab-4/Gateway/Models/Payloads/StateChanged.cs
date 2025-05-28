using Gateway.Models.Response;

namespace Gateway.Models.Payloads;

public record StateChanged(long OrderId, OrderStatus OrderStatus) : PayloadBaseModel(OrderId);