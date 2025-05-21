using IntegrationService.Adapters.IoT.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Adapters.IoT
{
    /// <summary>
    /// IoT Platform adaptor using MQTT protocol (MQTTnet).
    /// </summary>
    public class MqttAdaptor : IIoTPlatformAdaptor, IDisposable
    {
        public string Id { get; }
        private readonly ILogger<MqttAdaptor> _logger;
        private readonly ICredentialManager _credentialManager;
        private readonly IoTPlatformConfig _config;
        private readonly RetryPolicy _retryPolicy;
        private readonly CircuitBreakerPolicy _circuitBreakerPolicy;

        private IMqttClient? _mqttClient;
        private MqttClientOptions? _mqttClientOptions;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _isConnectingOrConnected = false;
        private bool _isDisposing = false;

        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        public MqttAdaptor(
            IoTPlatformConfig config,
            ILogger<MqttAdaptor> logger,
            ICredentialManager credentialManager,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            Id = config.Id;
            _config = config;
            _logger = logger;
            _credentialManager = credentialManager;
            _retryPolicy = retryPolicyFactory.GetDefaultRetryPolicy(); 
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.GetDefaultCircuitBreakerPolicy();

            _logger.LogInformation("MqttAdaptor '{Id}' initialized for endpoint {Endpoint}", Id, _config.Endpoint);
        }

        public async Task ConnectAsync()
        {
            if (_isConnectingOrConnected)
            {
                _logger.LogInformation("MqttAdaptor '{Id}' is already connecting or connected.", Id);
                return;
            }

            _isConnectingOrConnected = true;
            _logger.LogInformation("Connecting MqttAdaptor '{Id}' to {Endpoint}...", Id, _config.Endpoint);

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_config.Endpoint.Replace("mqtt://", "").Split(':')[0], 
                               _config.Endpoint.Contains(":") ? int.Parse(_config.Endpoint.Split(':')[^1]) : 1883) // Handle port
                .WithClientId($"IntegrationService_{Id}_{Guid.NewGuid():N}")
                .WithCleanSession()
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();

            await ApplyAuthenticationAsync(_mqttClientOptions);

            _mqttClient.ConnectedAsync += HandleConnectedAsync;
            _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;

            try
            {
                await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                         _logger.LogInformation("Attempting MQTT connection for '{Id}'...", Id);
                         await _mqttClient.ConnectAsync(_mqttClientOptions, _cancellationTokenSource.Token);
                         _logger.LogInformation("MQTT connection attempt finished for '{Id}'. IsConnected: {IsConnected}", Id, IsConnected);
                    }));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to connect MqttAdaptor '{Id}' to {Endpoint} after retries.", Id, _config.Endpoint);
                 _isConnectingOrConnected = false; 
                 throw; 
            }
        }

        private async Task ApplyAuthenticationAsync(MqttClientOptions options)
        {
             _logger.LogDebug("Applying authentication for MQTT adaptor '{Id}'. Type: {AuthType}", Id, _config.Authentication.Type);
            switch (_config.Authentication.Type)
            {
                case "UsernamePassword":
                    var password = await _credentialManager.GetCredentialAsync(_config.Authentication.CredentialKey);
                    options.Credentials = new MqttClientCredentials()
                    {
                        Username = _config.Authentication.Username,
                        Password = Encoding.UTF8.GetBytes(password) 
                    };
                    break;
                case "Certificate":
                    var cert = await _credentialManager.GetCertificateAsync(_config.Authentication.CertificateThumbprint);
                     ((MqttClientOptionsBuilder)_mqttClientOptions!.Build().UseTls()).WithClientCertificates(new[] { cert });
                     _logger.LogWarning("Certificate authentication placeholder: Ensure TLS configuration is complete.");
                    break;
                case "None":
                    break;
                default:
                    _logger.LogWarning("Unsupported MQTT authentication type: {AuthType}", _config.Authentication.Type);
                    break;
            }
        }

        private Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            _logger.LogInformation("MqttAdaptor '{Id}' connected to {Endpoint}.", Id, _config.Endpoint);
            _isConnectingOrConnected = true; 
            return Task.CompletedTask;
        }

        private Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            _logger.LogWarning("MqttAdaptor '{Id}' disconnected from {Endpoint}. Reason: {Reason}", Id, _config.Endpoint, eventArgs.Reason);
             _isConnectingOrConnected = false; 
            if (!_isDisposing)
            {
                 _logger.LogInformation("Attempting to re-establish MQTT connection for '{Id}'...", Id);
                _ = ConnectAsync(); 
            }
            return Task.CompletedTask;
        }

         private Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            _logger.LogInformation("MQTT message received by '{Id}'. Topic: {Topic}, Payload Size: {PayloadSize}", Id, eventArgs.ApplicationMessage.Topic, eventArgs.ApplicationMessage.PayloadSegment.Count);
            
            if (!_config.EnableBiDirectional)
            {
                 _logger.LogDebug("Bi-directional communication is disabled for '{Id}'. Ignoring incoming message.", Id);
                 return Task.CompletedTask;
            }

            try
            {
                string payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment);
                _logger.LogDebug("Received MQTT message payload: {Payload}", payload);

                if (_config.DataFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                {
                     var command = System.Text.Json.JsonSerializer.Deserialize<IoTCommand>(payload);
                     if (command != null)
                     {
                          _logger.LogInformation("Deserialized command '{CommandName}' from MQTT topic '{Topic}' for device '{DeviceId}'", command.CommandName, eventArgs.ApplicationMessage.Topic, command.TargetDeviceId);
                          _onCommandReceivedCallback?.Invoke(command); 
                     } else {
                         _logger.LogWarning("Failed to deserialize MQTT payload into IoTCommand for topic '{Topic}'.", eventArgs.ApplicationMessage.Topic);
                     }
                } else {
                     _logger.LogWarning("Unsupported data format '{DataFormat}' for incoming MQTT message processing on '{Id}'.", _config.DataFormat, Id);
                }
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error processing incoming MQTT message for '{Id}' on topic '{Topic}'.", Id, eventArgs.ApplicationMessage.Topic);
            }

            return Task.CompletedTask;
        }


        public async Task DisconnectAsync()
        {
             _isDisposing = true;
             _cancellationTokenSource.Cancel(); 
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                _logger.LogInformation("Disconnecting MqttAdaptor '{Id}' from {Endpoint}...", Id, _config.Endpoint);
                try
                {
                    await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions { Reason = MqttClientDisconnectReason.NormalDisconnection }, _cancellationTokenSource.Token);
                     _logger.LogInformation("MqttAdaptor '{Id}' disconnected successfully.", Id);
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Error disconnecting MqttAdaptor '{Id}'.", Id);
                }
            }
             _isConnectingOrConnected = false;
        }

        public async Task SendTelemetryAsync(IoTDataMessage message)
        {
            if (!IsConnected)
            {
                 _logger.LogWarning("MqttAdaptor '{Id}' is not connected. Cannot send telemetry for device {DeviceId}.", Id, message.DeviceId);
                throw new InvalidOperationException($"MQTT adaptor '{Id}' is not connected.");
            }

             _logger.LogDebug("Sending telemetry for device {DeviceId} via MqttAdaptor '{Id}'.", message.DeviceId, Id);

            byte[] payloadBytes;
            string topic = $"telemetry/{message.DeviceId}"; 

            try
            {
                if (_config.DataFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                {
                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(message); 
                    payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
                    _logger.LogTrace("Serialized JSON payload for device {DeviceId}: {JsonPayload}", message.DeviceId, jsonPayload);
                }
                else
                {
                     _logger.LogError("Unsupported data format '{DataFormat}' for sending MQTT telemetry on '{Id}'.", _config.DataFormat, Id);
                    throw new System.NotSupportedException($"Unsupported data format: {_config.DataFormat}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize telemetry message for device {DeviceId} using format {DataFormat}.", message.DeviceId, _config.DataFormat);
                throw; 
            }


            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payloadBytes)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) 
                .WithRetainFlag(false) 
                .Build();

            try
            {
                 await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                         _logger.LogDebug("Publishing MQTT message to topic '{Topic}' for device {DeviceId} via '{Id}'...", topic, message.DeviceId, Id);
                        await _mqttClient!.PublishAsync(mqttMessage, _cancellationTokenSource.Token);
                         _logger.LogTrace("Successfully published MQTT message for device {DeviceId} via '{Id}'.", message.DeviceId, Id);
                    }));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to send MQTT telemetry for device {DeviceId} via '{Id}' to topic '{Topic}' after retries.", message.DeviceId, Id, topic);
                 throw; 
            }
        }

        private System.Action<IoTCommand>? _onCommandReceivedCallback;

        public Task SubscribeToCommandsAsync(System.Action<IoTCommand> onCommandReceived)
        {
            if (!_config.EnableBiDirectional)
            {
                _logger.LogWarning("Bi-directional communication is disabled for MQTT adaptor '{Id}'. Cannot subscribe to commands.", Id);
                return Task.CompletedTask;
            }

            if (!IsConnected)
            {
                _logger.LogWarning("MqttAdaptor '{Id}' is not connected. Cannot subscribe to commands.", Id);
                _onCommandReceivedCallback = onCommandReceived;
                 _logger.LogInformation("Stored command subscription callback for '{Id}'. Will attempt subscription upon connection.", Id);
                return Task.CompletedTask; 
            }

            _logger.LogInformation("Subscribing MqttAdaptor '{Id}' to command topics...", Id);

            var commandTopicFilter = $"commands/{_config.Id}/#"; 
            _logger.LogWarning("Placeholder: Subscribing to hardcoded MQTT command topic '{Topic}' for '{Id}'. Needs configuration.", commandTopicFilter, Id);

            _onCommandReceivedCallback = onCommandReceived; 

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(commandTopicFilter)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            try
            {
                 return _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        _logger.LogDebug("Attempting MQTT subscribe to topic '{Topic}' via '{Id}'...", commandTopicFilter, Id);
                        var subscribeResult = await _mqttClient!.SubscribeAsync(topicFilter, _cancellationTokenSource.Token);
                        _logger.LogInformation("MQTT subscribe result for '{Id}' on topic '{Topic}': {ResultCode}", Id, commandTopicFilter, subscribeResult.Items.FirstOrDefault()?.ResultCode);
                    }));
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to subscribe MqttAdaptor '{Id}' to topic '{Topic}' after retries.", Id, commandTopicFilter);
                 throw; 
            }
        }

        public Task SendCommandResponseAsync(string commandId, object responsePayload)
        {
             if (!_config.EnableBiDirectional)
            {
                _logger.LogWarning("Bi-directional communication is disabled for MQTT adaptor '{Id}'. Cannot send command response.", Id);
                return Task.CompletedTask;
            }
             if (!IsConnected)
            {
                 _logger.LogWarning("MqttAdaptor '{Id}' is not connected. Cannot send command response.", Id);
                throw new InvalidOperationException($"MQTT adaptor '{Id}' is not connected.");
            }
             _logger.LogDebug("Sending command response for command ID {CommandId} via MqttAdaptor '{Id}'.", commandId, Id);

            string responseTopic = $"commands/response/{commandId}"; 
            byte[] payloadBytes;

            try
            {
                if (_config.DataFormat.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                {
                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(responsePayload);
                    payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
                }
                else
                {
                     _logger.LogError("Unsupported data format '{DataFormat}' for sending MQTT command response on '{Id}'.", _config.DataFormat, Id);
                    throw new System.NotSupportedException($"Unsupported data format: {_config.DataFormat}");
                }
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to serialize command response for command ID {CommandId} using format {DataFormat}.", commandId, _config.DataFormat);
                throw;
            }

             var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(responseTopic)
                .WithPayload(payloadBytes)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            try
            {
                 return _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        _logger.LogDebug("Publishing MQTT command response to topic '{Topic}' for command {CommandId} via '{Id}'...", responseTopic, commandId, Id);
                        await _mqttClient!.PublishAsync(mqttMessage, _cancellationTokenSource.Token);
                         _logger.LogTrace("Successfully published MQTT command response for command {CommandId} via '{Id}'.", commandId, Id);
                    }));
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to send MQTT command response for command {CommandId} via '{Id}' to topic '{Topic}' after retries.", commandId, Id, responseTopic);
                 throw; 
            }
        }

        public void Dispose()
        {
            _isDisposing = true;
            _cancellationTokenSource.Cancel();
            _mqttClient?.Dispose();
            _cancellationTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}