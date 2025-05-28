using Gateway.Mappers;
using GrpcApi;
using MyHttpRequest = Gateway.Models.Request;
using MyHttpResponse = Gateway.Models.Response;

namespace Gateway.Services;

public class MediatorGrpcOrderClient
{
    private readonly OrderService.OrderServiceClient _client;

    public MediatorGrpcOrderClient(OrderService.OrderServiceClient client)
    {
        _client = client;
    }

    public async Task<MyHttpResponse.Order[]> CreateOrderAsync(
        MyHttpRequest.CreateOrder[] order,
        CancellationToken cancellationToken)
    {
        var orders = order.Select(ToProto.Map).ToList();
        var request = new CreateOrdersRequest { Orders = { orders } };
        CreateOrdersResponse response = await _client.CreateOrdersAsync(request, cancellationToken: cancellationToken);
        return response.Orders.Select(FromProto.Map).ToArray();
    }

    public async Task<MyHttpResponse.OrderItem?[]> AddProductAsync(
        MyHttpRequest.AddItem[] item,
        CancellationToken cancellationToken)
    {
        var items = item.Select(ToProto.Map).ToList();
        var request = new CreateItemsRequest { Items = { items } };

        CreateItemsResponse response = await _client.CreateItemsAsync(request, cancellationToken: cancellationToken);

        return response.Items.Select(FromProto.Map).ToArray();
    }

    public async Task<bool> DeleteProductAsync(
        MyHttpRequest.RemoveItem[] item,
        CancellationToken cancellationToken)
    {
        var items = item.Select(ToProto.Map).ToList();
        var request = new DeleteItemsRequest { Items = { items } };

        DeleteItemsResponse response = await _client.DeleteItemsAsync(request, cancellationToken: cancellationToken);

        return response.Success;
    }

    public async Task<bool> ToProcessingStatusAsync(
        long[] orderId,
        CancellationToken cancellationToken)
    {
        var request = new OrdersToProcessingRequest { OrdersId = { orderId } };

        OrdersToProcessingResponse response =
            await _client.OrdersToProcessingAsync(request, cancellationToken: cancellationToken);

        return response.Success;
    }

    public async Task<bool> ToCompletedStatusAsync(
        long[] orderId,
        CancellationToken cancellationToken)
    {
        var request = new OrdersToCompletedRequest { OrdersId = { orderId } };

        OrdersToCompletedResponse response =
            await _client.OrdersToCompletedAsync(request, cancellationToken: cancellationToken);

        return response.Success;
    }

    public async Task<bool> ToCancelledStatusAsync(
        long[] orderId,
        CancellationToken cancellationToken)
    {
        var request = new OrdersToCancelledRequest { OrdersId = { orderId } };

        OrdersToCancelledResponse response =
            await _client.OrdersToCancelledAsync(request, cancellationToken: cancellationToken);

        return response.Success;
    }

    public async Task<MyHttpResponse.HistoryItem[]> FindInHistoryAsync(
        long orderId,
        MyHttpResponse.HistoryType[] types,
        CancellationToken cancellationToken)
    {
        var type = types.Select(ToProto.Map).ToList();
        var request = new GetHistoryItemsRequest
            { OrderId = orderId, Types_ = { type } };

        GetHistoryItemsResponse response = await _client.GetHistoryItemsAsync(request, cancellationToken: cancellationToken);

        return response.Items.Select(FromProto.Map).ToArray();
    }
}