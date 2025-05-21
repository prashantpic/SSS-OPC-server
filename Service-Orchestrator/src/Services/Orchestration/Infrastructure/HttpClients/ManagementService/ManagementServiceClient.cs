using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchestrationService.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For List

namespace OrchestrationService.Infrastructure.HttpClients.ManagementService
{
    // Placeholder DTOs based on IManagementServiceClient method signatures from SDS
    public class UserRolesResult // Was UserRoles in SDS
    {
        public string UserId { get; set; }
        public List<string> Roles { get; set; }
        // Other role details
    }

    public class DistributionDetailsResult // Was DistributionDetails in SDS
    {
        public string DistributionTargetId { get; set; }
        public List<string> Emails { get; set; } // Example
        public string TargetType { get; set; } // Example: "EmailGroup", "User"
        // Other distribution details
    }

    /// <summary>
    /// Implements <see cref="IManagementServiceClient"/> to make HTTP calls to the Management Service.
    /// Used by workflows requiring management data like user roles or distribution lists.
    /// Corresponds to REQ-7-022.
    /// </summary>
    public class ManagementServiceClient : IManagementServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ManagementServiceClient> _logger;
        private readonly ServiceEndpoints _serviceEndpoints;

        public ManagementServiceClient(HttpClient httpClient, IOptions<ServiceEndpoints> serviceEndpointsOptions, ILogger<ManagementServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceEndpoints = serviceEndpointsOptions?.Value ?? throw new ArgumentNullException(nameof(serviceEndpointsOptions));

            if (string.IsNullOrWhiteSpace(_serviceEndpoints.ManagementServiceUrl))
            {
                throw new ArgumentNullException(nameof(_serviceEndpoints.ManagementServiceUrl), "Management Service URL is not configured.");
            }
            _httpClient.BaseAddress = new Uri(_serviceEndpoints.ManagementServiceUrl);
        }

        /// <inheritdoc/>
        public async Task<UserRolesResult> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting user roles from Management Service for UserId: {UserId}", userId);
            try
            {
                // Adjust endpoint as per actual Management Service API
                var response = await _httpClient.GetAsync($"api/management/users/{Uri.EscapeDataString(userId)}/roles", cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<UserRolesResult>(cancellationToken: cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("Management Service returned null for user roles. UserId: {UserId}", userId);
                    throw new HttpRequestException($"Management Service returned null or invalid content for user roles: {userId}.");
                }
                _logger.LogInformation("User roles retrieved successfully from Management Service for UserId: {UserId}", userId);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Management Service for get-user-roles. UserId: {UserId}. Status: {StatusCode}. Message: {Message}", userId, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Management Service for get-user-roles. UserId: {UserId}. Message: {Message}", userId, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<DistributionDetailsResult> GetDistributionDetailsAsync(string distributionTargetId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting distribution details from Management Service for TargetId: {DistributionTargetId}", distributionTargetId);
            try
            {
                // Adjust endpoint as per actual Management Service API
                var response = await _httpClient.GetAsync($"api/management/distribution-targets/{Uri.EscapeDataString(distributionTargetId)}", cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var result = await response.Content.ReadFromJsonAsync<DistributionDetailsResult>(cancellationToken: cancellationToken);
                if (result == null)
                {
                    _logger.LogWarning("Management Service returned null for distribution details. TargetId: {DistributionTargetId}", distributionTargetId);
                    throw new HttpRequestException($"Management Service returned null or invalid content for distribution details: {distributionTargetId}.");
                }
                _logger.LogInformation("Distribution details retrieved successfully from Management Service for TargetId: {DistributionTargetId}", distributionTargetId);
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling Management Service for get-distribution-details. TargetId: {DistributionTargetId}. Status: {StatusCode}. Message: {Message}", distributionTargetId, ex.StatusCode, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error calling Management Service for get-distribution-details. TargetId: {DistributionTargetId}. Message: {Message}", distributionTargetId, ex.Message);
                throw;
            }
        }
    }
}