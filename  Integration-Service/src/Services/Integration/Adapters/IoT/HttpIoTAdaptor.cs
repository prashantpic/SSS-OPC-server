using IntegrationService.Adapters.IoT.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntegrationService.Adapters.IoT
{
    /// <summary>
    /// IoT Platform adaptor using HTTP/HTTPS protocol.
    /// </summary>
    public class HttpIoTAdaptor : IIoTPlatformAdaptor, IDisposable
    {
        public string Id { get; }
        private readonly ILogger<HttpIoTAdaptor> _logger;
        private readonly ICredentialManager _credentialManager;
        private readonly IoTPlatformConfig _config;
        private readonly HttpClient _httpClient;
        private readonly RetryPolicy _retryPolicy;
        private readonly CircuitBreakerPolicy _circuitBreakerPolicy;

        public bool IsConnected => true; 

        public HttpIoTAdaptor(
            IoTPlatformConfig config,
            ILogger<HttpIoTAdaptor> logger,
            ICredentialManager credentialManager,
            IHttpClientFactory httpClientFactory, 
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            Id = config.Id;
            _config = config;
            _logger = logger;
            _credentialManager = credentialManager;
            _httpClient = httpClientFactory.CreateClient(Id); 
            _retryPolicy = retryPolicyFactory.GetDefaultRetryPolicy(); 
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.GetDefaultCircuitBreakerPolicy(); 

            _httpClient.BaseAddress = new Uri(_config.Endpoint);
            _httpClient.Timeout = TimeSpan.FromSeconds(30); 

            _logger.LogInformation("HttpIoTAdaptor '{Id}' initialized for endpoint {Endpoint}", Id, _config.Endpoint);
        }

        public Task ConnectAsync()
        {
             _logger.LogInformation("HttpIoTAdaptor '{Id}' ConnectAsync (HTTP is stateless, no explicit connection).", Id);
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
             _logger.LogInformation("HttpIoTAdaptor '{Id}' DisconnectAsync (HTTP is stateless, no explicit disconnect).", Id);
            return Task.CompletedTask;
        }

        public async Task SendTelemetryAsync(IoTDataMessage message)
        {
             _logger.LogDebug("Sending telemetry for device {DeviceId} via HttpIoTAdaptor '{Id}' to {Endpoint}", message.DeviceId, Id, _config.Endpoint);

            string stringPayload;
            string contentType;

            try
            {
                if (_config.DataFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                {
                    stringPayload = JsonSerializer.Serialize(message); 
                    contentType = "application/json";
                     _logger.LogTrace("Serialized JSON payload for device {DeviceId}: {JsonPayload}", message.DeviceId, stringPayload);
                }
                else
                {
                     _logger.LogError("Unsupported data format '{DataFormat}' for sending HTTP telemetry on '{Id}'.", _config.DataFormat, Id);
                    throw new System.NotSupportedException($"Unsupported data format: {_config.DataFormat}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize telemetry message for device {DeviceId} using format {DataFormat}.", message.DeviceId, _config.DataFormat);
                throw; 
            }

            var request = new HttpRequestMessage(HttpMethod.Post, ""); 
            request.Content = new StringContent(stringPayload, Encoding.UTF8, contentType);
            await ApplyAuthenticationHeadersAsync(request.Headers);

            try
            {
                 await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                         _logger.LogDebug("Sending HTTP POST telemetry for device {DeviceId} via '{Id}'...", message.DeviceId, Id);
                        var response = await _httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode(); 
                         _logger.LogTrace("Successfully sent HTTP telemetry for device {DeviceId} via '{Id}'. Status: {StatusCode}", message.DeviceId, Id, response.StatusCode);
                    }));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to send HTTP telemetry for device {DeviceId} via '{Id}' after retries.", message.DeviceId, Id);
                 throw; 
            }
        }

        private async Task ApplyAuthenticationHeadersAsync(HttpRequestHeaders headers)
        {
            _logger.LogDebug("Applying authentication headers for HTTP adaptor '{Id}'. Type: {AuthType}", Id, _config.Authentication.Type);
            switch (_config.Authentication.Type)
            {
                case "ApiKey":
                    if (!string.IsNullOrEmpty(_config.Authentication.ApiKeyHeaderName) && !string.IsNullOrEmpty(_config.Authentication.CredentialKey))
                    {
                        var apiKey = await _credentialManager.GetCredentialAsync(_config.Authentication.CredentialKey);
                        headers.Add(_config.Authentication.ApiKeyHeaderName, apiKey);
                         _logger.LogTrace("Added API Key header '{HeaderName}' for '{Id}'.", _config.Authentication.ApiKeyHeaderName, Id);
                    }
                    break;
                 case "BearerToken":
                     _logger.LogWarning("Bearer token authentication placeholder for '{Id}'. Token retrieval logic needed.", Id);
                     break;
                case "None":
                    break;
                default:
                    _logger.LogWarning("Unsupported HTTP authentication type: {AuthType}", _config.Authentication.Type);
                    break;
            }
        }

        private System.Action<IoTCommand>? _onCommandReceivedCallback;
         public Task SubscribeToCommandsAsync(System.Action<IoTCommand> onCommandReceived)
        {
             if (!_config.EnableBiDirectional)
            {
                _logger.LogWarning("Bi-directional communication is disabled for HTTP adaptor '{Id}'. Cannot subscribe to commands via simple HTTP.", Id);
                return Task.CompletedTask;
            }

            _logger.LogWarning("HTTP adaptor '{Id}' typically does not support direct subscriptions for incoming commands. This requires a different mechanism (e.g., polling an endpoint, WebSockets, or the platform pushing commands via a different protocol). Placeholder implementation only.", Id);
            _onCommandReceivedCallback = onCommandReceived; 
            return Task.CompletedTask;
        }

        public Task SendCommandResponseAsync(string commandId, object responsePayload)
        {
             if (!_config.EnableBiDirectional)
            {
                _logger.LogWarning("Bi-directional communication is disabled for HTTP adaptor '{Id}'. Cannot send command response.", Id);
                return Task.CompletedTask;
            }
            _logger.LogWarning("HTTP adaptor '{Id}' typically does not support sending direct command responses unless the platform provides a specific API for it. Placeholder implementation only.", Id);
            return Task.CompletedTask; 
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing HttpIoTAdaptor '{Id}'. Disposing HttpClient.", Id);
            // HttpClient is managed by IHttpClientFactory, no explicit dispose here unless it was created manually.
            GC.SuppressFinalize(this);
        }
    }
}