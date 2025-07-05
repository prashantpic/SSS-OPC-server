using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Net.Sockets;

namespace Opc.System.Services.Integration.Infrastructure.Resilience;

/// <summary>
/// Defines shared Polly resilience policies for external service communications.
/// </summary>
public static class IntegrationResiliencePolicies
{
    /// <summary>
    /// Gets a default retry policy for HTTP clients.
    /// It handles transient HTTP errors and socket exceptions, retrying 3 times with exponential backoff.
    /// </summary>
    /// <returns>An asynchronous policy for retrying HTTP requests.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5xx, and 408 status codes
            .Or<SocketException>() // Also handle network-level errors
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Optionally log the retry attempt
                    // context.GetLogger()?.LogWarning(...)
                });
    }

    /// <summary>
    /// Gets a default circuit breaker policy for HTTP clients.
    /// It breaks the circuit for 30 seconds after 5 consecutive failures.
    /// </summary>
    /// <returns>An asynchronous circuit breaker policy for HTTP requests.</returns>
    public static IAsyncPolicy<HttpResponseMessage> GetDefaultCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, state, timespan, context) =>
                {
                     // Optionally log the break event
                },
                onReset: (context) =>
                {
                    // Optionally log the reset event
                }
            );
    }
}