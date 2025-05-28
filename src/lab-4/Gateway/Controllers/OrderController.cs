using Gateway.Models.Request;
using Gateway.Models.Response;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly MediatorGrpcOrderClient _orderClient;

    public OrderController(MediatorGrpcOrderClient orderClient)
    {
        _orderClient = orderClient;
    }

    [HttpPost("/create")]
    [SwaggerOperation("create orders")]
    [ProducesResponseType(typeof(Order[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<Order[]> CreateOrderAsync(
        [FromBody] CreateOrder[] order,
        CancellationToken cancellationToken)
    {
        return await _orderClient.CreateOrderAsync(order, cancellationToken);
    }

    [HttpPost("/items/add")]
    [SwaggerOperation("add items to order")]
    [ProducesResponseType(typeof(OrderItem[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<OrderItem?[]> AddProductAsync(
        [FromBody] AddItem[] item,
        CancellationToken cancellationToken)
    {
        return await _orderClient.AddProductAsync(item, cancellationToken);
    }

    [HttpDelete("/items/delete")]
    [SwaggerOperation("delete items from orders")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<bool> DeleteProductAsync(
        [FromBody] RemoveItem[] item,
        CancellationToken cancellationToken)
    {
        return await _orderClient.DeleteProductAsync(item, cancellationToken);
    }

    [HttpPut("/items/processing")]
    [SwaggerOperation("set orders processing status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<bool> ToProcessingStatusAsync(
        [FromBody] long[] orderId,
        CancellationToken cancellationToken)
    {
        return await _orderClient.ToProcessingStatusAsync(orderId, cancellationToken);
    }

    [HttpPut("/items/cancelled")]
    [SwaggerOperation("set orders cancelled status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<bool> ToCancelledStatusAsync(
        [FromBody] long[] orderId,
        CancellationToken cancellationToken)
    {
        return await _orderClient.ToCancelledStatusAsync(orderId, cancellationToken);
    }

    [HttpGet("/history/{orderId}/find")]
    [SwaggerOperation("find history items by order id and type")]
    [ProducesResponseType(typeof(HistoryItem[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<HistoryItem[]> FindInHistoryAsync(
        [FromRoute] long orderId,
        [FromRoute] HistoryType[] types,
        CancellationToken cancellationToken)
    {
        return await _orderClient.FindInHistoryAsync(orderId, types, cancellationToken);
    }
}