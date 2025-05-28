namespace Gateway.Models.Request;

public record AddItem(long OrderId, long ProductId, int Quantity = 1);