using Reporting.Application.Contracts.Services;
using System.Net.Http.Json;

namespace Reporting.Infrastructure.Services.Http;

public class AiServiceClient : IAiServiceClient
{
    private readonly HttpClient _httpClient;

    public AiServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<List<AnomalyInsightDto>> GetAnomaliesForReportAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        // Example endpoint: GET /api/insights/anomalies?start=...&end=...
        var response = await _httpClient.GetAsync($"api/insights/anomalies?startTime={startTime:O}&endTime={endTime:O}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<AnomalyInsightDto>>(cancellationToken: cancellationToken);
        return result ?? new List<AnomalyInsightDto>();
    }

    public async Task<List<MaintenanceInsightDto>> GetMaintenancePredictionsAsync(List<string> assetIds, CancellationToken cancellationToken = default)
    {
        // Example endpoint: POST /api/insights/maintenance
        // Body: { "assetIds": ["id1", "id2"] }
        var response = await _httpClient.PostAsJsonAsync("api/insights/maintenance", new { AssetIds = assetIds }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<MaintenanceInsightDto>>(cancellationToken: cancellationToken);
        return result ?? new List<MaintenanceInsightDto>();
    }
}