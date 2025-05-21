namespace IntegrationService.Services
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationService.Adapters.DigitalTwin.Models;
    using IntegrationService.Configuration;
    using IntegrationService.Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DigitalTwinIntegrationService
    {
        private readonly ILogger<DigitalTwinIntegrationService> _logger;
        private readonly IOptionsMonitor<DigitalTwinSettings> _digitalTwinSettings;
        private readonly IEnumerable<IDigitalTwinAdaptor> _platformAdaptors;
        private readonly IDataMapper _dataMapper;
        private readonly ICredentialManager _credentialManager; // Assuming this is used by adaptors
        private readonly ConcurrentDictionary<string, IDigitalTwinAdaptor> _activeAdaptors = new();
        private readonly ConcurrentDictionary<string, DigitalTwinConfig> _platformConfigs = new();


        // This service would need a way to get current data for twins.
        // For this example, SyncDataForTwinAsync will take the data as a parameter.
        // In a real system, it might fetch from REPO-DATA-SERVICE or an internal cache.
        // private readonly IDataSourceForDigitalTwins _dataSource;

        public DigitalTwinIntegrationService(
            ILogger<DigitalTwinIntegrationService> logger,
            IOptionsMonitor<DigitalTwinSettings> digitalTwinSettings,
            IEnumerable<IDigitalTwinAdaptor> platformAdaptors,
            IDataMapper dataMapper,
            ICredentialManager credentialManager
            /* IDataSourceForDigitalTwins dataSource */)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _digitalTwinSettings = digitalTwinSettings ?? throw new ArgumentNullException(nameof(digitalTwinSettings));
            _platformAdaptors = platformAdaptors ?? throw new ArgumentNullException(nameof(platformAdaptors));
            _dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
            _credentialManager = credentialManager;
            // _dataSource = dataSource;

            _digitalTwinSettings.OnChange(UpdatePlatformConfigurations);
            UpdatePlatformConfigurations(_digitalTwinSettings.CurrentValue);
        }

        private void UpdatePlatformConfigurations(DigitalTwinSettings settings)
        {
            _logger.LogInformation("Updating Digital Twin platform configurations.");
            var newConfigs = settings.Platforms.Where(p => p.IsEnabled).ToDictionary(p => p.Id);

            foreach (var existingId in _activeAdaptors.Keys.ToList())
            {
                if (!newConfigs.ContainsKey(existingId))
                {
                    // Disconnect logic for digital twin adaptors if applicable (HTTP might not have persistent connections)
                    if (_activeAdaptors.TryRemove(existingId, out _))
                    {
                         _logger.LogInformation("Removed adaptor for Digital Twin platform ID: {PlatformId}", existingId);
                    }
                    _platformConfigs.TryRemove(existingId, out _);
                }
            }

            foreach (var config in newConfigs.Values)
            {
                 _platformConfigs[config.Id] = config;
                if (!_activeAdaptors.ContainsKey(config.Id))
                {
                    InitializeAdaptorAsync(config, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }

        private async Task InitializeAdaptorAsync(DigitalTwinConfig config, CancellationToken cancellationToken)
        {
            // Assuming DigitalTwinConfig.Type specifies the adaptor type (e.g., "HttpV1", "AzureDigitalTwins")
             var adaptor = _platformAdaptors.FirstOrDefault(a => a.AdaptorType.Equals(config.Type, StringComparison.OrdinalIgnoreCase));
            if (adaptor == null)
            {
                _logger.LogError("No IDigitalTwinAdaptor found for type: {PlatformType} and ID: {PlatformId}", config.Type, config.Id);
                return;
            }
            try
            {
                // Adaptors might need specific configuration passed to them if not handled by DI IOptions directly.
                await adaptor.ConnectAsync(cancellationToken); // Connect might just validate endpoint/auth for HTTP.
                _activeAdaptors[config.Id] = adaptor;
                 _logger.LogInformation("Successfully initialized Digital Twin platform adaptor for ID: {PlatformId}, Type: {PlatformType}", config.Id, config.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Digital Twin platform adaptor for ID: {PlatformId}, Type: {PlatformType}", config.Id, config.Type);
            }
        }


        public async Task SyncDataForTwinAsync(string platformId, string twinInstanceId, object currentTwinData, CancellationToken cancellationToken)
        {
            if (!_platformConfigs.TryGetValue(platformId, out var platformConfig) || !_activeAdaptors.TryGetValue(platformId, out var adaptor))
            {
                _logger.LogWarning("Digital Twin Platform with ID '{PlatformId}' not found or not active. Cannot sync data for twin '{TwinInstanceId}'.", platformId, twinInstanceId);
                return;
            }

            _logger.LogInformation("Starting data synchronization for Twin ID: {TwinInstanceId} on Platform: {PlatformId}", twinInstanceId, platformId);

            try
            {
                // 1. Optionally, get Digital Twin Model Info for compatibility checks (REQ-8-011)
                DigitalTwinModelInfo? modelInfo = null;
                if (!string.IsNullOrEmpty(platformConfig.DigitalTwinModelId)) // Assuming model ID helps fetch broader model info
                {
                    // The GetDigitalTwinModelInfoAsync might take a generic model ID or a specific twin instance ID
                    modelInfo = await adaptor.GetDigitalTwinModelInfoAsync(twinInstanceId, cancellationToken);
                    if (modelInfo != null)
                    {
                        _logger.LogDebug("Digital Twin Model Info for {TwinInstanceId}: ID={ModelId}, Version={Version}",
                                         twinInstanceId, modelInfo.ModelId, modelInfo.Version);
                        // TODO: Add compatibility check logic here if modelInfo.Version mismatches expected version
                        // or if modelInfo.Definition is needed for mapping.
                    }
                }

                // 2. Map current data to DigitalTwinUpdateRequest
                DigitalTwinUpdateRequest? updateRequest = null;
                string? mappingIdentifier = platformConfig.MappingRuleId ?? platformConfig.MappingRulePath;

                if (!string.IsNullOrEmpty(mappingIdentifier))
                {
                    // The data mapper might need context about the modelInfo for accurate mapping
                    updateRequest = await _dataMapper.MapToExternalFormatAsync<object, DigitalTwinUpdateRequest>(currentTwinData, mappingIdentifier, cancellationToken);
                }
                else
                {
                    // Basic assumption for direct mapping if data is already in the correct format or can be easily serialized
                    if (currentTwinData is DigitalTwinUpdateRequest directRequest) updateRequest = directRequest;
                    else if (currentTwinData is JsonDocument jsonDoc) updateRequest = new DigitalTwinUpdateRequest(twinInstanceId, jsonDoc); // Example
                    else
                    {
                         // Attempt to serialize currentTwinData into a JsonDocument for a generic PATCH.
                        // This requires currentTwinData to be structured appropriately.
                        var serializedData = JsonSerializer.SerializeToDocument(currentTwinData);
                        updateRequest = new DigitalTwinUpdateRequest(twinInstanceId, serializedData);
                    }
                }


                if (updateRequest == null)
                {
                    _logger.LogWarning("Failed to map data to DigitalTwinUpdateRequest for Twin ID: {TwinInstanceId} on Platform: {PlatformId}", twinInstanceId, platformId);
                    return;
                }
                
                // Ensure the twin ID in the request matches the one we're syncing
                if (updateRequest.TwinId != twinInstanceId)
                {
                    _logger.LogWarning("Mapped DigitalTwinUpdateRequest TwinId ({MappedTwinId}) does not match target TwinInstanceId ({TargetTwinId}). Aborting sync.", updateRequest.TwinId, twinInstanceId);
                    // Potentially re-create the request with the correct TwinId if the payload is generic.
                    // For now, we'll assume the mapper or source data provides the correct TwinId in the payload structure.
                    // If DigitalTwinUpdateRequest is just a payload, then the adaptor's SyncDataAsync might take twinId separately.
                    // Let's assume DigitalTwinUpdateRequest includes the TwinId it pertains to.
                }


                // 3. Send update to Digital Twin platform via adaptor
                await adaptor.SyncDataAsync(updateRequest, cancellationToken);
                _logger.LogInformation("Successfully synchronized data for Twin ID: {TwinInstanceId} on Platform: {PlatformId}", twinInstanceId, platformId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing data for Twin ID: {TwinInstanceId} on Platform: {PlatformId}", twinInstanceId, platformId);
            }
        }

        // Method for OpcDataConsumer or other event-driven triggers
        public async Task ProcessIncomingDataForDigitalTwinsAsync(object data, string sourceTwinId, CancellationToken cancellationToken)
        {
            // This method would determine which configured digital twin platforms
            // are interested in this data (e.g., based on sourceTwinId or data content)
            // and then call SyncDataForTwinAsync.
            // For simplicity, let's assume a direct mapping or that all platforms get it.
            foreach(var platformId in _activeAdaptors.Keys)
            {
                // The `sourceTwinId` here might be an OPC tag or an asset ID that needs to be
                // mapped to a `twinInstanceId` on the specific digital twin platform.
                // This mapping logic could be part of the `DataMappingService` or configuration.
                // For this example, we'll assume sourceTwinId is directly usable or mapped simply.
                string targetTwinInstanceIdOnPlatform = ResolveTargetTwinInstanceId(platformId, sourceTwinId);
                if (!string.IsNullOrEmpty(targetTwinInstanceIdOnPlatform))
                {
                    await SyncDataForTwinAsync(platformId, targetTwinInstanceIdOnPlatform, data, cancellationToken);
                }
            }
        }

        private string ResolveTargetTwinInstanceId(string platformId, string sourceTwinId)
        {
            // Placeholder for logic to map a source ID (e.g., from OPC) to a specific twin instance ID
            // on the target digital twin platform. This could involve looking up a configuration
            // or applying a naming convention.
            // Example: return $"{platformId}_{sourceTwinId}";
            return sourceTwinId; // Simplistic assumption: sourceTwinId is the twinInstanceId
        }

        // Handle incoming commands/setpoints from Digital Twins
        public async Task HandleIncomingTwinUpdateAsync(string platformId, string twinInstanceId, object updateData, CancellationToken cancellationToken)
        {
             _logger.LogInformation("Received update from Digital Twin Platform {PlatformId} for Twin {TwinInstanceId}.", platformId, twinInstanceId);
            // 1. Validate incoming data
            // 2. Map to internal format using _dataMapper if needed
            // 3. Dispatch command/setpoint (e.g., publish to message queue for REPO-OPC-CORE)
            _logger.LogInformation("Dispatch logic for update from Twin {TwinInstanceId} to be implemented.", twinInstanceId);
        }
    }
}