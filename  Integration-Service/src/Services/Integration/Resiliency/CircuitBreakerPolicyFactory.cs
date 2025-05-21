using Polly;
using Polly.CircuitBreaker;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// Assuming ResiliencyConfigs is defined, as in RetryPolicyFactory.cs

namespace IntegrationService.Resiliency
{
    /// <summary>
    /// Factory for creating circuit breaker policies (e.g., using Polly).
    /// Centralizes the creation of configurable circuit breaker policies.
    /// </summary>
    public class CircuitBreakerPolicyFactory
    {
        private readonly ILogger<CircuitBreakerPolicyFactory> _logger;
        private readonly ResiliencyConfigs _defaultConfigs;

        public CircuitBreakerPolicyFactory(ILogger<CircuitBreakerPolicyFactory> logger, IOptions<ResiliencyConfigs> defaultConfigsAccessor)
        {
            _logger = logger;
            _defaultConfigs = defaultConfigsAccessor.Value;
             _logger.LogInformation("CircuitBreakerPolicyFactory initialized with default threshold {Threshold} and break duration {Duration}s", 
                                    _defaultConfigs.DefaultCircuitBreakerThresholdExceptionsAllowed, _defaultConfigs.DefaultCircuitBreakDurationSeconds);
        }

        public AsyncCircuitBreakerPolicy GetDefaultCircuitBreakerPolicyAsync()
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    _defaultConfigs.DefaultCircuitBreakerThresholdExceptionsAllowed,
                    TimeSpan.FromSeconds(_defaultConfigs.DefaultCircuitBreakDurationSeconds),
                    onBreak: (exception, breakDelay, context) =>
                    {
                        _logger.LogWarning(exception, "Circuit breaker for {PolicyKey} opening for {BreakDelay} seconds due to exception {ExceptionType}. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, breakDelay, exception.GetType().Name, context.PolicyWrapKey);
                    },
                    onReset: (context) =>
                    {
                        _logger.LogInformation("Circuit breaker for {PolicyKey} resetting. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    },
                    onHalfOpen: (context) =>
                    {
                        _logger.LogInformation("Circuit breaker for {PolicyKey} is half-open, allowing a single test call. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    });
        }
        
        public CircuitBreakerPolicy GetDefaultCircuitBreakerPolicySync()
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    _defaultConfigs.DefaultCircuitBreakerThresholdExceptionsAllowed,
                    TimeSpan.FromSeconds(_defaultConfigs.DefaultCircuitBreakDurationSeconds),
                    onBreak: (exception, breakDelay, context) =>
                    {
                        _logger.LogWarning(exception, "Synchronous circuit breaker for {PolicyKey} opening for {BreakDelay} seconds due to exception {ExceptionType}. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, breakDelay, exception.GetType().Name, context.PolicyWrapKey);
                    },
                    onReset: (context) =>
                    {
                        _logger.LogInformation("Synchronous circuit breaker for {PolicyKey} resetting. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    },
                    onHalfOpen: (context) =>
                    {
                        _logger.LogInformation("Synchronous circuit breaker for {PolicyKey} is half-open, allowing a single test call. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    });
        }

        public AsyncCircuitBreakerPolicy GetCustomCircuitBreakerPolicyAsync(int exceptionsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
             _logger.LogInformation("Creating custom async circuit breaker policy with threshold {Threshold} and break duration {Duration}", 
                                    exceptionsAllowedBeforeBreaking, durationOfBreak);
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking,
                    durationOfBreak,
                    onBreak: (exception, breakDelay, context) =>
                    {
                         _logger.LogWarning(exception, "Custom async circuit breaker for {PolicyKey} opening for {BreakDelay} seconds due to exception {ExceptionType}. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, breakDelay, exception.GetType().Name, context.PolicyWrapKey);
                    },
                    onReset: (context) =>
                    {
                         _logger.LogInformation("Custom async circuit breaker for {PolicyKey} resetting. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    },
                    onHalfOpen: (context) =>
                    {
                         _logger.LogInformation("Custom async circuit breaker for {PolicyKey} is half-open, allowing a single test call. Context: {ContextPolicyWrapKey}", 
                                           context.PolicyKey, context.PolicyWrapKey);
                    });
        }
    }
}