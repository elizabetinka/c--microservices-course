namespace Task3.Models.ForServices;

public record RemoveItem(long OrderId, long ProductId, int Quantity = 1);