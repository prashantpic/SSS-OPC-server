using ManagementService.Application.Abstractions.Services;
using ManagementService.Application.Abstractions.Services.Dto;
using ManagementService.Api.V1.DTOs; // For AggregatedKpisDto and ClientKpiFilterDto
using System.Net.Http.Json;
using System.Text.Json;
using System.Web; // For HttpUtility

namespace ManagementService.Infrastructure.HttpClients;

public class DataServiceApiClient : IDataServiceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataServiceApiClient> _logger;

    public DataServiceApiClient(IHttpClientFactory httpClientFactory, ILogger<DataServiceApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("DataServiceClient"); // Named client from Program.cs
        _logger = logger;
    }

    public async Task<bool> StoreClientKpiDataAsync(ClientKpiData data, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Storing KPI data for client {ClientInstanceId}", data.ClientInstanceId);
        try
        {
            // Assuming DataService endpoint: POST /api/kpis
            var response = await _httpClient.PostAsJsonAsync("api/kpis", data, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("KPI data stored successfully for client {ClientInstanceId}", data.ClientInstanceId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to store KPI data for client {ClientInstanceId}. Status: {StatusCode}, Response: {ErrorContent}",
                    data.ClientInstanceId, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing KPI data for client {ClientInstanceId}", data.ClientInstanceId);
            throw; // Allow Polly policies and error middleware to handle
        }
    }

    public async Task<AggregatedKpisDto> GetAggregatedKpisAsync(ClientKpiFilterDto? queryParams, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching aggregated KPIs from DataService.");
        var requestUri = "api/kpis/aggregated";

        if (queryParams != null)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            if (queryParams.ClientInstanceIds != null && queryParams.ClientInstanceIds.Any())
            {
                foreach(var id in queryParams.ClientInstanceIds)
                {
                    queryString.Add("clientInstanceIds", id.ToString());
                }
            }
            if (!string.IsNullOrWhiteSpace(queryParams.StatusFilter))
                queryString["statusFilter"] = queryParams.StatusFilter;
            if (queryParams.FromDate.HasValue)
                queryString["fromDate"] = queryParams.FromDate.Value.ToString("o"); // ISO 8601
            if (queryParams.ToDate.HasValue)
                queryString["toDate"] = queryParams.ToDate.Value.ToString("o");

            if (queryString.Count > 0)
            {
                requestUri += "?" + queryString.ToString();
            }
        }
        
        try
        {
            var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode(); // Throws for non-2xx status codes

            var result = await response.Content.ReadFromJsonAsync<AggregatedKpisDto>(cancellationToken: cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("DataService returned null for aggregated KPIs.");
                // Return a default/empty DTO or throw an exception based on expected behavior
                return new AggregatedKpisDto(0, 0, 0.0, 0.0); 
            }
            _logger.LogInformation("Aggregated KPIs fetched successfully from DataService.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching aggregated KPIs from DataService. Uri: {RequestUri}", _httpClient.BaseAddress + requestUri);
            throw; // Allow Polly policies and error middleware to handle
        }
    }
}