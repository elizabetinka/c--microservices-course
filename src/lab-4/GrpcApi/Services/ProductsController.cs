using Grpc.Core;
using GrpcApi.Mappers;
using Task3.Services.Interfaces;

namespace GrpcApi.Services;

public class ProductsController : ProductService.ProductServiceBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    public override async Task<ProductsResponse> CreateProducts(ProductsRequest request, ServerCallContext context)
    {
        if (request.Products.Any(x => string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrEmpty(x.Name)))
        {
            throw new ArgumentException("Name is required");
        }

        var products = request.Products.Select(FromProto.Map).ToList();

        List<Task3.Models.ForDatabase.Product> response = await _productService
            .AddAsync(products, context.CancellationToken)
            .ToListAsync(context.CancellationToken);

        var protoResponse = response.Select(ToProto.Map).ToList();
        return new ProductsResponse { Products = { protoResponse } };
    }
}