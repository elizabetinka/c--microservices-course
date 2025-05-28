using Refit;
using Task1.Model;

namespace Task1.Library;

public interface IConfiguratinRefitApi
{
    [Get("/configurations?pageSize={pageSize}&pageToken={pageToken}")]
    Task<QueryConfigurationsResponse> RequestAsync(int pageSize, string? pageToken, CancellationToken ct);
}