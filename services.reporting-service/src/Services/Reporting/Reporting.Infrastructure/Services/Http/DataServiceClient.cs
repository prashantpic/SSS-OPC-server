using Reporting.Application.Contracts.Services;
using System.Net.Http.Json;

namespace Reporting.Infrastructure.Services.Http;

public class DataServiceClient : IDataServiceClient
{
    private readonly HttpClient _httpClient;

    public DataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<List<HistoricalDataDto>> GetHistoricalDataAsync(string tag, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        // Example endpoint: GET /api/data/historical?tag=...&start=...&end=...
        var response = await _httpClient.GetAsync($"api/data/historical?tag={tag}&startTime={startTime:O}&endTime={endTime:O}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<HistoricalDataDto>>(cancellationToken: cancellationToken);
        return result ?? new List<HistoricalDataDto>();
    }
}