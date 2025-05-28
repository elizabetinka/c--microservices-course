using Task3.Models.ForDatabase;
using Task3.Models.ForServices;
using Task3.Models.Payloads;

namespace Task3.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateAsync(CreateOrder createdOrder, CancellationToken cancellationToken);

    IAsyncEnumerable<Order> CreateAsync(
        IEnumerable<CreateOrder> createdOrder,
        CancellationToken cancellationToken);

    Task<OrderItem?> AddProductAsync(AddItem item, CancellationToken cancellationToken);

    IAsyncEnumerable<OrderItem?> AddProductAsync(
        IEnumerable<AddItem> items,
        CancellationToken cancellationToken);

    Task DeleteProductAsync(RemoveItem product, CancellationToken cancellationToken);

    Task DeleteProductAsync(IEnumerable<RemoveItem> products, CancellationToken cancellationToken);

    Task ToProcessingStatusAsync(long orderId, CancellationToken cancellationToken);

    Task ToProcessingStatusAsync(long[] orderId, CancellationToken cancellationToken);

    Task ToCompletedAsync(long orderId, CancellationToken cancellationToken);

    Task ToCompletedAsync(long[] orderId, CancellationToken cancellationToken);

    Task ToCreatedStatusAsync(long[] orderId, CancellationToken cancellationToken);

    Task ToCancelledAsync(long orderId, CancellationToken cancellationToken);

    Task ToCancelledAsync(long[] orderId, CancellationToken cancellationToken);

    IAsyncEnumerable<HistoryItem> FindInHistoryAsyncInfinity(long id, HistoryType[] types, CancellationToken cancellationToken);

    IAsyncEnumerable<HistoryItem> FindInHistoryAsync(long id, HistoryType[] types, CancellationToken cancellationToken);

    Task AddHistoryItemAsync(PayloadBaseModel payload, CancellationToken cancellationToken);
}