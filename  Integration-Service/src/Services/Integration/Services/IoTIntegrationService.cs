namespace IntegrationService.Services
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using IntegrationService.Adapters.IoT.Models;
    using IntegrationService.Configuration;
    using IntegrationService.Interfaces;
    using IntegrationService.Validation;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly.Wrap; // For combining policies if needed

    public class IoTIntegrationService
    {
        private readonly ILogger<IoTIntegrationService> _logger;
        private readonly IOptionsMonitor<IoTPlatformSettings> _iotPlatformSettings;
        private readonly IEnumerable<IIoTPlatformAdaptor> _platformAdaptors;
        private readonly IDataMapper _dataMapper;
        private readonly ICredentialManager _credentialManager; // Assuming this is used by adaptors
        private readonly IncomingIoTDataValidator _dataValidator;
        private readonly ConcurrentDictionary<string, IIoTPlatformAdaptor> _activeAdaptors = new();
        private readonly ConcurrentDictionary<string, IoTPlatformConfig> _platformConfigs = new();


        public IoTIntegrationService(
            ILogger<IoTIntegrationService> logger,
            IOptionsMonitor<IoTPlatformSettings> iotPlatformSettings,
            IEnumerable<IIoTPlatformAdaptor> platformAdaptors,
            IDataMapper dataMapper,
            ICredentialManager credentialManager,
            IncomingIoTDataValidator dataValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iotPlatformSettings = iotPlatformSettings ?? throw new ArgumentNullException(nameof(iotPlatformSettings));
            _platformAdaptors = platformAdaptors ?? throw new ArgumentNullException(nameof(platformAdaptors));
            _dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
            _credentialManager = credentialManager; // Nullable if adaptors handle their own credential fetching via ICredentialManager
            _dataValidator = dataValidator ?? throw new ArgumentNullException(nameof(dataValidator));

            _iotPlatformSettings.OnChange(UpdatePlatformConfigurations);
            UpdatePlatformConfigurations(_iotPlatformSettings.CurrentValue); // Initial load
        }

        private void UpdatePlatformConfigurations(IoTPlatformSettings settings)
        {
            _logger.LogInformation("Updating IoT platform configurations.");
            var newConfigs = settings.Platforms.Where(p => p.IsEnabled).ToDictionary(p => p.Id);

            // Remove adaptors for platforms that are no longer configured or disabled
            foreach (var existingId in _activeAdaptors.Keys.ToList())
            {
                if (!newConfigs.ContainsKey(existingId))
                {
                    if (_activeAdaptors.TryRemove(existingId, out var adaptorToDisconnect))
                    {
                        adaptorToDisconnect.DisconnectAsync(CancellationToken.None).ConfigureAwait(false); // Fire and forget disconnect
                        _logger.LogInformation("Disconnected and removed adaptor for IoT platform ID: {PlatformId}", existingId);
                    }
                    _platformConfigs.TryRemove(existingId, out _);
                }
            }

            // Add or update adaptors
            foreach (var config in newConfigs.Values)
            {
                _platformConfigs[config.Id] = config;
                if (!_activeAdaptors.ContainsKey(config.Id))
                {
                    InitializeAdaptorAsync(config, CancellationToken.None).ConfigureAwait(false);
                }
                // If an adaptor exists, its internal configuration might need updating if the adaptor supports it.
                // For now, we re-initialize if critical parts change, or assume adaptors handle dynamic config.
            }
        }

        private async Task InitializeAdaptorAsync(IoTPlatformConfig config, CancellationToken cancellationToken)
        {
            var adaptor = _platformAdaptors.FirstOrDefault(a => a.AdaptorType.Equals(config.Type.ToString(), StringComparison.OrdinalIgnoreCase));
            if (adaptor == null)
            {
                _logger.LogError("No IIoTPlatformAdaptor found for type: {PlatformType} and ID: {PlatformId}", config.Type, config.Id);
                return;
            }

            try
            {
                // Adaptors should be designed to be configurable with IoTPlatformConfig if not already done via DI and IOptions
                // For example, if an adaptor is a singleton, it needs a way to handle multiple platform configs or be instantiated per config.
                // Assuming here the IEnumerable<IIoTPlatformAdaptor> provides distinct, configurable adaptors, or a factory is used.
                // A more robust approach might involve an IAdaptorFactory.

                await adaptor.ConnectAsync(cancellationToken);
                if (!string.IsNullOrEmpty(config.CommandTopic)) // Check if command subscription is configured
                {
                     await adaptor.SubscribeToCommandsAsync(HandleIncomingIoTCommandAsync, cancellationToken);
                }
                _activeAdaptors[config.Id] = adaptor;
                _logger.LogInformation("Successfully initialized and connected IoT platform adaptor for ID: {PlatformId}, Type: {PlatformType}", config.Id, config.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize or connect IoT platform adaptor for ID: {PlatformId}, Type: {PlatformType}", config.Id, config.Type);
            }
        }

        public async Task ProcessAndSendDataToIoTPlatformAsync(object opcData, string targetPlatformId, CancellationToken cancellationToken)
        {
            if (!_platformConfigs.TryGetValue(targetPlatformId, out var platformConfig) || !_activeAdaptors.TryGetValue(targetPlatformId, out var adaptor))
            {
                _logger.LogWarning("IoT Platform with ID '{PlatformId}' not found or not active. Cannot send data.", targetPlatformId);
                return;
            }

            try
            {
                IoTDataMessage? externalData = null;
                if (!string.IsNullOrEmpty(platformConfig.MappingRuleId) || !string.IsNullOrEmpty(platformConfig.MappingRulePath))
                {
                    var mappingId = platformConfig.MappingRuleId ?? platformConfig.MappingRulePath;
                    externalData = await _dataMapper.MapToExternalFormatAsync<object, IoTDataMessage>(opcData, mappingId!, cancellationToken);
                }
                else
                {
                    // Attempt direct conversion or use a default mapping if opcData is already IoTDataMessage-like
                    if (opcData is IoTDataMessage directMessage) externalData = directMessage;
                    else if (opcData is JsonElement jsonElement) externalData = JsonSerializer.Deserialize<IoTDataMessage>(jsonElement); // Basic assumption
                    // Add more sophisticated default mapping if needed
                }

                if (externalData == null)
                {
                    _logger.LogWarning("Failed to map OPC data to IoTDataMessage for platform ID: {PlatformId}", targetPlatformId);
                    return;
                }

                var validationResult = await _dataValidator.ValidateAsync(externalData, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Outgoing IoTDataMessage failed validation for platform ID {PlatformId}: {ValidationErrors}",
                        targetPlatformId, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    return;
                }

                _logger.LogDebug("Publishing telemetry to IoT platform {PlatformId}: {Payload}", targetPlatformId, JsonSerializer.Serialize(externalData.Payload));
                await adaptor.PublishTelemetryAsync(externalData, cancellationToken);
                _logger.LogInformation("Successfully sent data to IoT Platform ID: {PlatformId}", targetPlatformId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing and sending data to IoT Platform ID: {PlatformId}", targetPlatformId);
            }
        }
        
        public async Task ProcessAndSendDataToAllConfiguredPlatformsAsync(object opcData, CancellationToken cancellationToken)
        {
            foreach (var platformId in _activeAdaptors.Keys)
            {
                await ProcessAndSendDataToIoTPlatformAsync(opcData, platformId, cancellationToken);
            }
        }


        private async Task HandleIncomingIoTCommandAsync(IoTCommand command)
        {
            _logger.LogInformation("Received command: {CommandName} for device: {DeviceId} with CorrelationId: {CorrelationId}",
                command.CommandName, command.TargetDeviceId, command.CorrelationId);

            // Validate command
            // var validationResult = await _commandValidator.ValidateAsync(command); // Assuming a command validator
            // if (!validationResult.IsValid)
            // {
            //     _logger.LogWarning("Incoming IoTCommand failed validation: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            //     // Optionally send a NACK or error response if supported by the platform
            //     return;
            // }

            // Map command to internal format if necessary
            // object internalCommand = command; // Default if no mapping
            // if (platformConfig.CommandMappingRuleId != null)
            // {
            //     internalCommand = await _dataMapper.MapToInternalFormatAsync<IoTCommand, object>(command, platformConfig.CommandMappingRuleId, CancellationToken.None);
            // }

            // TODO: Dispatch the command. This could involve:
            // 1. Publishing to an internal message queue for REPO-OPC-CORE or another service to pick up.
            // 2. Directly calling another service if tightly coupled (not recommended in microservices).
            // 3. Storing it for later processing.
            _logger.LogInformation("Command {CommandName} for device {DeviceId} received. Dispatch logic to be implemented.", command.CommandName, command.TargetDeviceId);

            // Example: If a response is expected and the adaptor supports it
            // var response = new IoTCommandResponse(command.CorrelationId, JsonDocument.Parse("{\"status\":\"received\"}").RootElement, true);
            // await adaptor.SendIoTCommandResponseAsync(response, CancellationToken.None);
        }

        public async Task StopAllAdaptorsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping all IoT adaptors...");
            foreach (var adaptorEntry in _activeAdaptors)
            {
                try
                {
                    await adaptorEntry.Value.DisconnectAsync(cancellationToken);
                    _logger.LogInformation("Disconnected adaptor for IoT platform ID: {PlatformId}", adaptorEntry.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting adaptor for IoT platform ID: {PlatformId}", adaptorEntry.Key);
                }
            }
            _activeAdaptors.Clear();
            _platformConfigs.Clear();
            _logger.LogInformation("All IoT adaptors stopped.");
        }
    }
}