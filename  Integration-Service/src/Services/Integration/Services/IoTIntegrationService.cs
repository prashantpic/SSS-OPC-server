using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Adapters.IoT.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.Resiliency;
using IntegrationService.Validation;
using Polly;
using IntegrationService.Adapters.IoT; // Added for specific adaptor instantiation
using System.Net.Http; // Added for IHttpClientFactory

namespace IntegrationService.Services
{
    /// <summary>
    /// Orchestrates IoT platform integrations, using adaptors, data mappers, and credential managers.
    /// </summary>
    public class IoTIntegrationService
    {
        private readonly ILogger<IoTIntegrationService> _logger;
        private readonly IntegrationSettings _settings;
        private readonly IDataMapper _dataMapper;
        private readonly ICredentialManager _credentialManager;
        private readonly RetryPolicyFactory _retryPolicyFactory;
        private readonly CircuitBreakerPolicyFactory _circuitBreakerPolicyFactory;
        private readonly IncomingIoTDataValidator _dataValidator;

        private readonly Dictionary<string, IIoTPlatformAdaptor> _adaptors = new Dictionary<string, IIoTPlatformAdaptor>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory; // To create specific loggers for adaptors

        public IoTIntegrationService(
            ILogger<IoTIntegrationService> logger,
            IOptions<IntegrationSettings> settings,
            IDataMapper dataMapper,
            ICredentialManager credentialManager,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory,
            IncomingIoTDataValidator dataValidator,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _settings = settings.Value;
            _dataMapper = dataMapper;
            _credentialManager = credentialManager;
            _retryPolicyFactory = retryPolicyFactory;
            _circuitBreakerPolicyFactory = circuitBreakerPolicyFactory;
            _dataValidator = dataValidator;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;

            _logger.LogInformation("IoTIntegrationService initialized.");
            InitializeAdaptors();
        }

        private void InitializeAdaptors()
        {
             _logger.LogInformation("Initializing IoT adaptors...");
            var iotSettings = _settings.IoTPlatformSettings;
            var featureFlags = _settings.FeatureFlags;

            foreach (var platformConfig in iotSettings.Platforms)
            {
                IIoTPlatformAdaptor? adaptor = null;
                bool enabled = false;

                switch (platformConfig.Type.ToUpperInvariant())
                {
                    case "MQTT":
                        if (featureFlags.EnableMqttIntegration)
                        {
                            adaptor = new MqttAdaptor(platformConfig,
                                _loggerFactory.CreateLogger<MqttAdaptor>(),
                                _credentialManager, _retryPolicyFactory, _circuitBreakerPolicyFactory);
                            enabled = true;
                        } else { _logger.LogInformation("MQTT integration is disabled by feature flag for platform '{PlatformId}'.", platformConfig.Id); }
                        break;
                    case "AMQP":
                         if (featureFlags.EnableAmqpIntegration)
                        {
                            adaptor = new AmqpAdaptor(platformConfig,
                                _loggerFactory.CreateLogger<AmqpAdaptor>(),
                                _credentialManager, _retryPolicyFactory, _circuitBreakerPolicyFactory);
                             enabled = true;
                        } else { _logger.LogInformation("AMQP integration is disabled by feature flag for platform '{PlatformId}'.", platformConfig.Id); }
                        break;
                    case "HTTP":
                         if (featureFlags.EnableHttpIoTIntegration)
                        {
                            adaptor = new HttpIoTAdaptor(platformConfig,
                                _loggerFactory.CreateLogger<HttpIoTAdaptor>(),
                                _credentialManager, _httpClientFactory, _retryPolicyFactory, _circuitBreakerPolicyFactory);
                             enabled = true;
                        } else { _logger.LogInformation("HTTP IoT integration is disabled by feature flag for platform '{PlatformId}'.", platformConfig.Id); }
                        break;
                    default:
                        _logger.LogWarning("Unsupported IoT platform type configured: {PlatformType} for platform '{PlatformId}'. Skipping.", platformConfig.Type, platformConfig.Id);
                        break;
                }

                if (adaptor != null && enabled)
                {
                    if (_adaptors.ContainsKey(adaptor.Id))
                    {
                         _logger.LogWarning("Duplicate IoT platform configuration ID found: '{PlatformId}'. Only the first instance will be used.", adaptor.Id);
                         (adaptor as IDisposable)?.Dispose();
                    }
                    else
                    {
                        _adaptors.Add(adaptor.Id, adaptor);
                         _logger.LogInformation("Added IoT adaptor '{Id}' of type {Type}.", adaptor.Id, platformConfig.Type);

                        if (platformConfig.EnableBiDirectional && featureFlags.EnableBiDirectionalIoT)
                        {
                             _logger.LogInformation("Setting up command subscription for bi-directional IoT adaptor '{Id}'.", adaptor.Id);
                             adaptor.SubscribeToCommandsAsync(cmd => HandleIncomingIoTCommand(cmd, adaptor.Id))
                                 .ContinueWith(task =>
                                 {
                                     if (task.IsFaulted)
                                     {
                                         _logger.LogError(task.Exception, "Failed to subscribe to commands for IoT adaptor '{Id}'.", adaptor.Id);
                                     }
                                 });
                        } else if (platformConfig.EnableBiDirectional && !featureFlags.EnableBiDirectionalIoT) {
                             _logger.LogInformation("Bi-directional IoT integration is disabled by feature flag, skipping command subscription for adaptor '{Id}'.", adaptor.Id);
                        }
                    }
                }
            }
             _logger.LogInformation("IoT adaptor initialization finished. {Count} adaptors initialized.", _adaptors.Count);

            var mappingRuleIds = iotSettings.Platforms.Select(p => p.MappingRuleId).Where(id => !string.IsNullOrEmpty(id)).Distinct();
            if (mappingRuleIds.Any())
            {
                _dataMapper.LoadAllConfiguredRulesAsync(mappingRuleIds).GetAwaiter().GetResult();
            }
        }

