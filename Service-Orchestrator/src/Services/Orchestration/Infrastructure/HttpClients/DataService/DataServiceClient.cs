using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchestrationService.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For Dictionary

namespace OrchestrationService.Infrastructure.HttpClients.DataService
{
    // Placeholder DTOs based on IDataServiceClient method signatures from SDS
    public class DataQueryParameters
    {
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
        public string ReportType { get; set; } // Example
        public string AiAnalysisContext { get; set; } // Example from SDS
        // Add other query parameters
    }

    public class HistoricalDataResult // Was HistoricalData in SDS
    {
        public string DataReference { get; set; } // Could be a URI, an ID, or a summary
        public string ContentType { get; set; } // E.g. "application/json", "text/csv"
        public byte[] RawData { get; set; } // If data is returned directly
        // Other result details
    }

    /// <summary>
    /// Implements <see cref="IDataServiceClient"/> to make HTTP calls to the Data Service.
    /// Handles interaction for data persistence and retrieval tasks within workflows.
    /// Corresponds to REQ-7-020, REQ-7-022, REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class DataServiceClient : IDataServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataServiceClient> _logger;
        private readonly ServiceEndpoints _serviceEndpoints;

        public DataServiceClient(HttpClient httpClient, IOptions<ServiceEndpoints> serviceEndpointsOptions, ILogger<DataServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceEndpoints = serviceEndpointsOptions?.Value ?? throw new ArgumentNullException(nameof(serviceEndpointsOptions));

            if (string.IsNullOrWhiteSpace(_serviceEndpoints.DataServiceUrl))
            {
                throw new ArgumentNullException(nameof(_serviceEndpoints.DataServiceUrl), "Data Service URL is not configured.");
            }
            _httpClient.BaseAddress = new Uri(_serviceEndpoints.DataServiceUrl);
        }

        /// <inheritdoc/>
        public async Task<HistoricalDataResult> QueryHistoricalDataAsync(DataQueryParameters parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Querying historical data from Data Service.");
            try
            {
                // Adjust endpoint and request format as per actual Data Service API
                var response = await _httpClient.PostAsJsonAsync("api/data/query-historical", parameters, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<HistoricalDataResult>(cancellationToken: cancellationToken);
                 if (result == null)
                {
                    _logger.LogWarning("Data Service returned null for historical data query.");
                    // Depending on requirements, this might not be an error but simply no data found.
                    // For now, let's assume it's unexpected if it's null, but an empty DataReference might be valid.
                    // The activity should check DataReference.
                }
                _logger.LogInformation("Historical data queried successfully from Data Service. Reference: {DataReference}", result?.DataReference);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Data Service for query-historical. Status: {StatusCode}. Message: {Message}", ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Data Service for query-historical. Message: {Message}", ex.Message);
                throw;
            }
        }
        
        /// <inheritdoc/>
        public async Task ArchiveReportAsync(string reportId, string documentUri, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Archiving report with Data Service. ReportId: {ReportId}, DocumentUri: {DocumentUri}", reportId, documentUri);
            try
            {
                var payload = new { ReportId = reportId, DocumentUri = documentUri };
                // Adjust endpoint as per actual Data Service API
                var response = await _httpClient.PostAsJsonAsync("api/data/archive-report", payload, cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Report archived successfully via Data Service. ReportId: {ReportId}", reportId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Data Service for archive-report. ReportId: {ReportId}. Status: {StatusCode}. Message: {Message}", reportId, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Data Service for archive-report. ReportId: {ReportId}. Message: {Message}", reportId, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> StoreOffChainDataAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Storing off-chain data with Data Service. Data Length: {DataLength}", data?.Length);
            if (data == null || data.Length == 0)
            {
                _logger.LogWarning("Attempted to store null or empty off-chain data.");
                throw new ArgumentNullException(nameof(data), "Cannot store null or empty data off-chain.");
            }
            try
            {
                // Using ByteArrayContent for binary data
                using var content = new ByteArrayContent(data);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); // Or appropriate type
                
                // Adjust endpoint as per actual Data Service API
                var response = await _httpClient.PostAsync("api/data/store-offchain", content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                // Assuming the service returns the storage path/URI as a JSON string or plain text
                var result = await response.Content.ReadFromJsonAsync<OffChainStorageResult>(cancellationToken: cancellationToken); // Assuming a DTO like { "storagePath": "..." }
                if (result == null || string.IsNullOrWhiteSpace(result.StoragePath))
                {
                     _logger.LogError("Data Service returned null or empty storage path for off-chain data.");
                    throw new HttpRequestException("Data Service did not return a valid storage path for off-chain data.");
                }
                _logger.LogInformation("Off-chain data stored successfully via Data Service. Path: {StoragePath}", result.StoragePath);
                return result.StoragePath;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Data Service for store-offchain. Status: {StatusCode}. Message: {Message}", ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Data Service for store-offchain. Message: {Message}", ex.Message);
                throw;
            }
        }
        
        private class OffChainStorageResult // Helper DTO for deserialization
        {
            public string StoragePath { get; set; }
        }

        /// <inheritdoc/>
        public async Task DeleteDocumentAsync(string documentUri, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting document with Data Service. DocumentUri: {DocumentUri}", documentUri);
            try
            {
                // Adjust endpoint as per actual Data Service API. Using query parameter for URI for example.
                var response = await _httpClient.DeleteAsync($"api/data/document?uri={Uri.EscapeDataString(documentUri)}", cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Document deleted successfully via Data Service. DocumentUri: {DocumentUri}", documentUri);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Data Service for delete-document. DocumentUri: {DocumentUri}. Status: {StatusCode}. Message: {Message}", documentUri, ex.StatusCode, ex.Message);
                // In compensation, often we log and continue, not re-throw, unless the workflow should halt.
                // For now, let activity decide to re-throw.
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Data Service for delete-document. DocumentUri: {DocumentUri}. Message: {Message}", documentUri, ex.Message);
                throw;
            }
        }
    }
}