using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Repository.Interfaces;
using Task3.Services.Interfaces;

namespace Task3.Services;

public class ProductService : IProductService
{
    private readonly IProductsRepository _repository;

    public ProductService(IProductsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Product> AddAsync(AddProduct product, CancellationToken cancellationToken)
    {
        return await _repository.AddAsync(new Product(product.Name, product.Price), cancellationToken);
    }

    public IAsyncEnumerable<Product> AddAsync(IEnumerable<AddProduct> products, CancellationToken cancellationToken)
    {
        IList<Product> products2 = products.Select(x => new Product(x.Name, x.Price)).ToList();
        return _repository.AddAsync(products2, cancellationToken);
    }
}