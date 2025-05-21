namespace IntegrationService.Resiliency
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.CircuitBreaker;
    using IntegrationService.Configuration; // Assuming ResiliencySettings is in this namespace

    public class CircuitBreakerPolicyFactory
    {
        private readonly ILogger<CircuitBreakerPolicyFactory> _logger;
        private readonly CircuitBreakerPolicyConfig _defaultCircuitBreakerConfig;

        public CircuitBreakerPolicyFactory(IOptions<ResiliencySettings> resiliencySettings, ILogger<CircuitBreakerPolicyFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultCircuitBreakerConfig = resiliencySettings?.Value?.DefaultCircuitBreakerPolicy ?? new CircuitBreakerPolicyConfig();
        }
        
        public AsyncCircuitBreakerPolicy CreateAsyncCircuitBreakerPolicy(string policyName = "DefaultBreaker")
        {
            return CreateAsyncCircuitBreakerPolicy(
                _defaultCircuitBreakerConfig.ExceptionsAllowedBeforeBreaking,
                TimeSpan.FromSeconds(_defaultCircuitBreakerConfig.DurationOfBreakSeconds),
                policyName);
        }

        public AsyncCircuitBreakerPolicy CreateAsyncCircuitBreakerPolicy(
            int exceptionsAllowedBeforeBreaking,
            TimeSpan durationOfBreak,
            string policyName = "CustomBreaker")
        {
            if (exceptionsAllowedBeforeBreaking <= 0) exceptionsAllowedBeforeBreaking = _defaultCircuitBreakerConfig.ExceptionsAllowedBeforeBreaking;
            if (durationOfBreak <= TimeSpan.Zero) durationOfBreak = TimeSpan.FromSeconds(_defaultCircuitBreakerConfig.DurationOfBreakSeconds);

            return Policy
                .Handle<Exception>(ex => IsCritical(ex)) // Define what exceptions should trip the breaker
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking,
                    durationOfBreak,
                    (exception, breakDelay, context) =>
                    {
                        _logger.LogWarning(exception,
                            "[{PolicyName}] Circuit breaker opened for {BreakDelayMs}ms due to exception. Context: {PolicyKey}",
                            policyName, breakDelay.TotalMilliseconds, context.PolicyKey);
                    },
                    context =>
                    {
                        _logger.LogInformation("[{PolicyName}] Circuit breaker reset. Context: {PolicyKey}", policyName, context.PolicyKey);
                    },
                    () => // onHalfOpen
                    {
                        _logger.LogInformation("[{PolicyName}] Circuit breaker is now half-open. Next call is a test.", policyName);
                    }
                );
        }
        
        public AsyncCircuitBreakerPolicy<TResult> CreateAsyncCircuitBreakerPolicy<TResult>(
            Func<TResult, bool>? resultPredicate = null, 
            string policyName = "DefaultResultBreaker")
        {
            var policyBuilder = Policy.Handle<Exception>(ex => IsCritical(ex));
            
            if (resultPredicate != null)
            {
                policyBuilder = policyBuilder.OrResult(resultPredicate);
            }

            return policyBuilder
                .CircuitBreakerAsync(
                    _defaultCircuitBreakerConfig.ExceptionsAllowedBeforeBreaking,
                    TimeSpan.FromSeconds(_defaultCircuitBreakerConfig.DurationOfBreakSeconds),
                    (outcome, breakDelay, context) =>
                    {
                        _logger.LogWarning(outcome.Exception,
                            "[{PolicyName}] Circuit breaker opened for {BreakDelayMs}ms due to result: {Result} or exception. Context: {PolicyKey}",
                            policyName, breakDelay.TotalMilliseconds, outcome.Result, context.PolicyKey);
                    },
                    context =>
                    {
                        _logger.LogInformation("[{PolicyName}] Circuit breaker reset. Context: {PolicyKey}", policyName, context.PolicyKey);
                    },
                    () => 
                    {
                        _logger.LogInformation("[{PolicyName}] Circuit breaker is now half-open. Next call is a test.", policyName);
                    }
                );
        }

        private static bool IsCritical(Exception ex)
        {
            // Add logic to determine if the exception is critical enough to open the circuit.
            // Often similar to transient, but sometimes long-lasting failures.
            if (ex is BrokenCircuitException) return false; // Don't count breaks themselves
            if (ex is HttpRequestException) return true;
            // if (ex is System.Net.Sockets.SocketException) return true;
            // Consider specific API error responses as well.
            return true; // Default to true for unhandled exceptions for this example
        }
    }
}