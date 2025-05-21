using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Adapters.IoT.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Amqp; // AMQPNetLite
using Amqp.Framing;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationService.Adapters.IoT
{
    public class AmqpAdaptor : IIoTPlatformAdaptor
    {
        private readonly IoTPlatformConfig _config;
        private readonly ILogger<AmqpAdaptor> _logger;
        private Connection _connection;
        private Session _session;
        private SenderLink _senderLink;
        private ReceiverLink _receiverLink;
        private Func<IoTCommand, CancellationToken, Task> _onCommandReceived;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public string PlatformType => "AMQP";
        public string PlatformId => _config.Id;

        public AmqpAdaptor(
            IoTPlatformConfig config,
            ILogger<AmqpAdaptor> logger,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure AMQPNetLite Trace for debugging if needed
            // Amqp.Trace.TraceLevel = Amqp.TraceLevel.Frame | Amqp.TraceLevel.Verbose;
            // Amqp.Trace.TraceListener = (lvl, frm, str) => _logger.LogTrace($"AMQP Trace Level: {lvl}, Format: {frm}, Content: {str}");

            _retryPolicy = retryPolicyFactory.CreateAsyncRetryPolicy(3, TimeSpan.FromSeconds(2));
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.CreateAsyncCircuitBreakerPolicy(5, TimeSpan.FromSeconds(30));
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_connection != null && !_connection.IsClosed)
            {
                _logger.LogInformation("AMQP client for platform {PlatformId} is already connected.", _config.Id);
                return;
            }

            _logger.LogInformation("Connecting to AMQP broker for platform {PlatformId} at {Endpoint}", _config.Id, _config.Endpoint);
            
            try
            {
                await _retryPolicy.ExecuteAsync(async (ct) =>
                {
                    // Endpoint usually in format "amqp://host:port" or "amqps://host:port"
                    // Authentication details (SASL PLAIN, etc.) would be set on Address or ConnectionFactory
                    var address = new Address(_config.Endpoint);
                    if (_config.Authentication?.Properties.ContainsKey("Username") == true && 
                        _config.Authentication?.Properties.ContainsKey("Password") == true)
                    {
                        // Example for SASL PLAIN
                        // address = new Address(_config.Endpoint, 5672, 
                        //    _config.Authentication.Properties["Username"], 
                        //    _config.Authentication.Properties["Password"], 
                        //    "/", "AMQPS".Equals(_config.Endpoint.Split("://")[0], StringComparison.OrdinalIgnoreCase) ? "AMQPS" : "AMQP");
                        // AMQPNetLite handles this based on user/pass in URI: amqp://user:pass@host:port
                    }

                    var connectionFactory = new ConnectionFactory();
                    // Add TLS/SSL settings if required by _config (e.g., for "amqps")
                    // connectionFactory.SSL.ClientCertificates.Add(...);
                    // connectionFactory.SSL.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;


                    _connection = await connectionFactory.CreateAsync(address);
                    _connection.Closed += OnConnectionClosed;
                    _session = new Session(_connection);

                    _logger.LogInformation("AMQP connection established for platform {PlatformId}.", _config.Id);
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to AMQP broker for platform {PlatformId} after retries.", _config.Id);
                CleanupConnection();
                throw;
            }
        }
        
        private void OnConnectionClosed(IAmqpObject sender, Error error)
        {
            _logger.LogWarning("AMQP connection for platform {PlatformId} closed. Error: {ErrorDescription}", _config.Id, error?.Description ?? "N/A");
            CleanupConnection();
            // Implement reconnection strategy or notify service
        }

        private void CleanupConnection()
        {
            _senderLink?.Close();
            _senderLink = null;
            _receiverLink?.Close();
            _receiverLink = null;
            _session?.Close();
            _session = null;
            _connection?.Close(); // This might have already been called if OnConnectionClosed triggered
            _connection = null;
        }


        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disconnecting from AMQP broker for platform {PlatformId}.", _config.Id);
            if (_connection != null && !_connection.IsClosed)
            {
                await Task.Run(() => CleanupConnection(), cancellationToken); // AMQPNetLite close is sync
            }
            _logger.LogInformation("AMQP client for platform {PlatformId} disconnected.", _config.Id);
        }

        public async Task PublishTelemetryAsync(IoTDataMessage message, CancellationToken cancellationToken)
        {
            if (_session == null || _session.IsClosed)
            {
                _logger.LogWarning("AMQP session for platform {PlatformId} is not available. Attempting to publish telemetry for DeviceId: {DeviceId}", _config.Id, message.DeviceId);
                await ConnectAsync(cancellationToken);
                if (_session == null || _session.IsClosed)
                {
                     _logger.LogError("Failed to publish telemetry. AMQP session for platform {PlatformId} still not available.", _config.Id);
                    throw new InvalidOperationException("AMQP session not available.");
                }
            }

            var targetAddress = _config.Properties.GetValueOrDefault("TelemetryTargetAddress")?.Replace("{DeviceId}", message.DeviceId)
                                ?? $"telemetry/{message.DeviceId}"; // Example target address (queue or topic)

            if (_senderLink == null || _senderLink.IsClosed || _senderLink.Target.Address != targetAddress)
            {
                 _senderLink?.Close(); // Close if target changed
                _senderLink = new SenderLink(_session, $"sender-{_config.Id}-{Guid.NewGuid()}", targetAddress);
            }
            
            var payload = JsonSerializer.SerializeToUtf8Bytes(message.Payload);
            var amqpMessage = new Message(payload)
            {
                Properties = new Properties { MessageId = Guid.NewGuid().ToString(), CreationTime = DateTime.UtcNow },
                MessageAnnotations = new MessageAnnotations()
            };
            amqpMessage.MessageAnnotations[new Symbol("x-opt-partition-key")] = message.DeviceId; // Example for partitioning

            try
            {
                await _circuitBreakerPolicy.ExecuteAsync(async (ct) =>
                {
                    // AMQPNetLite Send is synchronous but can be wrapped
                    await Task.Run(() => _senderLink.Send(amqpMessage, TimeSpan.FromSeconds(10)), ct); // Timeout for send
                    _logger.LogInformation("Published telemetry to AMQP target {TargetAddress} for platform {PlatformId}.", targetAddress, _config.Id);
                }, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to publish telemetry to AMQP platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (AmqpException amqpEx)
            {
                _logger.LogError(amqpEx, "AMQP error publishing telemetry to {TargetAddress} for platform {PlatformId}. Error: {AmqpError}", targetAddress, _config.Id, amqpEx.Error?.Description);
                if (amqpEx.Error?.Condition == ErrorCode.LinkDetachForced || amqpEx.Error?.Condition == ErrorCode.SessionUnattached)
                {
                    CleanupConnection(); // Force reconnect on next attempt
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing telemetry to AMQP platform {PlatformId}.", _config.Id);
                throw;
            }
        }

        public async Task SubscribeToCommandsAsync(Func<IoTCommand, CancellationToken, Task> onCommandReceived, CancellationToken cancellationToken)
        {
            _onCommandReceived = onCommandReceived ?? throw new ArgumentNullException(nameof(onCommandReceived));

            if (_session == null || _session.IsClosed)
            {
                _logger.LogWarning("AMQP session for platform {PlatformId} is not available. Cannot subscribe to commands.", _config.Id);
                await ConnectAsync(cancellationToken);
                if (_session == null || _session.IsClosed)
                {
                    _logger.LogError("Failed to subscribe. AMQP session for platform {PlatformId} still not available.", _config.Id);
                    throw new InvalidOperationException("AMQP session not available for subscribing.");
                }
            }
            
            var sourceAddress = _config.Properties.GetValueOrDefault("CommandSourceAddress") // e.g., "commands/mydevice" or "commands/+" for wildcard (platform dependent)
                                ?? "commands"; 

            if (_receiverLink != null && !_receiverLink.IsClosed)
            {
                await Task.Run(()=> _receiverLink.Close(), cancellationToken); // Close existing if any, AMQPNetLite is sync
            }

            _receiverLink = new ReceiverLink(_session, $"receiver-{_config.Id}-{Guid.NewGuid()}", sourceAddress, OnAmqpMessageReceived);
            _logger.LogInformation("Subscribed to AMQP source {SourceAddress} for commands on platform {PlatformId}.", sourceAddress, _config.Id);
        }

        private void OnAmqpMessageReceived(IReceiverLink receiver, Message message)
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Timeout for processing
            Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Received AMQP message from {Source} on platform {PlatformId}. MessageId: {MessageId}", 
                        receiver.Settings.Source.Address, _config.Id, message.Properties?.MessageId);

                    if (message.Body is byte[] bodyBytes)
                    {
                        var payloadString = Encoding.UTF8.GetString(bodyBytes);
                        var command = JsonSerializer.Deserialize<IoTCommand>(payloadString);

                        if (command != null)
                        {
                            // Potentially enrich command with info from message properties/annotations
                            // command.TargetDeviceId = message.ApplicationProperties?["deviceId"]?.ToString();
                            // command.CorrelationId = message.Properties?.CorrelationId?.ToString();

                            if (_onCommandReceived != null)
                            {
                                await _onCommandReceived(command, cts.Token);
                                receiver.Accept(message); // Acknowledge message
                                return;
                            }
                            else
                            {
                                _logger.LogWarning("No handler registered for AMQP commands on platform {PlatformId}.", _config.Id);
                            }
                        }
                        else
                        {
                             _logger.LogWarning("Could not deserialize incoming AMQP message payload to IoTCommand from {Source} for platform {PlatformId}.", receiver.Settings.Source.Address, _config.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Received AMQP message with non-byte[] body from {Source} for platform {PlatformId}. Body type: {BodyType}", 
                            receiver.Settings.Source.Address, _config.Id, message.Body?.GetType().Name);
                    }
                    
                    receiver.Reject(message, new Error(ErrorCode.InvalidField){ Description = "Unable to process message" });
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize AMQP message payload from {Source} for platform {PlatformId}.", receiver.Settings.Source.Address, _config.Id);
                    receiver.Reject(message, new Error(ErrorCode.DecodeError){ Description = "JSON deserialization failed." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing received AMQP message from {Source} for platform {PlatformId}.", receiver.Settings.Source.Address, _config.Id);
                    // Consider releasing or rejecting based on error type
                    receiver.Modify(message, true, false); // Release message, allow redelivery
                }
            }, cts.Token);
        }

        public async Task SendIoTCommandResponseAsync(IoTCommandResponse response, CancellationToken cancellationToken)
        {
            // AMQP command/response often uses reply-to semantics or dedicated response queues.
            // This is a simplified version assuming a known response target address.
             if (_session == null || _session.IsClosed)
            {
                _logger.LogWarning("AMQP session for platform {PlatformId} is not available. Cannot send command response.", _config.Id);
                throw new InvalidOperationException("AMQP session not available.");
            }

            var responseTargetAddress = _config.Properties.GetValueOrDefault("CommandResponseTargetAddress")
                                    ?.Replace("{DeviceId}", response.DeviceId)
                                    .Replace("{CommandId}", response.CommandId)
                                ?? $"commandresponses/{response.DeviceId}/{response.CommandId}";
            
            // Use a temporary sender link or manage a pool if sending responses frequently
            var tempSenderLink = new SenderLink(_session, $"response-sender-{Guid.NewGuid()}", responseTargetAddress);

            try
            {
                var payload = JsonSerializer.SerializeToUtf8Bytes(response.Payload);
                var amqpMessage = new Message(payload)
                {
                    Properties = new Properties { 
                        MessageId = Guid.NewGuid().ToString(), 
                        CorrelationId = response.CommandId, // Link response to original command
                        CreationTime = DateTime.UtcNow 
                    }
                };

                await _circuitBreakerPolicy.ExecuteAsync(async (ct) =>
                {
                    await Task.Run(() => tempSenderLink.Send(amqpMessage, TimeSpan.FromSeconds(10)), ct);
                    _logger.LogInformation("Sent AMQP command response to {TargetAddress} for platform {PlatformId}.", responseTargetAddress, _config.Id);
                }, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to send command response via AMQP for platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (AmqpException amqpEx)
            {
                _logger.LogError(amqpEx, "AMQP error sending command response to {TargetAddress} for platform {PlatformId}. Error: {AmqpError}", 
                    responseTargetAddress, _config.Id, amqpEx.Error?.Description);
                 if (amqpEx.Error?.Condition == ErrorCode.LinkDetachForced || amqpEx.Error?.Condition == ErrorCode.SessionUnattached)
                {
                    CleanupConnection(); // Force reconnect on next attempt
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command response via AMQP for platform {PlatformId}.", _config.Id);
                throw;
            }
            finally
            {
                await Task.Run(() => tempSenderLink.Close(), cancellationToken); // Close temporary link
            }
        }
    }
}