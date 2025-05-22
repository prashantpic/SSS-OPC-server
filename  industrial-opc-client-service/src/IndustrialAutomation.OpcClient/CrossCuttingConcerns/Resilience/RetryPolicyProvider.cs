using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using Grpc.Core; // For gRPC status codes

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

        public AsyncRetryPolicy GetDefaultRetryPolicy(string policyName = "Default")
        {
            // Example: Read from configuration
            int retryCount = _configuration.GetValue<int>($"ResiliencePolicies:{policyName}:RetryCount", 3);
            int delaySeconds = _configuration.GetValue<int>($"ResiliencePolicies:{policyName}:DelaySeconds", 2);

            return Policy
                .Handle<Exception>(ex => !(ex is OperationCanceledException)) // Don't retry on cancellation
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(delaySeconds, retryAttempt)),
                    (exception, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(exception, "Policy {PolicyName}: Retry attempt {Attempt} after {TimeSpan} due to {ExceptionType}.",
                            policyName, attempt, timeSpan, exception.GetType().Name);
                    });
        }

        public AsyncRetryPolicy GetOpcConnectionPolicy()
        {
            // Specific for OPC connection attempts
            // Can handle Opc.Ua.ServiceResultException or other OPC specific exceptions
            int retryCount = _configuration.GetValue<int>("ResiliencePolicies:OpcConnection:RetryCount", 5);
            int initialDelayMs = _configuration.GetValue<int>("ResiliencePolicies:OpcConnection:InitialDelayMs", 1000);
            int maxDelayMs = _configuration.GetValue<int>("ResiliencePolicies:OpcConnection:MaxDelayMs", 30000);


            return Policy
                .Handle<Opc.Ua.ServiceResultException>(ex => IsTransientOpcUaError(ex))
                .Or<TimeoutException>()
                .Or<System.Net.Sockets.SocketException>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromMilliseconds(Math.Min(initialDelayMs * Math.Pow(2, retryAttempt -1), maxDelayMs)), // Exponential backoff with jitter could be added
                    (exception, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(exception, "OPC Connection Policy: Retry attempt {Attempt} for {PolicyKey} after {TimeSpan} due to {ExceptionType} - {ExceptionMessage}.",
                                           attempt, context.PolicyKey, timeSpan, exception.GetType().Name, exception.Message);
                    })
                .WithPolicyKey("OpcConnectionRetry");
        }

        public AsyncRetryPolicy GetServerCommsPolicy() // For gRPC or Message Queue
        {
            int retryCount = _configuration.GetValue<int>("ResiliencePolicies:ServerCommunication:RetryCount", 3);
            double jitterFactor = _configuration.GetValue<double>("ResiliencePolicies:ServerCommunication:JitterFactor", 0.2);


            return Policy
                .Handle<HttpRequestException>() // For gRPC HTTP errors
                .Or<RpcException>(ex => IsTransientGrpcError(ex.StatusCode)) // For gRPC specific transient errors
                .Or<RabbitMQ.Client.Exceptions.BrokerUnreachableException>() // For RabbitMQ
                .Or<Confluent.Kafka.ProduceException>(ex => ex.Error.IsBrokerError || ex.Error.IsLocalError) // For Kafka
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt =>
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                        var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, (int)(delay.TotalMilliseconds * jitterFactor)));
                        return delay + jitter;
                    },
                    (exception, timeSpan, attempt, context) =>
                    {
                        _logger.LogWarning(exception, "Server Communication Policy: Retry attempt {Attempt} for {PolicyKey} after {TimeSpan} due to {ExceptionType} - {ExceptionMessage}.",
                                           attempt, context.PolicyKey, timeSpan, exception.GetType().Name, exception.Message);
                    })
                .WithPolicyKey("ServerCommunicationRetry");
        }
        
        private bool IsTransientOpcUaError(Opc.Ua.ServiceResultException ex)
        {
            // Add specific OPC UA status codes that are considered transient
            // Example: Bad_Timeout, Bad_CommunicationError, Bad_ServerNotConnected
            var transientCodes = new[] {
                Opc.Ua.StatusCodes.BadTimeout,
                Opc.Ua.StatusCodes.BadCommunicationError,
                Opc.Ua.StatusCodes.BadServerNotConnected,
                Opc.Ua.StatusCodes.BadServerHalted,
                Opc.Ua.StatusCodes.BadTcpServerTooBusy,
                Opc.Ua.StatusCodes.BadSecureChannelClosed,
                Opc.Ua.StatusCodes.BadSessionClosed,
                Opc.Ua.StatusCodes.BadSessionNotActivated
            };
            return transientCodes.Contains(ex.StatusCode);
        }

        private bool IsTransientGrpcError(StatusCode statusCode)
        {
            // See https://grpc.github.io/grpc/core/md_doc_statuscodes.html
            return statusCode == StatusCode.Unavailable ||
                   statusCode == StatusCode.DeadlineExceeded ||
                   statusCode == StatusCode.ResourceExhausted || // Can sometimes be transient
                   statusCode == StatusCode.Internal; // Often indicates server-side transient issues
        }
    }
}