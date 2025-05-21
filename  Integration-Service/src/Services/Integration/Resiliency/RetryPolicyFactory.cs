using Polly;
using Polly.Retry;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// Assuming ResiliencyConfigs is defined, e.g., in Program.cs or a separate file.
// For this file, let's assume it's accessible.
// If not, we'd need:
// namespace IntegrationService.Configuration.Models { public class ResiliencyConfigs { ... } }

namespace IntegrationService.Resiliency
{
    /// <summary>
    /// Factory for creating retry policies (e.g., using Polly).
    /// Centralizes the creation of configurable retry policies.
    /// </summary>
    public class RetryPolicyFactory
    {
        private readonly ILogger<RetryPolicyFactory> _logger;
        private readonly ResiliencyConfigs _defaultConfigs;

        public RetryPolicyFactory(ILogger<RetryPolicyFactory> logger, IOptions<ResiliencyConfigs> defaultConfigsAccessor)
        {
            _logger = logger;
            _defaultConfigs = defaultConfigsAccessor.Value;
             _logger.LogInformation("RetryPolicyFactory initialized with default attempts {Attempts} and delay {Delay}s", 
                                    _defaultConfigs.DefaultRetryAttempts, _defaultConfigs.DefaultRetryDelaySeconds);
        }

        public AsyncRetryPolicy GetDefaultRetryPolicyAsync()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _defaultConfigs.DefaultRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(_defaultConfigs.DefaultRetryDelaySeconds, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Retry {RetryCount} for {PolicyKey} operation failed. Waiting {TimeSpan} before next try. Context: {ContextPolicyWrapKey}",
                            retryCount, context.PolicyKey, timeSpan, context.PolicyWrapKey);
                    });
        }

        public RetryPolicy GetDefaultRetryPolicySync()
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    _defaultConfigs.DefaultRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(_defaultConfigs.DefaultRetryDelaySeconds, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Synchronous Retry {RetryCount} for {PolicyKey} operation failed. Waiting {TimeSpan} before next try. Context: {ContextPolicyWrapKey}",
                            retryCount, context.PolicyKey, timeSpan, context.PolicyWrapKey);
                    });
        }

        public AsyncRetryPolicy GetCustomRetryPolicyAsync(int retryAttempts, int baseDelaySeconds)
        {
             _logger.LogInformation("Creating custom async retry policy with attempts {Attempts} and base delay {BaseDelay}s", retryAttempts, baseDelaySeconds);
             return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(baseDelaySeconds, retryAttempt)),
                     (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "Custom Async Retry {RetryCount} for {PolicyKey} operation failed. Waiting {TimeSpan} before next try. Context: {ContextPolicyWrapKey}",
                            retryCount, context.PolicyKey, timeSpan, context.PolicyWrapKey);
                    });
        }
    }

    // This class should be defined elsewhere, e.g. Program.cs or a Models folder
    // Adding it here for compilation if not already present in the assumed context.
    // If ResiliencyConfigs is part of IntegrationSettings, then IOptions<IntegrationSettings> would be used.
    // Based on Program.cs generation, it's a separate class.
    public class ResiliencyConfigs
    {
        public int DefaultRetryAttempts { get; set; } = 3;
        public int DefaultRetryDelaySeconds { get; set; } = 2;
        public int DefaultCircuitBreakerThresholdExceptionsAllowed { get; set; } = 5;
        public int DefaultCircuitBreakDurationSeconds { get; set; } = 30;
    }
}