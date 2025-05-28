using Server = Task3.Models.ForServices;

namespace GrpcApi.Mappers;

public static class FromProto
{
    public static Task3.Models.ForDatabase.OrderStatus Map(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Created => Task3.Models.ForDatabase.OrderStatus.OrderCreated,
            OrderStatus.Processing => Task3.Models.ForDatabase.OrderStatus.InProcessing,
            OrderStatus.Completed => Task3.Models.ForDatabase.OrderStatus.OrderCompleted,
            OrderStatus.Cancelled => Task3.Models.ForDatabase.OrderStatus.OrderCancelled,
            OrderStatus.Unspecified => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    public static Task3.Models.ForDatabase.HistoryType Map(HistoryType type)
    {
        return type switch
        {
            HistoryType.Created => Task3.Models.ForDatabase.HistoryType.Created,
            HistoryType.ItemAdded => Task3.Models.ForDatabase.HistoryType.ItemAdded,
            HistoryType.ItemRemoved => Task3.Models.ForDatabase.HistoryType.ItemRemoved,
            HistoryType.StateChanged => Task3.Models.ForDatabase.HistoryType.StateChanged,
            HistoryType.Unspecified => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public static Server.AddProduct Map(AddProduct product)
    {
        return new Server.AddProduct(product.Name, product.Price.DecimalValue);
    }

    public static Server.CreateOrder Map(AddOrder order)
    {
        return new Server.CreateOrder(order.CreatedAt.ToDateTime(), order.CreatedBy);
    }

    public static Server.AddItem Map(AddItem item)
    {
        return new Server.AddItem(item.OrderId, item.ProductId, item.Quantity);
    }

    public static Server.RemoveItem Map(RemoveItem item)
    {
        return new Server.RemoveItem(item.OrderId, item.ProductId, item.Quantity);
    }
}