namespace Task3.Models.ForDatabase;

public record Order(OrderStatus State, DateTime CreatedAt, string CreatedBy, long Id = 0);