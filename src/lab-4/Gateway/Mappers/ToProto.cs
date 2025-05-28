using Google.Protobuf.WellKnownTypes;
using Google.Type;
using GrpcApi;
using MyHttpRequest = Gateway.Models.Request;
using MyHttpResponse = Gateway.Models.Response;

namespace Gateway.Mappers;

public static class ToProto
{
    public static OrderStatus Map(MyHttpResponse.OrderStatus status)
    {
        return status switch
        {
            MyHttpResponse.OrderStatus.OrderCreated => OrderStatus.Created,
            MyHttpResponse.OrderStatus.InProcessing => OrderStatus.Processing,
            MyHttpResponse.OrderStatus.OrderCompleted => OrderStatus.Completed,
            MyHttpResponse.OrderStatus.OrderCancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    public static HistoryType Map(MyHttpResponse.HistoryType type)
    {
        return type switch
        {
            MyHttpResponse.HistoryType.Created => HistoryType.Created,
            MyHttpResponse.HistoryType.ItemAdded => HistoryType.ItemAdded,
            MyHttpResponse.HistoryType.ItemRemoved => HistoryType.ItemRemoved,
            MyHttpResponse.HistoryType.StateChanged => HistoryType.StateChanged,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public static AddProduct Map(MyHttpRequest.AddProduct product)
    {
        return new AddProduct { Name = product.Name, Price = new Money { DecimalValue = product.Price } };
    }

    public static AddOrder Map(MyHttpRequest.CreateOrder order)
    {
        return new AddOrder { CreatedAt = Timestamp.FromDateTime(order.CreatedAt.ToUniversalTime()), CreatedBy = order.CreatedBy };
    }

    public static AddItem Map(MyHttpRequest.AddItem item)
    {
        return new AddItem { OrderId = item.OrderId, ProductId = item.ProductId, Quantity = item.Quantity };
    }

    public static RemoveItem Map(MyHttpRequest.RemoveItem item)
    {
        return new RemoveItem { OrderId = item.OrderId, ProductId = item.ProductId, Quantity = item.Quantity };
    }
}