        public async Task ProcessIncomingOpcDataAsync(object opcData)
        {
             _logger.LogInformation("Processing incoming OPC data...");

            foreach (var adaptorEntry in _adaptors)
            {
                var adaptorId = adaptorEntry.Key;
                var adaptor = adaptorEntry.Value;
                var platformConfig = _settings.IoTPlatformSettings.Platforms.FirstOrDefault(p => p.Id == adaptorId);

                if (platformConfig == null)
                {
                    _logger.LogError("Configuration not found for initialized adaptor '{AdaptorId}'. Skipping.", adaptorId);
                    continue;
                }

                if (!adaptor.IsConnected)
                {
                     _logger.LogWarning("IoT adaptor '{AdaptorId}' is not connected. Attempting to connect before sending data.", adaptorId);
                    try
                    {
                         await adaptor.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                         _logger.LogError(ex, "Failed to connect IoT adaptor '{AdaptorId}'. Cannot send data.", adaptorId);
                         continue;
                    }
                }

                try
                {
                     IoTDataMessage iotMessage;
                    if (!string.IsNullOrEmpty(platformConfig.MappingRuleId) && _settings.FeatureFlags.EnableIoTRuleBasedMapping)
                    {
                        if (!_dataMapper.AreRulesLoaded(platformConfig.MappingRuleId))
                        {
                            _logger.LogWarning("Mapping rules '{MappingRuleId}' for adaptor '{AdaptorId}' are not loaded. Attempting to load.", platformConfig.MappingRuleId, adaptorId);
                            await _dataMapper.LoadMappingRulesAsync(platformConfig.MappingRuleId);
                             if (!_dataMapper.AreRulesLoaded(platformConfig.MappingRuleId))
                             {
                                 _logger.LogError("Failed to load mapping rules '{MappingRuleId}' for adaptor '{AdaptorId}'. Cannot map and send data.", platformConfig.MappingRuleId, adaptorId);
                                 continue;
                             }
                        }
                         _logger.LogDebug("Mapping OPC data using rules '{MappingRuleId}' for adaptor '{AdaptorId}'.", platformConfig.MappingRuleId, adaptorId);
                        iotMessage = _dataMapper.Map<object, IoTDataMessage>(opcData, platformConfig.MappingRuleId);
                    }
                    else
                    {
                         _logger.LogDebug("Rule-based mapping disabled or no rule ID for adaptor '{AdaptorId}'. Using direct mapping (placeholder).", adaptorId);
                        iotMessage = new IoTDataMessage
                        {
                             DeviceId = opcData.GetType().GetProperty("TagId")?.GetValue(opcData)?.ToString() ?? "PlaceholderDeviceId",
                             Timestamp = (DateTimeOffset)(opcData.GetType().GetProperty("Timestamp")?.GetValue(opcData) ?? DateTimeOffset.UtcNow),
                             Payload = opcData.GetType().GetProperty("Value")?.GetValue(opcData) ?? opcData,
                             Metadata = new Dictionary<string, string>() { { "SourceAdaptor", adaptorId } }
                        };
                         _logger.LogWarning("Using placeholder direct mapping for adaptor '{AdaptorId}'. Implement logic to map OPC data to IoTDataMessage when rules are not used.", adaptorId);
                    }

                    if (!_dataValidator.Validate(iotMessage))
                    {
                         _logger.LogWarning("Mapped IoT data for adaptor '{AdaptorId}' failed validation. Skipping send for DeviceId: {DeviceId}", adaptorId, iotMessage.DeviceId);
                        continue;
                    }

                     await adaptor.SendTelemetryAsync(iotMessage);
                     _logger.LogInformation("Successfully processed and sent data for device {DeviceId} via IoT adaptor '{AdaptorId}'.", iotMessage.DeviceId, adaptorId);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing or sending OPC data via IoT adaptor '{AdaptorId}'.", adaptorId);
                }
            }
        }

