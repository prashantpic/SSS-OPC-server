using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchestrationService.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For Dictionary

namespace OrchestrationService.Infrastructure.HttpClients.IntegrationService
{
    // Placeholder DTOs based on IIntegrationServiceClient method signatures from SDS
    public class DistributionTargetDetails // Was DistributionTarget in SDS
    {
        public string TargetIdentifier { get; set; } // e.g., email, group ID, file path pattern
        public List<string> Recipients { get; set; } // If resolved
        public Dictionary<string, string> Parameters { get; set; } // Other params like subject, body template
    }

    public class BlockchainMetadata
    {
        public string SourceSystem { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> CustomData { get; set; }
    }

    public class CommitToBlockchainResponse // Helper for deserialization
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
    }


    /// <summary>
    /// Implements <see cref="IIntegrationServiceClient"/> to make HTTP calls to the Integration Service.
    /// Handles interaction for external systems like email or blockchain.
    /// Corresponds to REQ-7-020, REQ-8-007.
    /// </summary>
    public class IntegrationServiceClient : IIntegrationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IntegrationServiceClient> _logger;
        private readonly ServiceEndpoints _serviceEndpoints;

        public IntegrationServiceClient(HttpClient httpClient, IOptions<ServiceEndpoints> serviceEndpointsOptions, ILogger<IntegrationServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceEndpoints = serviceEndpointsOptions?.Value ?? throw new ArgumentNullException(nameof(serviceEndpointsOptions));

            if (string.IsNullOrWhiteSpace(_serviceEndpoints.IntegrationServiceUrl))
            {
                throw new ArgumentNullException(nameof(_serviceEndpoints.IntegrationServiceUrl), "Integration Service URL is not configured.");
            }
            _httpClient.BaseAddress = new Uri(_serviceEndpoints.IntegrationServiceUrl);
        }

        /// <inheritdoc/>
        public async Task DistributeReportAsync(string documentUri, DistributionTargetDetails target, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Distributing report via Integration Service. DocumentUri: {DocumentUri}, Target: {TargetIdentifier}", documentUri, target?.TargetIdentifier);
            try
            {
                var payload = new { DocumentUri = documentUri, TargetDetails = target };
                // Adjust endpoint as per actual Integration Service API
                var response = await _httpClient.PostAsJsonAsync("api/integration/distribute-report", payload, cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Report distribution initiated successfully via Integration Service for DocumentUri: {DocumentUri}", documentUri);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Integration Service for distribute-report. DocumentUri: {DocumentUri}. Status: {StatusCode}. Message: {Message}", documentUri, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Integration Service for distribute-report. DocumentUri: {DocumentUri}. Message: {Message}", documentUri, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> CommitToBlockchainAsync(string dataHash, string offChainRef, BlockchainMetadata metadata, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Committing to blockchain via Integration Service. Hash: {DataHash}, OffChainRef: {OffChainRef}", dataHash, offChainRef);
            try
            {
                var payload = new { DataHash = dataHash, OffChainReference = offChainRef, Metadata = metadata };
                // Adjust endpoint as per actual Integration Service API
                var response = await _httpClient.PostAsJsonAsync("api/integration/commit-blockchain", payload, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<CommitToBlockchainResponse>(cancellationToken: cancellationToken);
                if (result == null || string.IsNullOrEmpty(result.TransactionId))
                {
                    _logger.LogError("Integration Service returned null or empty transaction ID for blockchain commit. Hash: {DataHash}", dataHash);
                    throw new HttpRequestException("Integration Service did not return a valid transaction ID for blockchain commit.");
                }
                _logger.LogInformation("Blockchain commit initiated successfully via Integration Service. TransactionId: {TransactionId}", result.TransactionId);
                return result.TransactionId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Integration Service for commit-blockchain. Hash: {DataHash}. Status: {StatusCode}. Message: {Message}", dataHash, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Integration Service for commit-blockchain. Hash: {DataHash}. Message: {Message}", dataHash, ex.Message);
                throw;
            }
        }

        // Placeholder for sending failure notifications, if defined by Integration service
        public async Task SendFailureNotificationAsync(string subject, string body, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to send failure notification: {Subject}", subject);
            try
            {
                // var payload = new { Subject = subject, Body = body, Recipients = new[] { "admin@example.com" } }; // Example
                // var response = await _httpClient.PostAsJsonAsync("api/integration/notify-failure", payload, cancellationToken);
                // response.EnsureSuccessStatusCode();
                _logger.LogWarning("SendFailureNotificationAsync is a placeholder and not fully implemented. Subject: {Subject}", subject);
                await Task.CompletedTask; // Replace with actual call
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending failure notification {Subject}: {Message}", subject, ex.Message);
                // Do not re-throw from notification usually, just log.
            }
        }
    }
}