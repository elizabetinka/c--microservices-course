using Task1.Model;

namespace Task1.Library;

public interface IServiceClient
{
    IAsyncEnumerable<IEnumerable<ConfigurationItemDto>> RequestAsync(CancellationToken ct);
}