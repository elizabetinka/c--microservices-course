using Google.Protobuf.WellKnownTypes;
using Google.Type;
using Payloads = Task3.Models.Payloads;
using Server = Task3.Models.ForDatabase;

namespace GrpcApi.Mappers;

public static class ToProto
{
    public static OrderStatus Map(Server.OrderStatus status)
    {
        return status switch
        {
            Server.OrderStatus.OrderCreated => OrderStatus.Created,
            Server.OrderStatus.InProcessing => OrderStatus.Processing,
            Server.OrderStatus.OrderCompleted => OrderStatus.Completed,
            Server.OrderStatus.OrderCancelled => OrderStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }

    public static HistoryType Map(Server.HistoryType type)
    {
        return type switch
        {
            Server.HistoryType.Created => HistoryType.Created,
            Server.HistoryType.ItemAdded => HistoryType.ItemAdded,
            Server.HistoryType.ItemRemoved => HistoryType.ItemRemoved,
            Server.HistoryType.StateChanged => HistoryType.StateChanged,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public static Product Map(Server.Product product)
    {
        return new Product { Name = product.Name, Price = new Money { DecimalValue = product.Price }, Id = product.Id };
    }

    public static Order Map(Server.Order order)
    {
        return new Order { CreatedAt = Timestamp.FromDateTime(order.CreatedAt.ToUniversalTime()), CreatedBy = order.CreatedBy, Id = order.Id, State = Map(order.State) };
    }

    public static OrderItem Map(Server.OrderItem? order)
    {
        if (order == null)
        {
            return new OrderItem { IsNull = true };
        }

        bool deleted = order.Deleted ?? false;
        return new OrderItem { OrderId = order.OrderId, ProductId = order.ProductId, Quantity = order.Quantity, Deleted = deleted, Id = order.OrderId, IsNull = false };
    }

    public static HistoryItem Map(Server.HistoryItem historyItem)
    {
        var ans = new HistoryItem { HistoryItemId = historyItem.HistoryItemId, OrderId = historyItem.OrderId, CreatedAt = Timestamp.FromDateTime(historyItem.CreatedAt.ToUniversalTime()), Type = Map(historyItem.Type) };
        SetPayload(ref ans, historyItem.Payload);
        return ans;
    }

    public static CreateOrderPayload Map(Payloads.CreatedOrder value)
    {
        return new CreateOrderPayload { CreatedAt = Timestamp.FromDateTime(value.CreatedAt.ToUniversalTime()), CreatedBy = value.CreatedBy, OrderId = value.OrderId };
    }

    public static ItemAddedPayload Map(Payloads.ItemAdded value)
    {
        return new ItemAddedPayload { OrderId = value.OrderId, ProductId = value.ProductId, Quantity = value.Quantity };
    }

    public static ItemRemovedPayload Map(Payloads.ItemRemoved value)
    {
        return new ItemRemovedPayload { OrderId = value.OrderId, ProductId = value.ProductId, Quantity = value.Quantity };
    }

    public static StateChangedPayload Map(Payloads.StateChanged value)
    {
        return new StateChangedPayload { OrderId = value.OrderId, OrderStatus = Map(value.OrderStatus) };
    }

    private static void SetPayload(ref HistoryItem historyItem, Payloads.PayloadBaseModel payload)
    {
        switch (payload)
        {
            case Payloads.CreatedOrder value:
                historyItem.Payload.CreateOrderPayload = Map(value);
                break;
            case Payloads.ItemAdded value:
                historyItem.Payload.ItemAddedPayload = Map(value);
                break;
            case Payloads.ItemRemoved value:
                historyItem.Payload.ItemRemovedPayload = Map(value);
                break;
            case Payloads.StateChanged value:
                historyItem.Payload.StateChangedPayload = Map(value);
                break;
            case null:
                throw new ArgumentNullException(nameof(payload));
            default:
                throw new ArgumentOutOfRangeException(nameof(payload), payload, null);
        }
    }
}