        private void HandleIncomingIoTCommand(IoTCommand command, string sourceAdaptorId)
        {
             _logger.LogInformation("Received incoming IoT command '{CommandName}' for device '{TargetDeviceId}' from adaptor '{SourceAdaptorId}' (CorrelationId: {CorrelationId})",
                command.CommandName, command.TargetDeviceId, sourceAdaptorId, command.CorrelationId);

            _logger.LogWarning("Placeholder: Received incoming IoT command from '{SourceAdaptorId}'. Implement logic to process command and interact with target systems.", sourceAdaptorId);

             if (!string.IsNullOrEmpty(command.CorrelationId))
             {
                 if (_adaptors.TryGetValue(sourceAdaptorId, out var receivingAdaptor))
                 {
                     var responsePayload = new { status = "CommandReceived_Placeholder", commandId = command.CorrelationId, details = "Processing logic not yet implemented." };
                     receivingAdaptor.SendCommandResponseAsync(command.CorrelationId, responsePayload)
                         .ContinueWith(task => {
                             if (task.IsFaulted) _logger.LogError(task.Exception, "Failed to send command response for command {CommandId} via adaptor {SourceAdaptorId}.", command.CorrelationId, sourceAdaptorId);
                             else _logger.LogInformation("Sent placeholder response for command {CommandId} via adaptor {SourceAdaptorId}.", command.CorrelationId, sourceAdaptorId);
                         });
                 } else {
                     _logger.LogError("Source adaptor ID '{SourceAdaptorId}' not found for sending command response.", sourceAdaptorId);
                 }
             }
        }

        public async Task ConnectAllAdaptorsAsync()
        {
             _logger.LogInformation("Attempting to connect all initialized IoT adaptors...");
            var connectTasks = _adaptors.Values.Select(adaptor => adaptor.ConnectAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                     _logger.LogError(task.Exception, "Failed to connect IoT adaptor '{AdaptorId}'.", adaptor.Id);
                } else {
                     _logger.LogInformation("IoT adaptor '{AdaptorId}' connection task completed. IsConnected: {IsConnected}", adaptor.Id, adaptor.IsConnected);
                }
            })).ToList();

            await Task.WhenAll(connectTasks);
             _logger.LogInformation("Finished attempting to connect all IoT adaptors.");
        }

         public async Task DisconnectAllAdaptorsAsync()
        {
             _logger.LogInformation("Attempting to disconnect all initialized IoT adaptors...");
            var disconnectTasks = _adaptors.Values.Select(adaptor => adaptor.DisconnectAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                     _logger.LogError(task.Exception, "Failed to disconnect IoT adaptor '{AdaptorId}'.", adaptor.Id);
                } else {
                     _logger.LogInformation("IoT adaptor '{AdaptorId}' disconnection task completed.", adaptor.Id);
                }
            })).ToList();

            await Task.WhenAll(disconnectTasks);
             _logger.LogInformation("Finished attempting to disconnect all IoT adaptors.");
        }
    }
}