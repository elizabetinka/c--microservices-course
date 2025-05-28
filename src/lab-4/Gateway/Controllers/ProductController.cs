using Gateway.Models.Request;
using Gateway.Models.Response;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly MediatorGrpcProductClient _client;

    public ProductController(MediatorGrpcProductClient client)
    {
        _client = client;
    }

    [HttpPost("/add")]
    [SwaggerOperation("add products")]
    [ProducesResponseType(typeof(Product[]), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<Product[]>> AddProductAsync(
        [FromBody] AddProduct[] product,
        CancellationToken cancellationToken)
    {
       return await _client.CreateProductsAsync(product, cancellationToken);
    }
}