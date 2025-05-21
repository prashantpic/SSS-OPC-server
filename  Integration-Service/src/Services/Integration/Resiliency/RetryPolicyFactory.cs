namespace IntegrationService.Resiliency
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Retry;
    using IntegrationService.Configuration; // Assuming ResiliencySettings is in this namespace

    public class RetryPolicyFactory
    {
        private readonly ILogger<RetryPolicyFactory> _logger;
        private readonly RetryPolicyConfig _defaultRetryConfig;

        public RetryPolicyFactory(IOptions<ResiliencySettings> resiliencySettings, ILogger<RetryPolicyFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultRetryConfig = resiliencySettings?.Value?.DefaultRetryPolicy ?? new RetryPolicyConfig();
        }

        public AsyncRetryPolicy CreateAsyncRetryPolicy(string policyName = "Default")
        {
            return CreateAsyncRetryPolicy(_defaultRetryConfig.RetryAttempts, TimeSpan.FromSeconds(_defaultRetryConfig.RetryDelaySeconds), policyName);
        }

        public AsyncRetryPolicy CreateAsyncRetryPolicy(int retryAttempts, TimeSpan baseDelay, string policyName = "Custom")
        {
            if (retryAttempts <= 0) retryAttempts = _defaultRetryConfig.RetryAttempts;
            if (baseDelay <= TimeSpan.Zero) baseDelay = TimeSpan.FromSeconds(_defaultRetryConfig.RetryDelaySeconds);

            return Policy
                .Handle<Exception>(ex => IsTransient(ex)) // Define what exceptions to retry on
                .WaitAndRetryAsync(
                    retryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(baseDelay.TotalSeconds, retryAttempt)), // Exponential backoff
                    (exception, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(exception,
                            "[{PolicyName}] Delaying for {DelayMs}ms, then making retry {RetryAttempt} of {MaxRetries}. Context: {PolicyKey}",
                            policyName, timeSpan.TotalMilliseconds, attempt, retryAttempts, context.PolicyKey);
                    }
                );
        }
        
        public AsyncRetryPolicy<TResult> CreateAsyncRetryPolicy<TResult>(Func<TResult, bool>? resultPredicate = null, string policyName = "DefaultResultPolicy")
        {
            var policyBuilder = Policy.Handle<Exception>(ex => IsTransient(ex));
            
            if (resultPredicate != null)
            {
                policyBuilder = policyBuilder.OrResult(resultPredicate);
            }

            return policyBuilder
                .WaitAndRetryAsync(
                    _defaultRetryConfig.RetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(_defaultRetryConfig.RetryDelaySeconds, retryAttempt)), 
                    (outcome, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(outcome.Exception,
                            "[{PolicyName}] Delaying for {DelayMs}ms, then making retry {RetryAttempt} of {MaxRetries} due to result: {Result} or exception. Context: {PolicyKey}",
                            policyName, timeSpan.TotalMilliseconds, attempt, _defaultRetryConfig.RetryAttempts, outcome.Result, context.PolicyKey);
                    }
                );
        }


        private static bool IsTransient(Exception ex)
        {
            // Add logic to determine if the exception is transient
            // For example, HttpRequestException, specific SQL exceptions, custom transient exceptions
            if (ex is HttpRequestException) return true;
            if (ex is TimeoutException) return true;
            // Add more specific transient exceptions here
            // if (ex is System.IO.IOException) return true;
            return false;
        }
    }
}