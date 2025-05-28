namespace Gateway.Models.Request;

public record RemoveItem(long OrderId, long ProductId, int Quantity = 1);