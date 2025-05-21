using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace GatewayService.Extensions
{
    public static class PollyPolicyExtensions
    {
        public static IServiceCollection AddResiliencePolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var pollyConfig = configuration.GetSection("PollyPolicies");
            int defaultRetryCount = pollyConfig.GetValue<int?>("DefaultRetryCount") ?? 3;
            double defaultCircuitBreakerThreshold = pollyConfig.GetValue<double?>("DefaultCircuitBreakerThreshold") ?? 0.5; // 50% failure threshold
            int defaultCircuitBreakerDurationSeconds = pollyConfig.GetValue<int?>("DefaultCircuitBreakerDurationSeconds") ?? 30;
            int defaultTimeoutSeconds = pollyConfig.GetValue<int?>("DefaultTimeoutSeconds") ?? 15;

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5XX, and 408
                .Or<TimeoutRejectedException>() // Handle Polly's own timeout
                .WaitAndRetryAsync(
                    retryCount: defaultRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        context.GetLogger()?.LogWarning(
                            "Delaying for {Timespan}ms, then making retry {RetryAttempt} for {RequestUri}.",
                            timespan.TotalMilliseconds, retryAttempt, context.GetRequestUri());
                    }
                );

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: (int)(defaultCircuitBreakerThreshold * 10), // Assuming 10 requests sample
                    durationOfBreak: TimeSpan.FromSeconds(defaultCircuitBreakerDurationSeconds),
                    onBreak: (outcome, breakDelay, context) =>
                    {
                        context.GetLogger()?.LogWarning(
                            "Circuit breaker tripped for {BreakDelay}ms due to {OutcomeResult} for {RequestUri}.",
                            breakDelay.TotalMilliseconds, outcome.Result, context.GetRequestUri());
                    },
                    onReset: context =>
                    {
                        context.GetLogger()?.LogInformation(
                            "Circuit breaker reset for {RequestUri}.", context.GetRequestUri());
                    },
                    onHalfOpen: () =>
                    {
                        // Log half-open state if needed
                    }
                );

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(defaultTimeoutSeconds));

            // Example of registering a named HttpClient for a specific downstream service
            services.AddHttpClient("ManagementServiceClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy); // Order matters: timeout is outer, then CB, then retry

            services.AddHttpClient("AIServiceClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy);

            services.AddHttpClient("DataServiceClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy);

            services.AddHttpClient("IntegrationServiceClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy);
            
            // Default HttpClient for other uses (e.g. GraphQL resolvers not using named clients)
            services.AddHttpClient("DefaultResilientClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy);


            return services;
        }
    }

    // Helper extension methods to get logger and request URI from PolicyContext
    internal static class PolicyContextExtensions
    {
        internal static ILogger? GetLogger(this Context context)
        {
            if (context.TryGetValue("ILogger", out var logger) && logger is ILogger actualLogger)
            {
                return actualLogger;
            }
            return null;
        }

        internal static string GetRequestUri(this Context context)
        {
             if (context.TryGetValue("RequestUri", out var requestUri) && requestUri is string uriString)
            {
                return uriString;
            }
            // Fallback or alternative way to get URI if needed. This requires HttpRequestMessage to be in context.
            // if (context.TryGetValue(HttpPolicyExtensions.PolicyKeyHttpRequestMessage, out var requestMessage) && requestMessage is HttpRequestMessage httpRequestMessage)
            // {
            //     return httpRequestMessage.RequestUri?.ToString() ?? "Unknown URI";
            // }
            return "Unknown URI";
        }

        // Helper to add logger to context if not using HttpRequestMessage based logging
        // public static Context WithLogger(this Context context, ILogger logger)
        // {
        //     context["ILogger"] = logger;
        //     return context;
        // }
    }
}