using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using ui.webapp.Models; // Assuming DTOs are in this namespace

namespace ui.webapp.ApiClient
{
    /// <summary>
    /// Provides a strongly-typed, testable interface to the backend's management APIs.
    /// </summary>
    public interface IManagementApiClient
    {
        Task<List<ClientInstanceDto>> GetClientInstancesAsync();
        Task<ClientConfigurationDto> GetClientConfigurationAsync(Guid clientId);
        Task<List<OpcNodeDto>> BrowseNamespaceAsync(Guid clientId, string nodeId);
        Task UpdateClientConfigurationAsync(Guid clientId, ClientConfigurationDto config);
        Task<BulkOperationResultDto> PerformBulkOperationAsync(BulkOperationDto operation);
    }

    /// <summary>
    /// A typed HttpClient for communicating with the Management Service via the API Gateway.
    /// </summary>
    public class ManagementApiClient : IManagementApiClient
    {
        private readonly HttpClient _httpClient;

        public ManagementApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<ClientInstanceDto>> GetClientInstancesAsync()
        {
            var response = await _httpClient.GetAsync("api/management/clients");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ClientInstanceDto>>() ?? new List<ClientInstanceDto>();
        }

        public async Task<ClientConfigurationDto> GetClientConfigurationAsync(Guid clientId)
        {
            var response = await _httpClient.GetAsync($"api/management/clients/{clientId}/configuration");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ClientConfigurationDto>() ?? throw new InvalidOperationException("Failed to deserialize client configuration.");
        }

        public async Task<List<OpcNodeDto>> BrowseNamespaceAsync(Guid clientId, string nodeId)
        {
            var encodedNodeId = HttpUtility.UrlEncode(nodeId);
            var response = await _httpClient.GetAsync($"api/management/clients/{clientId}/browse?nodeId={encodedNodeId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<OpcNodeDto>>() ?? new List<OpcNodeDto>();
        }

        public async Task UpdateClientConfigurationAsync(Guid clientId, ClientConfigurationDto config)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/management/clients/{clientId}/configuration", config);
            response.EnsureSuccessStatusCode();
        }

        public async Task<BulkOperationResultDto> PerformBulkOperationAsync(BulkOperationDto operation)
        {
            var response = await _httpClient.PostAsJsonAsync("api/management/clients/bulk", operation);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BulkOperationResultDto>() ?? throw new InvalidOperationException("Failed to deserialize bulk operation result.");
        }
    }
}