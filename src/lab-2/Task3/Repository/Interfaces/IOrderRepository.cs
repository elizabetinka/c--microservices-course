using Task3.Models.ForDatabase;

namespace Task3.Repository.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);

    public IAsyncEnumerable<Order> AddAsync(IEnumerable<Order> order, CancellationToken cancellationToken);

    Task ChangeStatusByIdAsync(long orderId, OrderStatus orderStatus, CancellationToken cancellationToken);

    public Task ChangeStatusByIdAsync(long[] orderId, OrderStatus orderStatus, CancellationToken cancellationToken);

    Task<Order?> FindByIdAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Order> FindByFiltersAsync(long[] id, OrderStatus[] statuses, string[] authors, int cursor, int pageSize, CancellationToken cancellationToken);
}