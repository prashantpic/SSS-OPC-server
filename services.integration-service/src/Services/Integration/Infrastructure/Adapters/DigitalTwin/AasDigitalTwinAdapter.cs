using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Services.Integration.Application.Interfaces;
using Services.Integration.Domain.Aggregates;
using System.Text.Json;
using System.Net;
using System.Text;

namespace Services.Integration.Infrastructure.Adapters.DigitalTwin;

/// <summary>
/// Implements the <see cref="IDigitalTwinAdapter"/> for platforms compliant with the
/// Asset Administration Shell (AAS) standard, using a RESTful API approach.
/// </summary>
public class AasDigitalTwinAdapter : IDigitalTwinAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AasDigitalTwinAdapter> _logger;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Initializes a new instance of the <see cref="AasDigitalTwinAdapter"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HttpClient instances.</param>
    /// <param name="logger">The logger for structured logging.</param>
    public AasDigitalTwinAdapter(IHttpClientFactory httpClientFactory, ILogger<AasDigitalTwinAdapter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        // Define a retry policy for transient HTTP errors (e.g., 5xx, 408)
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => r.StatusCode >= HttpStatusCode.InternalServerError || r.StatusCode == HttpStatusCode.RequestTimeout)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Delaying for {Delay}ms, then making retry {Retry} of calling AAS endpoint. Outcome: {Outcome}", timespan.TotalMilliseconds, retryCount, outcome.Result?.StatusCode);
                });

        // Define a circuit breaker policy to stop calls after 5 consecutive failures
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => _logger.LogWarning("AAS Circuit breaker tripped for {BreakDelay} seconds due to: {Exception}", breakDelay, ex.Message),
                onReset: () => _logger.LogInformation("AAS Circuit breaker has been reset."),
                onHalfOpen: () => _logger.LogInformation("AAS Circuit breaker is now half-open.")
            );
    }
    
    /// <inheritdoc />
    public async Task SendDataToTwinAsync(IntegrationEndpoint endpoint, object data)
    {
        var targetUrl = endpoint.Address.Uri;
        _logger.LogInformation("Sending data to AAS Digital Twin at {Url}", targetUrl);

        var httpClient = _httpClientFactory.CreateClient("AasAdapter");
        var jsonPayload = JsonSerializer.Serialize(data);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Combine policies: retry first, then the circuit breaker.
        var policyWrap = _circuitBreakerPolicy.WrapAsync(_retryPolicy);

        try
        {
            var response = await policyWrap.ExecuteAsync(async () =>
            {
                // Using PATCH for partial updates, common in AAS interactions.
                var responseMessage = await httpClient.PatchAsync(targetUrl, content);
                responseMessage.EnsureSuccessStatusCode(); // Throws on non-2xx responses
                return responseMessage;
            });
            
            _logger.LogInformation("Successfully sent data to AAS twin. Status Code: {StatusCode}", response.StatusCode);
        }
        catch (BrokenCircuitException bce)
        {
            _logger.LogError(bce, "Could not send data to AAS twin at {Url}: The circuit is open.", targetUrl);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending data to AAS twin at {Url}", targetUrl);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<object> ReceiveDataFromTwinAsync(IntegrationEndpoint endpoint)
    {
        var targetUrl = endpoint.Address.Uri;
        _logger.LogInformation("Receiving data from AAS Digital Twin at {Url}", targetUrl);

        var httpClient = _httpClientFactory.CreateClient("AasAdapter");
        var policyWrap = _circuitBreakerPolicy.WrapAsync(_retryPolicy);
        
        try
        {
            var response = await policyWrap.ExecuteAsync(async () =>
            {
                var responseMessage = await httpClient.GetAsync(targetUrl);
                responseMessage.EnsureSuccessStatusCode();
                return responseMessage;
            });

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object>(jsonResponse);

            _logger.LogInformation("Successfully received data from AAS twin.");
            return data!;
        }
        catch (BrokenCircuitException bce)
        {
            _logger.LogError(bce, "Could not receive data from AAS twin at {Url}: The circuit is open.", targetUrl);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error receiving data from AAS twin at {Url}", targetUrl);
            throw;
        }
    }
}