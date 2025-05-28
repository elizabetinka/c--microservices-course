using Task3.Models.ForDatabase;
using Task3.Models.ForServices;

namespace Task3.Services.Interfaces;

public interface IProductService
{
    public Task<Product> AddAsync(AddProduct product, CancellationToken cancellationToken);

    public IAsyncEnumerable<Product> AddAsync(IEnumerable<AddProduct> products, CancellationToken cancellationToken);
}