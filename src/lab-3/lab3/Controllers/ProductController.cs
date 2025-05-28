using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Services.Interfaces;

namespace Lab3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost("/add/one")]
    [SwaggerOperation("add product")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(StatusCodeResult), 500)]
    public async Task<ActionResult<Product>> AddProductAsync(
        [FromQuery] AddProduct product,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrEmpty(product.Name))
        {
            throw new ArgumentException("Name is required");
        }

        Product result = await _productService.AddAsync(product, cancellationToken);
        return Ok(result);
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
        if (product.Any(x => string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrEmpty(x.Name)))
        {
            throw new ArgumentException("Name is required");
        }

        IList<Product> result = await _productService.AddAsync(product, cancellationToken).ToListAsync(cancellationToken);
        return Ok(result);
    }
}