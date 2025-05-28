using Gateway.Mappers;
using GrpcApi;
using MyHttpRequest = Gateway.Models.Request;
using MyHttpResponse = Gateway.Models.Response;

namespace Gateway.Services;

public class MediatorGrpcProductClient
{
    private readonly ProductService.ProductServiceClient _client;

    public MediatorGrpcProductClient(ProductService.ProductServiceClient client)
    {
        _client = client;
    }

    public async Task<MyHttpResponse.Product[]> CreateProductsAsync(
        MyHttpRequest.AddProduct[] products,
        CancellationToken cancellationToken)
    {
        var products_ = products.Select(ToProto.Map).ToList();
        var request = new ProductsRequest { Products = { products_ } };

        ProductsResponse response = await _client.CreateProductsAsync(request, cancellationToken: cancellationToken);

        return response.Products.Select(FromProto.Map).ToArray();
    }
}