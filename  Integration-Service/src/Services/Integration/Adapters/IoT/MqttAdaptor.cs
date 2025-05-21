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
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationService.Adapters.IoT
{
    public class MqttAdaptor : IIoTPlatformAdaptor
    {
        private readonly IoTPlatformConfig _config;
        private readonly ILogger<MqttAdaptor> _logger;
        private readonly IMqttClient _mqttClient;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private Func<IoTCommand, CancellationToken, Task> _onCommandReceived;

        public string PlatformType => "MQTT";
        public string PlatformId => _config.Id;

        public MqttAdaptor(
            IoTPlatformConfig config,
            ILogger<MqttAdaptor> logger,
            IMqttClientFactory mqttClientFactory,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mqttClient = mqttClientFactory.CreateMqttClient();

            // Assuming ResiliencySettings are part of IoTPlatformConfig or globally available
            // For simplicity, using default values if not configured, real app would get from config.
            _retryPolicy = retryPolicyFactory.CreateAsyncRetryPolicy(3, TimeSpan.FromSeconds(2));
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.CreateAsyncCircuitBreakerPolicy(5, TimeSpan.FromSeconds(30));

            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_mqttClient.IsConnected)
            {
                _logger.LogInformation("MQTT client for platform {PlatformId} is already connected.", _config.Id);
                return;
            }

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.Endpoint.Split(':')[0], int.Parse(_config.Endpoint.Split(':')[1])) // Basic parsing, improve for robustness
                .WithClientId(_config.Authentication?.Properties.GetValueOrDefault("ClientId") ?? Guid.NewGuid().ToString())
                .WithCredentials(_config.Authentication?.Properties.GetValueOrDefault("Username"), _config.Authentication?.Properties.GetValueOrDefault("Password"))
                // .WithTls(new MqttClientOptionsBuilderTlsParameters { UseTls = true }) // Add TLS config based on _config
                .Build();
            
            _logger.LogInformation("Connecting to MQTT broker for platform {PlatformId} at {Endpoint}", _config.Id, _config.Endpoint);
            try
            {
                await _retryPolicy.ExecuteAsync(ct => _mqttClient.ConnectAsync(mqttClientOptions, ct), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker for platform {PlatformId} after retries.", _config.Id);
                throw;
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.LogInformation("MQTT client for platform {PlatformId} is already disconnected.", _config.Id);
                return;
            }
            _logger.LogInformation("Disconnecting from MQTT broker for platform {PlatformId}.", _config.Id);
            await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectReason.NormalDisconnection).Build(), cancellationToken);
        }

        public async Task PublishTelemetryAsync(IoTDataMessage message, CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MQTT client for platform {PlatformId} is not connected. Attempting to publish telemetry for DeviceId: {DeviceId}", _config.Id, message.DeviceId);
                await ConnectAsync(cancellationToken); // Attempt to reconnect
                if (!_mqttClient.IsConnected)
                {
                    _logger.LogError("Failed to publish telemetry. MQTT client for platform {PlatformId} still not connected.", _config.Id);
                    throw new MqttCommunicationException("Client not connected.");
                }
            }

            // Topic can be part of IoTPlatformConfig or derived from message
            var topic = _config.Properties.GetValueOrDefault("TelemetryTopicTemplate")?.Replace("{DeviceId}", message.DeviceId) 
                        ?? $"devices/{message.DeviceId}/telemetry";
            
            var payload = JsonSerializer.Serialize(message.Payload); // Assuming payload is serializable
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // Configurable
                .Build();

            try
            {
                await _circuitBreakerPolicy.ExecuteAsync(async ct =>
                {
                    var result = await _mqttClient.PublishAsync(applicationMessage, ct);
                    if (result.ReasonCode != MqttClientPublishReasonCode.Success)
                    {
                        _logger.LogError("Failed to publish MQTT message to {Topic} for platform {PlatformId}. Reason: {ReasonCode}", topic, _config.Id, result.ReasonCode);
                        // Potentially throw a specific exception based on ReasonCode
                        throw new MqttCommunicationException($"MQTT publish failed with code {result.ReasonCode}");
                    }
                    _logger.LogInformation("Published telemetry to MQTT topic {Topic} for platform {PlatformId}.", topic, _config.Id);
                }, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to publish telemetry to MQTT platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing telemetry to MQTT platform {PlatformId}.", _config.Id);
                throw;
            }
        }

        public async Task SubscribeToCommandsAsync(Func<IoTCommand, CancellationToken, Task> onCommandReceived, CancellationToken cancellationToken)
        {
            _onCommandReceived = onCommandReceived ?? throw new ArgumentNullException(nameof(onCommandReceived));

            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MQTT client for platform {PlatformId} not connected. Cannot subscribe to commands.", _config.Id);
                await ConnectAsync(cancellationToken);
                if (!_mqttClient.IsConnected)
                {
                     _logger.LogError("Failed to subscribe to commands. MQTT client for platform {PlatformId} still not connected.", _config.Id);
                    throw new MqttCommunicationException("Client not connected for subscribing.");
                }
            }

            // Command topic should be configurable
            var commandTopic = _config.Properties.GetValueOrDefault("CommandTopicTemplate")?.Replace("+", _config.Properties.GetValueOrDefault("CommandDeviceIdWildcard", "+")) // e.g., devices/+/commands
                                ?? "devices/+/commands"; 
            
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(commandTopic).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                .Build();

            try
            {
                var result = await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
                foreach (var subResult in result.Items)
                {
                    if (subResult.ResultCode > MqttClientSubscribeResultCode.GrantedQoS2)
                    {
                        _logger.LogError("Failed to subscribe to MQTT topic {Topic} for platform {PlatformId}. Result code: {ResultCode}", 
                            subResult.TopicFilter.Topic, _config.Id, subResult.ResultCode);
                        // Handle partial subscription failure
                    }
                    else
                    {
                        _logger.LogInformation("Successfully subscribed to MQTT topic {Topic} for platform {PlatformId}.", subResult.TopicFilter.Topic, _config.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to commands on MQTT platform {PlatformId}.", _config.Id);
                throw;
            }
        }
        
        // Define IoTCommandResponse if not already defined
        // public record IoTCommandResponse(string CommandId, string DeviceId, int StatusCode, JsonElement Payload);

        public async Task SendIoTCommandResponseAsync(IoTCommandResponse response, CancellationToken cancellationToken)
        {
             if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MQTT client for platform {PlatformId} is not connected. Cannot send command response for CommandId: {CommandId}", _config.Id, response.CommandId);
                 // Optionally attempt to connect or throw
                throw new MqttCommunicationException("Client not connected.");
            }

            // Response topic might be derived from original command or configured
            var responseTopic = _config.Properties.GetValueOrDefault("CommandResponseTopicTemplate")
                                    ?.Replace("{DeviceId}", response.DeviceId)
                                    .Replace("{CommandId}", response.CommandId)
                                ?? $"devices/{response.DeviceId}/commands/{response.CommandId}/response";

            var payload = JsonSerializer.Serialize(response.Payload);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(responseTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // Configurable
                .Build();
            
            try
            {
                 await _circuitBreakerPolicy.ExecuteAsync(async ct =>
                {
                    var result = await _mqttClient.PublishAsync(applicationMessage, ct);
                    if (result.ReasonCode != MqttClientPublishReasonCode.Success)
                    {
                        _logger.LogError("Failed to publish MQTT command response to {Topic} for platform {PlatformId}. Reason: {ReasonCode}", responseTopic, _config.Id, result.ReasonCode);
                        throw new MqttCommunicationException($"MQTT command response publish failed with code {result.ReasonCode}");
                    }
                    _logger.LogInformation("Published command response to MQTT topic {Topic} for platform {PlatformId}.", responseTopic, _config.Id);
                }, cancellationToken);
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to send command response to MQTT platform {PlatformId}.", _config.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command response to MQTT platform {PlatformId}.", _config.Id);
                throw;
            }
        }


        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation("MQTT client for platform {PlatformId} connected successfully.", _config.Id);
            // Potentially re-subscribe if subscriptions are lost on disconnect
            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("MQTT client for platform {PlatformId} disconnected. Reason: {Reason}. Will attempt to reconnect: {WillReconnect}", 
                _config.Id, e.Reason, e.ClientWasConnected);
            
            // MQTTnet client has auto-reconnect capabilities that can be configured in MqttClientOptions.
            // If not using auto-reconnect, or for more control, implement reconnection logic here or in a monitoring service.
            return Task.CompletedTask;
        }

        private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogInformation("Received MQTT message on topic {Topic} for platform {PlatformId}.", e.ApplicationMessage.Topic, _config.Id);
            if (_onCommandReceived != null)
            {
                try
                {
                    var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                    var command = JsonSerializer.Deserialize<IoTCommand>(payloadString); // Assuming IoTCommand is standard
                    
                    // Extract DeviceId from topic if necessary
                    // Example: devices/DEVICE_ID/commands -> extract DEVICE_ID
                    // This logic would depend on the topic structure.
                    if (string.IsNullOrEmpty(command.TargetDeviceId))
                    {
                        // Simple extraction, make more robust
                        var topicParts = e.ApplicationMessage.Topic.Split('/');
                        if (topicParts.Length >= 2 && topicParts[0] == "devices") // Example convention
                        {
                            command.TargetDeviceId = topicParts[1];
                        }
                    }

                    if (command != null)
                    {
                        await _onCommandReceived(command, CancellationToken.None); // Consider passing a CancellationToken
                    }
                    else
                    {
                        _logger.LogWarning("Could not deserialize incoming MQTT message payload to IoTCommand on topic {Topic} for platform {PlatformId}.", e.ApplicationMessage.Topic, _config.Id);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize MQTT message payload on topic {Topic} for platform {PlatformId}.", e.ApplicationMessage.Topic, _config.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing received MQTT message on topic {Topic} for platform {PlatformId}.", e.ApplicationMessage.Topic, _config.Id);
                }
            }
            else
            {
                _logger.LogWarning("No handler registered for incoming MQTT commands on platform {PlatformId}.", _config.Id);
            }
        }
    }
}