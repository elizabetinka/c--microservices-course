using Task3.Models.ForDatabase;

namespace Task3.Repository.Interfaces;

public interface IOrderItemsRepository
{
    Task<OrderItem> AddAsync(OrderItem item, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> AddAsync(IEnumerable<OrderItem> item, CancellationToken cancellationToken);

    Task SafeDeleteAsync(long itemId, long orderId, CancellationToken cancellationToken);

    Task SafeDeleteAsync(long[] itemId, long orderId, CancellationToken cancellationToken);

    public Task UpdateAsync(long productId, int quantity, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem> FindByFiltersAsync(long[] goodIds, long[] orderIds, bool deleted, int cursor, int pageSize, int atLeastQuantity, CancellationToken cancellationToken);
}