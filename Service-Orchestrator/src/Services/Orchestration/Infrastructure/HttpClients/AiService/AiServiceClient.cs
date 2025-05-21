using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchestrationService.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For Dictionary

namespace OrchestrationService.Infrastructure.HttpClients.AiService
{
    // Placeholder DTOs based on IAiServiceClient method signatures from SDS
    // These should ideally be in a shared DTOs library or defined more concretely.
    public class AiServiceRequestDto
    {
        public string ReportType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        // Add other necessary fields for AI analysis initiation
    }

    public class AiAnalysisResult
    {
        public string ResultUri { get; set; }
        public string Status { get; set; }
        // Other result details
    }

    public class ReportGenerationInputForDocument // Was ReportGenerationData in SDS for IAiServiceClient
    {
        public string ReportType { get; set; }
        public Dictionary<string, object> CustomParameters { get; set; }
        public string AiAnalysisResultUri { get; set; }
        public string HistoricalDataRef { get; set; }
        // Other necessary fields
    }

    public class ReportDocumentResult // Was ReportDocument in SDS
    {
        public string DocumentUri { get; set; }
        public string Format { get; set; }
        // Other document details
    }


    /// <summary>
    /// Implements <see cref="IAiServiceClient"/> to make HTTP calls to the AI Processing Service.
    /// Handles request/response serialization, error handling for communication.
    /// Corresponds to REQ-7-020.
    /// </summary>
    public class AiServiceClient : IAiServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiServiceClient> _logger;
        private readonly ServiceEndpoints _serviceEndpoints;

        public AiServiceClient(HttpClient httpClient, IOptions<ServiceEndpoints> serviceEndpointsOptions, ILogger<AiServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceEndpoints = serviceEndpointsOptions?.Value ?? throw new ArgumentNullException(nameof(serviceEndpointsOptions));
            
            if (string.IsNullOrWhiteSpace(_serviceEndpoints.AiServiceUrl))
            {
                throw new ArgumentNullException(nameof(_serviceEndpoints.AiServiceUrl), "AI Service URL is not configured.");
            }
            _httpClient.BaseAddress = new Uri(_serviceEndpoints.AiServiceUrl);
        }

        /// <inheritdoc/>
        public async Task<AiAnalysisResult> InitiateAnalysisAsync(AiServiceRequestDto parameters, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initiating AI analysis with AI Service. ReportType: {ReportType}", parameters?.ReportType);
            try
            {
                // Adjust endpoint as per actual AI Service API definition
                var response = await _httpClient.PostAsJsonAsync("api/ai/initiate-analysis", parameters, cancellationToken);
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for non-2xx responses
                
                var result = await response.Content.ReadFromJsonAsync<AiAnalysisResult>(cancellationToken: cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("AI Service returned null for initiate-analysis. ReportType: {ReportType}", parameters?.ReportType);
                    throw new HttpRequestException("AI Service returned null or invalid content for analysis initiation.");
                }
                _logger.LogInformation("AI analysis initiated successfully via AI Service. Result URI: {ResultUri}", result.ResultUri);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling AI Service for initiate-analysis. Status: {StatusCode}. Message: {Message}", ex.StatusCode, ex.Message);
                throw; // Re-throw for workflow activity to handle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling AI Service for initiate-analysis. Message: {Message}", ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ReportDocumentResult> GenerateReportDocumentAsync(ReportGenerationInputForDocument reportData, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating report document with AI Service. ReportType: {ReportType}", reportData?.ReportType);
            try
            {
                 // Adjust endpoint as per actual AI Service API definition
                var response = await _httpClient.PostAsJsonAsync("api/ai/generate-document", reportData, cancellationToken);
                response.EnsureSuccessStatusCode(); 
                
                var result = await response.Content.ReadFromJsonAsync<ReportDocumentResult>(cancellationToken: cancellationToken);
                if (result == null)
                {
                     _logger.LogWarning("AI Service returned null for generate-document. ReportType: {ReportType}", reportData?.ReportType);
                    throw new HttpRequestException("AI Service returned null or invalid content for document generation.");
                }
                _logger.LogInformation("Report document generated successfully via AI Service. Document URI: {DocumentUri}", result.DocumentUri);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling AI Service for generate-document. Status: {StatusCode}. Message: {Message}", ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling AI Service for generate-document. Message: {Message}", ex.Message);
                throw;
            }
        }

        // Placeholder for potential AI resource cleanup, if defined by AI service
        public async Task CleanupAnalysisAsync(string analysisResultUri, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to cleanup AI analysis resource: {AnalysisResultUri}", analysisResultUri);
            try
            {
                // Example: could be a DELETE request
                // var response = await _httpClient.DeleteAsync($"api/ai/analysis-resource/{Uri.EscapeDataString(analysisResultUri)}", cancellationToken);
                // response.EnsureSuccessStatusCode();
                _logger.LogWarning("CleanupAnalysisAsync is a placeholder and not fully implemented. URI: {AnalysisResultUri}", analysisResultUri);
                await Task.CompletedTask; // Replace with actual call if AI service supports it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up AI analysis resource {AnalysisResultUri}: {Message}", analysisResultUri, ex.Message);
                // Do not re-throw from compensation cleanup usually, just log.
            }
        }
    }
}