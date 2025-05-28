using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Task1.Extensions;
using Task1.Model;

namespace Task1.Library;

public class HttpServiceClient : IServiceClient, IDisposable
{
    private readonly HttpClient _client;

    private readonly string _requestString = "configurations?pageSize={0}&pageToken={1}";

    private readonly int _pageSize;

    private readonly JsonSerializerOptions _options;

    public HttpServiceClient(HttpClient httpClient, IOptions<ServiceOptions> options)
    {
        _client = httpClient;
        _client.BaseAddress = new Uri(options.Value.ConfigurationServerPath);
        _pageSize = options.Value.PageSize;
        _options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }

    public async IAsyncEnumerable<IEnumerable<ConfigurationItemDto>> RequestAsync([EnumeratorCancellation] CancellationToken ct)
    {
        string? pageToken = null;
        do
        {
            using var message =
                new HttpRequestMessage(HttpMethod.Get, string.Format(_requestString, _pageSize, pageToken));

            HttpResponseMessage response = await _client.SendAsync(message, ct);
            string responseString = await response.Content.ReadAsStringAsync(ct);

            QueryConfigurationsResponse? queryConfigurationsResponse =
                JsonSerializer.Deserialize<QueryConfigurationsResponse>(responseString, _options);

            queryConfigurationsResponse = queryConfigurationsResponse ?? throw new InvalidOperationException();
            pageToken = queryConfigurationsResponse.PageToken;

            if (queryConfigurationsResponse.Items.Any())
            {
                yield return queryConfigurationsResponse.Items;
            }
        }
        while (pageToken != null);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}