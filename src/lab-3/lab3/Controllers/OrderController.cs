using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Services.Interfaces;

namespace Lab3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("/create/one")]
    [SwaggerOperation("create order")]
    [ProducesResponseType(typeof(Order), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<Order>> CreateOrderAsync(
        [FromQuery] CreateOrder order,
        CancellationToken cancellationToken)
    {
        Order result = await _orderService.CreateAsync(order, cancellationToken);
        return Ok(result);
    }

    [HttpPost("/create")]
    [SwaggerOperation("create orders")]
    [ProducesResponseType(typeof(Order[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<Order[]>> CreateOrderAsync(
        [FromBody] CreateOrder[] order,
        CancellationToken cancellationToken)
    {
        IList<Order> result = await _orderService.CreateAsync(order, cancellationToken).ToListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("/items/add/one")]
    [SwaggerOperation("add item to order")]
    [ProducesResponseType(typeof(OrderItem), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<OrderItem>> AddProductAsync(
        [FromQuery] AddItem item,
        CancellationToken cancellationToken)
    {
        OrderItem? result = await _orderService.AddProductAsync(item, cancellationToken);
        return result == null ? throw new NpgsqlException("can't add product") : Ok(result);
    }

    [HttpPost("/items/add")]
    [SwaggerOperation("add items to order")]
    [ProducesResponseType(typeof(OrderItem[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<OrderItem[]>> AddProductAsync(
        [FromBody] AddItem[] item,
        CancellationToken cancellationToken)
    {
        IList<OrderItem?> result = await _orderService.AddProductAsync(item, cancellationToken).ToListAsync(cancellationToken);
        return result.Any(x => x == null) ? throw new NpgsqlException("can't add product") : (ActionResult<OrderItem[]>)Ok(result);
    }

    [HttpPost("/items/delete/one")]
    [SwaggerOperation("delete item from order")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> DeleteProductAsync(
        [FromQuery] RemoveItem item,
        CancellationToken cancellationToken)
    {
        await _orderService.DeleteProductAsync(item, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/delete")]
    [SwaggerOperation("delete items from orders")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> DeleteProductAsync(
        [FromBody] RemoveItem[] item,
        CancellationToken cancellationToken)
    {
        await _orderService.DeleteProductAsync(item, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/processing/one")]
    [SwaggerOperation("set order processing status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToProcessingStatusAsync(
        [FromQuery] long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToProcessingStatusAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/processing")]
    [SwaggerOperation("set orders processing status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToProcessingStatusAsync(
        [FromBody] long[] orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToProcessingStatusAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/completed/one")]
    [SwaggerOperation("set order completed status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToCompletedStatusAsync(
        [FromQuery] long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToCompletedAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/completed")]
    [SwaggerOperation("set orders completed status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToCompletedStatusAsync(
        [FromBody] long[] orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToCompletedAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/cancelled/one")]
    [SwaggerOperation("set order cancelled status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToCancelledStatusAsync(
        [FromQuery] long orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToCancelledAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/items/cancelled")]
    [SwaggerOperation("set orders cancelled status")]
    [ProducesResponseType(typeof(OkResult), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult> ToCancelledStatusAsync(
        [FromBody] long[] orderId,
        CancellationToken cancellationToken)
    {
        await _orderService.ToCancelledAsync(orderId, cancellationToken);
        return Ok();
    }

    [HttpPost("/history/{orderId}/find")]
    [SwaggerOperation("find history items by order id and type")]
    [ProducesResponseType(typeof(HistoryItem[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<HistoryItem[]>> FindInHistoryAsync(
        [FromRoute] long orderId,
        [FromBody] HistoryType[] types,
        CancellationToken cancellationToken)
    {
        IList<HistoryItem> result = await _orderService.FindInHistoryAsync(orderId, types, cancellationToken).ToListAsync(cancellationToken);
        return Ok(result);
    }
}