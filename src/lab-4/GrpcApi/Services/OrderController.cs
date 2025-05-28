using Grpc.Core;
using GrpcApi.Kafka.Events;
using GrpcApi.Mappers;

using Task3.Services.Interfaces;
using Server = Task3.Models.ForDatabase;

namespace GrpcApi.Services;

public class OrderController : OrderService.OrderServiceBase
{
    private readonly IOrderService _orderService;
    private readonly IEventPublisher _publisher;

    public OrderController(IOrderService orderService, IEventPublisher publisher)
    {
        _orderService = orderService;
        _publisher = publisher;
    }

    public override async Task<CreateOrdersResponse> CreateOrders(CreateOrdersRequest request, ServerCallContext context)
    {
        var createOrder = request.Orders.Select(FromProto.Map).ToList();

        List<Server.Order> response = await _orderService
            .CreateAsync(createOrder, context.CancellationToken)
            .ToListAsync(context.CancellationToken);
        var protoResponse = response.Select(ToProto.Map).ToList();

        IEnumerable<Server.Order> created = response.Where(order => order.State == Server.OrderStatus.OrderCreated);
        IEnumerable<ServiceEventToCreate> events = created.Select(order => new ServiceEventToCreate(order.Id, order.CreatedAt));
        await _publisher.NotifyAsync(events, context.CancellationToken);

        return new CreateOrdersResponse { Orders = { protoResponse } };
    }

    public override async Task<CreateItemsResponse> CreateItems(CreateItemsRequest request, ServerCallContext context)
    {
        var createItems = request.Items.Select(FromProto.Map).ToList();

        List<Server.OrderItem?> response = await _orderService
            .AddProductAsync(createItems, context.CancellationToken)
            .ToListAsync(context.CancellationToken);
        var responseProto = response.Select(ToProto.Map).ToList();
        return new CreateItemsResponse { Items = { responseProto } };
    }

    public override async Task<DeleteItemsResponse> DeleteItems(DeleteItemsRequest request, ServerCallContext context)
    {
        var deleteItems = request.Items.Select(FromProto.Map).ToList();

        await _orderService.DeleteProductAsync(deleteItems, context.CancellationToken);
        return new DeleteItemsResponse { Success = true };
    }

    public override async Task<OrdersToProcessingResponse> OrdersToProcessing(OrdersToProcessingRequest request, ServerCallContext context)
    {
        IEnumerable<ServiceEventToProcessing> events = request.OrdersId.Select(order => new ServiceEventToProcessing(order, DateTime.Now));
        await _publisher.NotifyAsync(events, context.CancellationToken);

        await _orderService.ToProcessingStatusAsync([.. request.OrdersId], context.CancellationToken);
        return new OrdersToProcessingResponse { Success = true };
    }

    public override async Task<OrdersToCancelledResponse> OrdersToCancelled(OrdersToCancelledRequest request, ServerCallContext context)
    {
        await _orderService.ToCancelledAsync([.. request.OrdersId], context.CancellationToken);
        return new OrdersToCancelledResponse { Success = true };
    }

    public override async Task<GetHistoryItemsResponse> GetHistoryItems(GetHistoryItemsRequest request, ServerCallContext context)
    {
        Server.HistoryType[] types = request.Types_.Select(FromProto.Map).ToArray();

        List<Server.HistoryItem> response = await _orderService
            .FindInHistoryAsync(request.OrderId, types, context.CancellationToken)
            .ToListAsync(context.CancellationToken);

        var protoResponse = response.Select(ToProto.Map).ToList();
        return new GetHistoryItemsResponse { Items = { protoResponse } };
    }
}
