namespace Task3.Models.ForDatabase;

public record OrderItem(long OrderId, long ProductId, int Quantity, bool? Deleted, long Id = 0);