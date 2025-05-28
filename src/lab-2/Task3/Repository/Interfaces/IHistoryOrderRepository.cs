using Task3.Models.ForDatabase;

namespace Task3.Repository.Interfaces;

public interface IHistoryOrderRepository
{
    Task<HistoryItem> AddAsync(HistoryItem item, CancellationToken cancellationToken);

    IAsyncEnumerable<HistoryItem> AddAsync(IEnumerable<HistoryItem> item, CancellationToken cancellationToken);

    IAsyncEnumerable<HistoryItem> FindByFiltersAsync(long id, HistoryType[] historyTypes, int cursor, int pageSize, CancellationToken cancellationToken);
}