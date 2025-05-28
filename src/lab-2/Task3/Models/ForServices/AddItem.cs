namespace Task3.Models.ForServices;

public record AddItem(long OrderId, long ProductId, int Quantity = 1);