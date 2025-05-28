using GrpcApi;
using MyHttpResponse = Gateway.Models.Response;
using Payloads = Gateway.Models.Payloads;

namespace Gateway.Mappers;

public static class FromProto
{
    public static MyHttpResponse.OrderStatus Map(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Created => MyHttpResponse.OrderStatus.OrderCreated,
            OrderStatus.Processing => MyHttpResponse.OrderStatus.InProcessing,
            OrderStatus.Completed => MyHttpResponse.OrderStatus.OrderCompleted,
            OrderStatus.Cancelled => MyHttpResponse.OrderStatus.OrderCancelled,
            OrderStatus.Unspecified => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    public static MyHttpResponse.HistoryType Map(HistoryType type)
    {
        return type switch
        {
            HistoryType.Created => MyHttpResponse.HistoryType.Created,
            HistoryType.ItemAdded => MyHttpResponse.HistoryType.ItemAdded,
            HistoryType.ItemRemoved => MyHttpResponse.HistoryType.ItemRemoved,
            HistoryType.StateChanged => MyHttpResponse.HistoryType.StateChanged,
            HistoryType.Unspecified => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public static MyHttpResponse.Product Map(Product product)
    {
        return new MyHttpResponse.Product(product.Name, product.Price.DecimalValue, product.Id);
    }

    public static IList<MyHttpResponse.Product> Map(ProductsResponse products)
    {
        return products.Products.Select(Map).ToList();
    }

    public static MyHttpResponse.Order Map(Order order)
    {
        return new MyHttpResponse.Order(Map(order.State), order.CreatedAt.ToDateTime(), order.CreatedBy, order.Id);
    }

    public static IList<MyHttpResponse.Order> Map(CreateOrdersResponse orders)
    {
        return orders.Orders.Select(Map).ToList();
    }

    public static MyHttpResponse.OrderItem? Map(OrderItem order)
    {
        if (order.IsNull)
        {
            return null;
        }

        return new MyHttpResponse.OrderItem(order.OrderId, order.ProductId, order.Quantity, order.Deleted, order.OrderId);
    }

    public static IList<MyHttpResponse.OrderItem?> Map(CreateItemsResponse orders)
    {
        return orders.Items.Select(Map).ToList();
    }

    public static MyHttpResponse.HistoryItem Map(HistoryItem historyItem)
    {
        return new MyHttpResponse.HistoryItem(historyItem.OrderId, historyItem.CreatedAt.ToDateTime(), Map(historyItem.Type), GetPayload(historyItem), historyItem.HistoryItemId);
    }

    public static IList<MyHttpResponse.HistoryItem> Map(GetHistoryItemsResponse orders)
    {
       return orders.Items.Select(Map).ToList();
    }

    private static Payloads.PayloadBaseModel GetPayload(HistoryItem item)
    {
        Payload payload = item.Payload;
        return payload.DataCase switch
        {
            Payload.DataOneofCase.CreateOrderPayload => new Payloads.CreatedOrder(payload.CreateOrderPayload.OrderId, payload.CreateOrderPayload.CreatedAt.ToDateTime(), payload.CreateOrderPayload.CreatedBy),
            Payload.DataOneofCase.ItemAddedPayload => new Payloads.ItemAdded(payload.ItemAddedPayload.OrderId, payload.ItemAddedPayload.ProductId, payload.ItemAddedPayload.Quantity),
            Payload.DataOneofCase.ItemRemovedPayload => new Payloads.ItemRemoved(payload.ItemRemovedPayload.OrderId, payload.ItemRemovedPayload.ProductId, payload.ItemRemovedPayload.Quantity),
            Payload.DataOneofCase.StateChangedPayload => new Payloads.StateChanged(payload.StateChangedPayload.OrderId, Map(payload.StateChangedPayload.OrderStatus)),
            Payload.DataOneofCase.None => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }
}