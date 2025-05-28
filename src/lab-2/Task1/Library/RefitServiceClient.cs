using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Task1.Extensions;
using Task1.Model;

namespace Task1.Library;

public class RefitServiceClient : IServiceClient
{
    private readonly IConfiguratinRefitApi _refitApi;
    private readonly int _pageSize;

    public RefitServiceClient(IConfiguratinRefitApi api, IOptions<ServiceOptions> options)
    {
        _pageSize = options.Value.PageSize;
        _refitApi = api;
    }

    public async IAsyncEnumerable<IEnumerable<ConfigurationItemDto>> RequestAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        string? pageToken = null;
        do
        {
            QueryConfigurationsResponse queryConfigurationsResponse =
                await _refitApi.RequestAsync(_pageSize, pageToken, ct);

            queryConfigurationsResponse = queryConfigurationsResponse ?? throw new InvalidOperationException();
            pageToken = queryConfigurationsResponse.PageToken;

            if (queryConfigurationsResponse.Items.Any())
            {
                yield return queryConfigurationsResponse.Items;
            }
        }
        while (pageToken != null);
    }
}