using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Adapters.DigitalTwin.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationService.Adapters.DigitalTwin
{
    public class HttpDigitalTwinAdaptor : IDigitalTwinAdaptor
    {
        private readonly DigitalTwinConfig _config;
        private readonly ILogger<HttpDigitalTwinAdaptor> _logger;
        private readonly HttpClient _httpClient;
        private readonly ICredentialManager _credentialManager;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public string PlatformId => _config.Id;

        public HttpDigitalTwinAdaptor(
            DigitalTwinConfig config,
            ILogger<HttpDigitalTwinAdaptor> logger,
            HttpClient httpClient, // Should be managed by HttpClientFactory
            ICredentialManager credentialManager,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _credentialManager = credentialManager ?? throw new ArgumentNullException(nameof(credentialManager));
            
            _retryPolicy = retryPolicyFactory.CreateAsyncHttpResponseRetryPolicy(3, TimeSpan.FromSeconds(2));
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.CreateAsyncHttpResponseCircuitBreakerPolicy(5, TimeSpan.FromSeconds(30));
        }

        private async Task EnsureHttpClientAuthorized(CancellationToken cancellationToken)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null && !string.IsNullOrEmpty(_config.CredentialIdentifier))
            {
                var token = await _credentialManager.GetCredentialAsync(_config.CredentialIdentifier, cancellationToken);
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                     _logger.LogInformation("Authorization token set for Digital Twin platform {PlatformId}.", _config.Id);
                }
                else
                {
                    _logger.LogWarning("Could not retrieve authorization token for Digital Twin platform {PlatformId} using identifier {CredentialIdentifier}.", _config.Id, _config.CredentialIdentifier);
                    // Decide if this is a critical failure
                }
            }
        }


        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_config.Endpoint))
            {
                _logger.LogError("Digital Twin endpoint for platform {PlatformId} is not configured.", _config.Id);
                throw new InvalidOperationException($"Endpoint not configured for Digital Twin platform {_config.Id}");
            }
            _httpClient.BaseAddress = new Uri(_config.Endpoint); // Set base address
            
            await EnsureHttpClientAuthorized(cancellationToken);

            // Optionally, make a test call to check connectivity / auth
            // var testUri = new Uri(_httpClient.BaseAddress, _config.Properties.GetValueOrDefault("HealthCheckPath", "/health"));
            // var response = await _httpClient.GetAsync(testUri, cancellationToken);
            // response.EnsureSuccessStatusCode();

            _logger.LogInformation("HttpDigitalTwinAdaptor for platform {PlatformId} initialized. Endpoint: {Endpoint}. Auth possibly set.", _config.Id, _config.Endpoint);
        }

        public async Task SyncDataAsync(string twinId, DigitalTwinUpdateRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(twinId)) throw new ArgumentNullException(nameof(twinId));
            if (request == null) throw new ArgumentNullException(nameof(request));

            await EnsureHttpClientAuthorized(cancellationToken);

            // Endpoint path for updating a twin, e.g., /digitaltwins/{twinId}
            // This should be configurable, possibly in _config.Properties
            var updatePath = _config.Properties.GetValueOrDefault("UpdateTwinPath", "/digitaltwins/{twinId}").Replace("{twinId}", twinId);
            var requestUri = new Uri(_httpClient.BaseAddress, updatePath);
            
            var payload = JsonSerializer.Serialize(request.PatchPayload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); // Assuming PatchPayload is the data
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json-patch+json"); // Or "application/json" if not PATCH

            _logger.LogDebug("Syncing data for twin {TwinId} to Digital Twin platform {PlatformId} at {RequestUri}", twinId, _config.Id, requestUri);

            HttpResponseMessage response = null;
            try
            {
                 response = await _retryPolicy.ExecuteAsync(ct => 
                    _circuitBreakerPolicy.ExecuteAsync(async token => 
                    {
                        // Typically PATCH for updates, but could be PUT or POST depending on API
                        var httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = httpContent }; 
                        return await _httpClient.SendAsync(httpRequest, token);
                    }, ct), 
                cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to sync data for twin {TwinId} on platform {PlatformId}. Status: {StatusCode}, Response: {ErrorContent}",
                        twinId, _config.Id, response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }
                 _logger.LogInformation("Successfully synced data for twin {TwinId} on platform {PlatformId}. Status: {StatusCode}", twinId, _config.Id, response.StatusCode);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to sync data for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error syncing data for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error syncing data for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<DigitalTwinModelInfo> GetDigitalTwinModelInfoAsync(string twinId, CancellationToken cancellationToken)
        {
             if (string.IsNullOrEmpty(twinId)) throw new ArgumentNullException(nameof(twinId));

            await EnsureHttpClientAuthorized(cancellationToken);

            // Endpoint path for getting model info for a twin, e.g., /digitaltwins/{twinId}/model
            var modelInfoPath = _config.Properties.GetValueOrDefault("GetModelInfoPath", "/digitaltwins/{twinId}/model").Replace("{twinId}", twinId);
            var requestUri = new Uri(_httpClient.BaseAddress, modelInfoPath);

            _logger.LogDebug("Getting model info for twin {TwinId} from Digital Twin platform {PlatformId} at {RequestUri}", twinId, _config.Id, requestUri);

            HttpResponseMessage response = null;
            try
            {
                response = await _retryPolicy.ExecuteAsync(ct => 
                    _circuitBreakerPolicy.ExecuteAsync(token => _httpClient.GetAsync(requestUri, token), ct), 
                cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to get model info for twin {TwinId} on platform {PlatformId}. Status: {StatusCode}, Response: {ErrorContent}",
                        twinId, _config.Id, response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var modelInfo = JsonSerializer.Deserialize<DigitalTwinModelInfo>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                _logger.LogInformation("Successfully retrieved model info for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                return modelInfo;
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to get model info for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error getting model info for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize model info for twin {TwinId} from platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting model info for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }

         // Example for sending telemetry, if the IDigitalTwinAdaptor interface evolves to include it
        public async Task SendTelemetryAsync(string twinId, IoTDataMessage telemetry, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(twinId)) throw new ArgumentNullException(nameof(twinId));
            if (telemetry == null) throw new ArgumentNullException(nameof(telemetry));

            await EnsureHttpClientAuthorized(cancellationToken);

            var telemetryPath = _config.Properties.GetValueOrDefault("SendTelemetryPath", "/digitaltwins/{twinId}/telemetry").Replace("{twinId}", twinId);
            var requestUri = new Uri(_httpClient.BaseAddress, telemetryPath);
            
            var payload = JsonSerializer.Serialize(telemetry.Payload);
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending telemetry for twin {TwinId} to Digital Twin platform {PlatformId} at {RequestUri}", twinId, _config.Id, requestUri);

            HttpResponseMessage response = null;
            try
            {
                 response = await _retryPolicy.ExecuteAsync(ct => 
                    _circuitBreakerPolicy.ExecuteAsync(async token => 
                    {
                        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = httpContent };
                        return await _httpClient.SendAsync(httpRequest, token);
                    }, ct), 
                cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to send telemetry for twin {TwinId} on platform {PlatformId}. Status: {StatusCode}, Response: {ErrorContent}",
                        twinId, _config.Id, response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }
                 _logger.LogInformation("Successfully sent telemetry for twin {TwinId} on platform {PlatformId}. Status: {StatusCode}", twinId, _config.Id, response.StatusCode);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to send telemetry for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error sending telemetry for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending telemetry for twin {TwinId} on platform {PlatformId}.", twinId, _config.Id);
                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }
    }
}