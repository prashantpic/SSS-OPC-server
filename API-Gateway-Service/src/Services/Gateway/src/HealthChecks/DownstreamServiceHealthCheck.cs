using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.HealthChecks
{
    /// <summary>
    /// Implements a health check for a specific downstream service.
    /// Monitors the health of a downstream microservice (e.g., Management Service, AI Service)
    /// by making a lightweight request to its health endpoint.
    /// </summary>
    public class DownstreamServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _serviceName;
        private readonly string _healthCheckUrl;
        private readonly ILogger<DownstreamServiceHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownstreamServiceHealthCheck"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The factory to create HTTP clients.</param>
        /// <param name="serviceName">The name of the downstream service being checked.</param>
        /// <param name="healthCheckUrl">The URL of the downstream service's health endpoint.</param>
        /// <param name="logger">The logger.</param>
        public DownstreamServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            string serviceName,
            string healthCheckUrl,
            ILogger<DownstreamServiceHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _healthCheckUrl = healthCheckUrl ?? throw new ArgumentNullException(nameof(healthCheckUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!Uri.TryCreate(_healthCheckUrl, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Health check URL must be an absolute URI.", nameof(healthCheckUrl));
            }
        }

        /// <summary>
        /// Checks the health of the downstream service.
        /// </summary>
        /// <param name="context">A context object associated with the current health check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation with a <see cref="HealthCheckResult"/>.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Performing health check for downstream service: {ServiceName} at {HealthCheckUrl}", _serviceName, _healthCheckUrl);

            try
            {
                var httpClient = _httpClientFactory.CreateClient("DownstreamHealthCheckClient"); // Use a named client if specific policies apply
                
                // Set a reasonable timeout for health checks
                var healthCheckTimeout = TimeSpan.FromSeconds(5); 
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(healthCheckTimeout);

                var request = new HttpRequestMessage(HttpMethod.Get, _healthCheckUrl);
                var response = await httpClient.SendAsync(request, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Downstream service {ServiceName} is healthy. Status: {StatusCode}", _serviceName, response.StatusCode);
                    return HealthCheckResult.Healthy($"Downstream service '{_serviceName}' is healthy.");
                }
                else
                {
                    _logger.LogWarning("Downstream service {ServiceName} is unhealthy. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                        _serviceName, response.StatusCode, response.ReasonPhrase);
                    return HealthCheckResult.Unhealthy(
                        $"Downstream service '{_serviceName}' is unhealthy. Status: {response.StatusCode}. Reason: {response.ReasonPhrase}.",
                        data: new Dictionary<string, object> { { "status_code", (int)response.StatusCode }, { "reason_phrase", response.ReasonPhrase } });
                }
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                 _logger.LogWarning(ex, "Health check for downstream service {ServiceName} was canceled by the caller.", _serviceName);
                return HealthCheckResult.Unhealthy($"Health check for '{_serviceName}' was canceled.", ex);
            }
            catch (OperationCanceledException ex) // Timeout
            {
                _logger.LogWarning(ex, "Health check for downstream service {ServiceName} timed out.", _serviceName);
                return HealthCheckResult.Unhealthy($"Health check for '{_serviceName}' timed out.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking health of downstream service {ServiceName}.", _serviceName);
                return HealthCheckResult.Unhealthy($"An error occurred while checking health of '{_serviceName}'.", ex);
            }
        }
    }
}