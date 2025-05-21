using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.HealthChecks
{
    /// <summary>
    /// Implements a health check for a specific downstream service.
    /// Implements ASP.NET Core's IHealthCheck interface to monitor the health 
    /// of a specific downstream microservice (e.g., Management Service, AI Service). 
    /// This involves making a lightweight request to the downstream service's health endpoint.
    /// </summary>
    public class DownstreamServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _serviceName;
        private readonly string? _healthCheckUrl; // Can be fully qualified or relative

        /// <summary>
        /// Initializes a new instance of the <see cref="DownstreamServiceHealthCheck"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="serviceName">The logical name of the service (used to look up its base URL).</param>
        /// <param name="healthEndpointPath">The relative path to the health endpoint (e.g., "/health").</param>
        public DownstreamServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            string serviceName,
            string healthEndpointPath = "/health") // Default health path
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            
            // Construct the full health check URL from ServiceEndpoints configuration
            var serviceBaseUrl = configuration[$"ServiceEndpoints:{serviceName}"];
            if (string.IsNullOrEmpty(serviceBaseUrl))
            {
                // Alternatively, serviceName could be the full URL itself if configured that way
                if (Uri.TryCreate(serviceName, UriKind.Absolute, out var parsedUri) && (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
                {
                     _healthCheckUrl = serviceName; // serviceName is already a full URL
                }
                else
                {
                    throw new ArgumentException($"Base URL for service '{serviceName}' not found in ServiceEndpoints configuration and serviceName is not a valid URL.", nameof(serviceName));
                }
            }
            else
            {
                 _healthCheckUrl = serviceBaseUrl.TrimEnd('/') + healthEndpointPath.TrimStart('~');
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DownstreamServiceHealthCheck"/> class with a direct URL.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="directHealthCheckUrl">The fully qualified URL for the health check.</param>
        public DownstreamServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            string directHealthCheckUrl)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            if (string.IsNullOrWhiteSpace(directHealthCheckUrl) || !Uri.IsWellFormedUriString(directHealthCheckUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Direct health check URL must be a valid absolute URL.", nameof(directHealthCheckUrl));
            }
            _serviceName = new Uri(directHealthCheckUrl).Host; // Use host as service name
            _healthCheckUrl = directHealthCheckUrl;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_healthCheckUrl))
            {
                 return HealthCheckResult.Unhealthy($"Health check URL for service '{_serviceName}' is not configured.");
            }

            var httpClient = _httpClientFactory.CreateClient("HealthCheckClient"); // Using a named client

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _healthCheckUrl);
                // Set a reasonable timeout for health checks
                // This can also be configured on the HttpClient itself via HttpClientFactory options
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                
                var response = await httpClient.SendAsync(request, linkedCts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy($"Downstream service '{_serviceName}' at '{_healthCheckUrl}' is healthy.");
                }
                else
                {
                    return HealthCheckResult.Unhealthy(
                        $"Downstream service '{_serviceName}' at '{_healthCheckUrl}' is unhealthy. Status: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == timeoutCts.Token)
            {
                 return HealthCheckResult.Unhealthy(
                    $"Downstream service '{_serviceName}' at '{_healthCheckUrl}' timed out.", ex);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    $"Downstream service '{_serviceName}' at '{_healthCheckUrl}' is unhealthy due to an exception.", ex);
            }
        }
    }
}