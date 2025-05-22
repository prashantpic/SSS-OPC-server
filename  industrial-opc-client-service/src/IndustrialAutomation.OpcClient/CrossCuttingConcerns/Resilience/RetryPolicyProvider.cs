using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Net.Sockets;
using Grpc.Core; // For gRPC specific status codes

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.Resilience
{
    public class RetryPolicyProvider
    {
        private readonly ILogger<RetryPolicyProvider> _logger;
        private readonly IConfiguration _configuration;

        public RetryPolicyProvider(ILogger<RetryPolicyProvider> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public AsyncRetryPolicy GetDefaultRetryPolicyAsync(string policyName = "Default")
        {
            var maxRetries = _configuration.GetValue<int>($"ResiliencePolicies:{policyName}:MaxRetries", 3);
            var initialDelaySeconds = _configuration.GetValue<double>($"ResiliencePolicies:{policyName}:InitialDelaySeconds", 1.0);
            var maxDelaySeconds = _configuration.GetValue<double>($"ResiliencePolicies:{policyName}:MaxDelaySeconds", 30.0);

            return Policy
                .Handle<Exception>(ex => IsTransient(ex)) // Generic transient exceptions
                .WaitAndRetryAsync(
                    maxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Min(initialDelaySeconds * Math.Pow(2, retryAttempt -1), maxDelaySeconds)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "[{PolicyName}] Retry {RetryCount} encountered after {TimeSpan}. Operation Key: {OperationKey}",
                            policyName, retryCount, timeSpan, context.OperationKey);
                    }
                );
        }

        public AsyncRetryPolicy GetOpcConnectionPolicy() => GetDefaultRetryPolicyAsync("OpcConnection");
        public AsyncRetryPolicy GetServerCommsPolicy() => GetDefaultRetryPolicyAsync("ServerComms");
        public AsyncRetryPolicy GetSubscriptionReestablishmentPolicy() => GetDefaultRetryPolicyAsync("SubscriptionReestablishment");


        private static bool IsTransient(Exception ex)
        {
            if (ex is SocketException || ex is IOException || ex is TimeoutException)
                return true;

            if (ex is RpcException rpcException)
            {
                // Common gRPC transient error codes
                return rpcException.StatusCode == StatusCode.Unavailable ||
                       rpcException.StatusCode == StatusCode.DeadlineExceeded ||
                       rpcException.StatusCode == StatusCode.ResourceExhausted; // Can sometimes be transient
            }

            // Add more specific exceptions for RabbitMQ, Kafka, OPC UA if needed
            // For example, OPC UA specific status codes indicating server busy or temporary issues.
            // if (ex is Opc.Ua.ServiceResultException sre)
            // {
            //    return sre.StatusCode == Opc.Ua.StatusCodes.BadServerNotConnected ||
            //           sre.StatusCode == Opc.Ua.StatusCodes.BadTcpServerTooBusy;
            // }

            return false; // Default to non-transient
        }
    }
}