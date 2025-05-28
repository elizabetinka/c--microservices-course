using Task3.Models.ForDatabase;

namespace Task3.Repository.Interfaces;

public interface IProductsRepository
{
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> AddAsync(IEnumerable<Product> product, CancellationToken cancellationToken);

    IAsyncEnumerable<Product> FindByFiltersAsync(long[] id, string? pattern, decimal? minCost, decimal? maxCost, int cursor, int pageSize, CancellationToken cancellationToken);
}