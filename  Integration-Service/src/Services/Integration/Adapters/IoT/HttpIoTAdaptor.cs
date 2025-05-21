using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Adapters.IoT.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationService.Adapters.IoT
{
    public class HttpIoTAdaptor : IIoTPlatformAdaptor
    {
        private readonly IoTPlatformConfig _config;
        private readonly ILogger<HttpIoTAdaptor> _logger;
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        // private Func<IoTCommand, CancellationToken, Task> _onCommandReceived; // Webhook based command reception is complex for a generic adapter

        public string PlatformType => "HTTP";
        public string PlatformId => _config.Id;

        public HttpIoTAdaptor(
            IoTPlatformConfig config,
            ILogger<HttpIoTAdaptor> logger,
            HttpClient httpClient, // Should be managed by HttpClientFactory
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            _retryPolicy = retryPolicyFactory.CreateAsyncHttpResponseRetryPolicy(3, TimeSpan.FromSeconds(2));
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.CreateAsyncHttpResponseCircuitBreakerPolicy(5, TimeSpan.FromSeconds(30));
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            // For HTTP, "connection" is typically per-request.
            // We can check base URI and auth setup here if needed.
            if (string.IsNullOrEmpty(_config.Endpoint))
            {
                _logger.LogError("HTTP endpoint for platform {PlatformId} is not configured.", _config.Id);
                throw new InvalidOperationException($"Endpoint not configured for HTTP IoT platform {_config.Id}");
            }
            _logger.LogInformation("HTTP IoT Adaptor for platform {PlatformId} initialized with endpoint {Endpoint}.", _config.Id, _config.Endpoint);
            // Potentially pre-configure HttpClient base address and default headers if not done by factory
            // _httpClient.BaseAddress = new Uri(_config.Endpoint);
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken)
        {
            // HTTP is stateless, so no explicit disconnect needed beyond disposing HttpClient if owned by this class (typically not).
            _logger.LogInformation("HTTP IoT Adaptor for platform {PlatformId} 'disconnected' (stateless operation).", _config.Id);
            return Task.CompletedTask;
        }

        public async Task PublishTelemetryAsync(IoTDataMessage message, CancellationToken cancellationToken)
        {
            var telemetryEndpoint = _config.Properties.GetValueOrDefault("TelemetryEndpoint") ?? ""; // e.g., "/api/telemetry" or full URL if _config.Endpoint is just base
            var requestUri = new Uri(new Uri(_config.Endpoint), telemetryEndpoint); // Combine base and relative

            var payload = JsonSerializer.Serialize(message.Payload);
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

            // Apply authentication if configured
            if (_config.Authentication?.Type.Equals("ApiKey", StringComparison.OrdinalIgnoreCase) == true &&
                _config.Authentication.Properties.TryGetValue("ApiKeyHeaderName", out var headerName) &&
                _config.Authentication.Properties.TryGetValue("ApiKeyValue", out var keyValue)) // Real app: use ICredentialManager
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerName, keyValue);
            }
            // Add other auth types like Bearer token

            _logger.LogDebug("Publishing telemetry to HTTP endpoint {RequestUri} for platform {PlatformId}.", requestUri, _config.Id);

            HttpResponseMessage response = null;
            try
            {
                // Combine policies: retry wraps circuit breaker
                response = await _retryPolicy.ExecuteAsync(ct => 
                    _circuitBreakerPolicy.ExecuteAsync(async token => 
                    {
                        // Create HttpRequestMessage to allow configuring method (POST/PUT) per call
                        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = httpContent };
                        return await _httpClient.SendAsync(request, token);
                    }, ct), 
                cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to publish telemetry to HTTP platform {PlatformId}. Status: {StatusCode}, Response: {ErrorContent}", 
                        _config.Id, response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode(); // Throws HttpRequestException
                }
                _logger.LogInformation("Successfully published telemetry to HTTP platform {PlatformId}. Status: {StatusCode}", _config.Id, response.StatusCode);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to publish telemetry to HTTP platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error publishing telemetry to platform {PlatformId}.", _config.Id);
                throw; // Rethrow to indicate failure
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error publishing telemetry to HTTP platform {PlatformId}.", _config.Id);
                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }

        public Task SubscribeToCommandsAsync(Func<IoTCommand, CancellationToken, Task> onCommandReceived, CancellationToken cancellationToken)
        {
            // _onCommandReceived = onCommandReceived;
            _logger.LogWarning("Subscribing to commands via generic HTTP GET/POST is not typically supported for real-time bi-directional IoT without platform-specific mechanisms like WebSockets, long polling, or webhooks. Platform {PlatformId} might require a specific implementation or webhook registration.", _config.Id);
            // If a platform supports command retrieval via polling, it could be implemented here.
            // Otherwise, this adapter might only support outbound communication.
            // A webhook setup would mean this service needs an inbound HTTP endpoint, not handled by this adapter directly.
            return Task.CompletedTask; // Or throw NotImplementedException
        }

        public async Task SendIoTCommandResponseAsync(IoTCommandResponse response, CancellationToken cancellationToken)
        {
            // This assumes there's an HTTP endpoint to post command responses to.
            var responseEndpoint = _config.Properties.GetValueOrDefault("CommandResponseEndpoint")?.Replace("{CommandId}", response.CommandId) ?? "";
            if (string.IsNullOrEmpty(responseEndpoint))
            {
                _logger.LogWarning("CommandResponseEndpoint not configured for HTTP IoT platform {PlatformId}. Cannot send command response.", _config.Id);
                throw new InvalidOperationException("CommandResponseEndpoint not configured.");
            }
            var requestUri = new Uri(new Uri(_config.Endpoint), responseEndpoint);

            var payload = JsonSerializer.Serialize(response.Payload);
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            
            // Apply authentication (similar to PublishTelemetryAsync)

            _logger.LogDebug("Sending command response to HTTP endpoint {RequestUri} for platform {PlatformId}.", requestUri, _config.Id);
            
            HttpResponseMessage httpResponse = null;
            try
            {
                 httpResponse = await _retryPolicy.ExecuteAsync(ct => 
                    _circuitBreakerPolicy.ExecuteAsync(async token => 
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = httpContent };
                        return await _httpClient.SendAsync(request, token);
                    }, ct), 
                cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Failed to send command response to HTTP platform {PlatformId}. Status: {StatusCode}, Response: {ErrorContent}", 
                        _config.Id, httpResponse.StatusCode, errorContent);
                    httpResponse.EnsureSuccessStatusCode();
                }
                 _logger.LogInformation("Successfully sent command response to HTTP platform {PlatformId}. Status: {StatusCode}", _config.Id, httpResponse.StatusCode);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to send command response to HTTP platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error sending command response to platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending command response to HTTP platform {PlatformId}.", _config.Id);
                throw;
            }
            finally
            {
                httpResponse?.Dispose();
            }
        }
    }
}