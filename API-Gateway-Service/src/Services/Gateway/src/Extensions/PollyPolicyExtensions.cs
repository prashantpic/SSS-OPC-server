using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System;
using System.Net.Http;

namespace GatewayService.Extensions
{
    /// <summary>
    /// Extension methods for configuring Polly resilience policies.
    /// </summary>
    public static class PollyPolicyExtensions
    {
        public static IServiceCollection AddPollyPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var pollyPoliciesConfig = configuration.GetSection("PollyPolicies");

            var retryConfig = pollyPoliciesConfig.GetSection("DefaultRetry");
            int defaultRetryCount = retryConfig.GetValue<int>("Count", 3);
            TimeSpan defaultRetryDelay = TimeSpan.FromSeconds(retryConfig.GetValue<double>("DelaySeconds", 2));

            var circuitBreakerConfig = pollyPoliciesConfig.GetSection("DefaultCircuitBreaker");
            int exceptionsAllowedBeforeBreaking = circuitBreakerConfig.GetValue<int>("ExceptionsAllowedBeforeBreaking", 5);
            TimeSpan durationOfBreak = TimeSpan.FromSeconds(circuitBreakerConfig.GetValue<double>("DurationOfBreakSeconds", 30));

            var timeoutConfig = pollyPoliciesConfig.GetSection("DefaultTimeout");
            TimeSpan defaultTimeout = TimeSpan.FromSeconds(timeoutConfig.GetValue<double>("TimeoutSeconds", 10));


            // --- Define Policies ---

            // Retry Policy: Retries on transient HTTP errors
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5XX status codes, 408 status code
                .Or<TimeoutRejectedException>() // Handle Polly's own timeout
                .WaitAndRetryAsync(
                    retryCount: defaultRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(defaultRetryDelay.TotalSeconds, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogWarning(outcome.Exception, "Delaying for {Delay}ms, then making retry {RetryAttempt} for request {RequestUri}", timespan.TotalMilliseconds, retryAttempt, context.GetHttpRequestMessage()?.RequestUri);
                    });

            // Circuit Breaker Policy: Stops trying if too many failures occur
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                    durationOfBreak: durationOfBreak,
                    onBreak: (outcome, breakDelay, context) =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogWarning(outcome.Exception, "Circuit breaking for {BreakDelay}ms for request {RequestUri}", breakDelay.TotalMilliseconds, context.GetHttpRequestMessage()?.RequestUri);
                    },
                    onReset: context =>
                    {
                        var logger = context.GetLogger();
                        logger?.LogInformation("Circuit closed for request {RequestUri}", context.GetHttpRequestMessage()?.RequestUri);
                    },
                    onHalfOpen: () =>
                    {
                        // var logger = Polly.PolicyContext.GetLogger(); // Need a way to get logger here if context is not passed
                        // logger?.LogInformation("Circuit is half-open; next call is a trial.");
                        Console.WriteLine("Circuit is half-open; next call is a trial.");
                    });

            // Timeout Policy: Aborts requests that take too long
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(defaultTimeout);


            // --- Register Policies in Registry (for Ocelot.Provider.Polly or named access) ---
            IPolicyRegistry<string> policyRegistry = services.AddPolicyRegistry();
            policyRegistry.Add("DefaultRetryPolicy", retryPolicy);
            policyRegistry.Add("DefaultCircuitBreakerPolicy", circuitBreakerPolicy);
            policyRegistry.Add("DefaultTimeoutPolicy", timeoutPolicy); // Note: Timeout is often applied per-client

            // --- Configure HttpClientFactory with Policies for specific clients ---
            // Example for a generic "DownstreamClient" used by GraphQL resolvers or custom handlers
            // These clients would be injected via IHttpClientFactory.CreateClient("DownstreamClient")
            services.AddHttpClient("DownstreamClient")
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(circuitBreakerPolicy)
                .AddPolicyHandler(timeoutPolicy); // Order matters: timeout should be innermost usually

            // Example for specific service clients if needed
            // services.AddHttpClient<IManagementServiceApiClient, ManagementServiceApiClient>(client =>
            //     {
            //         client.BaseAddress = new Uri(configuration["ServiceEndpoints:ManagementServiceHttp"]);
            //     })
            //     .AddPolicyHandler(retryPolicy)
            //     .AddPolicyHandler(circuitBreakerPolicy);

            return services;
        }
    }
}