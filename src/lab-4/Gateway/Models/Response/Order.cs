namespace Gateway.Models.Response;

public record Order(OrderStatus State, DateTime CreatedAt, string CreatedBy, long Id = 0);