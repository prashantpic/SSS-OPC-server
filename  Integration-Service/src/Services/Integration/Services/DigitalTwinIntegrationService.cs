using IntegrationService.Interfaces;
using IntegrationService.Adapters.DigitalTwin.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationService.Configuration;
using IntegrationService.Adapters.DigitalTwin; // Added for specific adaptor instantiation
using System.Net.Http; // Added for IHttpClientFactory
using IntegrationService.Resiliency; // Added for policy factories

namespace IntegrationService.Services
{
    /// <summary>
    /// Orchestrates Digital Twin integrations, managing data synchronization and version compatibility.
    /// </summary>
    public class DigitalTwinIntegrationService
    {
        private readonly ILogger<DigitalTwinIntegrationService> _logger;
        private readonly IntegrationSettings _settings;
        private readonly IDataMapper _dataMapper;
        private readonly ICredentialManager _credentialManager;
        private readonly Dictionary<string, IDigitalTwinAdaptor> _adaptors = new Dictionary<string, IDigitalTwinAdaptor>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory; // To create specific loggers for adaptors
        private readonly RetryPolicyFactory _retryPolicyFactory;
        private readonly CircuitBreakerPolicyFactory _circuitBreakerPolicyFactory;


        public DigitalTwinIntegrationService(
            ILogger<DigitalTwinIntegrationService> logger,
            IOptions<IntegrationSettings> settings,
            IDataMapper dataMapper,
            ICredentialManager credentialManager,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _logger = logger;
            _settings = settings.Value;
            _dataMapper = dataMapper;
            _credentialManager = credentialManager;
            _httpClientFactory = httpClientFactory;
            _loggerFactory = loggerFactory;
            _retryPolicyFactory = retryPolicyFactory;
            _circuitBreakerPolicyFactory = circuitBreakerPolicyFactory;

             _logger.LogInformation("DigitalTwinIntegrationService initialized.");
            InitializeAdaptors();
        }

        private void InitializeAdaptors()
        {
            _logger.LogInformation("Initializing Digital Twin adaptors...");
            var dtSettings = _settings.DigitalTwinSettings;
            var featureFlags = _settings.FeatureFlags;

            foreach (var twinConfig in dtSettings.Twins)
            {
                IDigitalTwinAdaptor? adaptor = null;
                bool enabled = false;

                switch (twinConfig.Type.ToUpperInvariant())
                {
                    case "AZUREDIGITALTWINS":
                    case "HTTP":
                         if (featureFlags.EnableDigitalTwinSync)
                        {
                            adaptor = new HttpDigitalTwinAdaptor(twinConfig,
                                _loggerFactory.CreateLogger<HttpDigitalTwinAdaptor>(),
                                _credentialManager, _httpClientFactory, _retryPolicyFactory, _circuitBreakerPolicyFactory);
                             enabled = true;
                        } else { _logger.LogInformation("Digital Twin sync is disabled by feature flag for twin '{TwinId}'.", twinConfig.Id); }
                        break;
                    default:
                        _logger.LogWarning("Unsupported Digital Twin platform type configured: {PlatformType} for twin '{TwinId}'. Skipping.", twinConfig.Type, twinConfig.Id);
                        break;
                }

                if (adaptor != null && enabled)
                {
                     if (_adaptors.ContainsKey(adaptor.Id))
                    {
                         _logger.LogWarning("Duplicate Digital Twin configuration ID found: '{TwinId}'. Only the first instance will be used.", adaptor.Id);
                         (adaptor as IDisposable)?.Dispose();
                    }
                    else
                    {
                        _adaptors.Add(adaptor.Id, adaptor);
                         _logger.LogInformation("Added Digital Twin adaptor '{Id}' of type {Type}.", adaptor.Id, twinConfig.Type);

                        if (twinConfig.EnableBiDirectional && featureFlags.EnableDigitalTwinSync)
                        {
                             _logger.LogInformation("Setting up twin change subscription for bi-directional Digital Twin adaptor '{Id}'.", adaptor.Id);
                             adaptor.SubscribeToTwinChangesAsync(twinConfig.Id, (change) => HandleIncomingTwinChange(change, adaptor.Id))
                                 .ContinueWith(task =>
                                 {
                                     if (task.IsFaulted)
                                     {
                                         _logger.LogError(task.Exception, "Failed to subscribe to twin changes for Digital Twin adaptor '{Id}'.", adaptor.Id);
                                     }
                                 });
                        }
                    }
                }
            }
             _logger.LogInformation("Digital Twin adaptor initialization finished. {Count} adaptors initialized.", _adaptors.Count);

            var mappingRuleIds = dtSettings.Twins.Select(t => t.DataMappingRuleId).Where(id => !string.IsNullOrEmpty(id)).Distinct();
             if (mappingRuleIds.Any())
            {
                _dataMapper.LoadAllConfiguredRulesAsync(mappingRuleIds).GetAwaiter().GetResult();
            }
        }

        public async Task SynchronizeTwinDataAsync(string twinId)
        {
            if (!_settings.FeatureFlags.EnableDigitalTwinSync)
            {
                 _logger.LogDebug("Digital Twin sync is disabled by feature flag. Skipping sync for twin {TwinId}.", twinId);
                 return;
            }

            _logger.LogInformation("Starting data synchronization for Digital Twin '{TwinId}'.", twinId);

            if (!_adaptors.TryGetValue(twinId, out var adaptor))
            {
                 _logger.LogError("No Digital Twin adaptor found for twin ID '{TwinId}'. Cannot synchronize.", twinId);
                 return;
            }

            var twinConfig = _settings.DigitalTwinSettings.Twins.FirstOrDefault(t => t.Id == twinId);
            if (twinConfig == null)
            {
                 _logger.LogError("Configuration not found for Digital Twin '{TwinId}'. Cannot synchronize.", twinId);
                 return;
            }

            if (!adaptor.IsConnected)
            {
                 _logger.LogWarning("Digital Twin adaptor '{TwinId}' is not connected. Attempting to connect before syncing.", twinId);
                try
                {
                    await adaptor.ConnectAsync();
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Failed to connect Digital Twin adaptor '{TwinId}'. Cannot synchronize.", twinId);
                     return;
                }
            }

            try
            {
                 _logger.LogDebug("Checking model compatibility for twin '{TwinId}' (Target Model: {TargetModelId}).", twinId, twinConfig.TargetModelId);
                var modelInfo = await adaptor.GetTwinModelInfoAsync(twinId); // twinId here is the adaptor.Id which is twinConfig.Id
                if (modelInfo == null || !IsModelCompatible(modelInfo, twinConfig.TargetModelId))
                {
                     _logger.LogWarning("Digital Twin model '{ModelId}' (Version: {Version}) for twin '{TwinId}' is not compatible with target '{TargetModelId}'. Skipping sync.",
                        modelInfo?.ModelId, modelInfo?.Version, twinId, twinConfig.TargetModelId);
                    return;
                }
                 _logger.LogDebug("Digital Twin model '{ModelId}' for twin '{TwinId}' is compatible.", modelInfo.ModelId, twinId);

                 _logger.LogWarning("Placeholder: Retrieving latest data from internal systems for twin '{TwinId}'. Requires REPO-DATA-SERVICE integration.", twinId);
                var latestInternalData = GetLatestInternalDataForTwin(twinId);

                if (latestInternalData == null)
                {
                     _logger.LogWarning("No latest internal data found for twin '{TwinId}'. Skipping sync update.", twinId);
                     return;
                }

                DigitalTwinUpdateRequest updateRequest;
                if (!string.IsNullOrEmpty(twinConfig.DataMappingRuleId))
                {
                     if (!_dataMapper.AreRulesLoaded(twinConfig.DataMappingRuleId))
                    {
                        _logger.LogWarning("Mapping rules '{MappingRuleId}' for twin '{TwinId}' are not loaded. Attempting to load.", twinConfig.DataMappingRuleId, twinId);
                        await _dataMapper.LoadMappingRulesAsync(twinConfig.DataMappingRuleId);
                         if (!_dataMapper.AreRulesLoaded(twinConfig.DataMappingRuleId))
                         {
                             _logger.LogError("Failed to load mapping rules '{MappingRuleId}' for twin '{TwinId}'. Cannot map and send data.", twinConfig.DataMappingRuleId, twinId);
                             return;
                         }
                    }
                     _logger.LogDebug("Mapping internal data using rules '{MappingRuleId}' for twin '{TwinId}'.", twinConfig.DataMappingRuleId, twinId);
                     updateRequest = _dataMapper.Map<object, DigitalTwinUpdateRequest>(latestInternalData, twinConfig.DataMappingRuleId);
                }
                else
                {
                     _logger.LogWarning("No mapping rule ID configured for twin '{TwinId}'. Cannot map and send data.", twinId);
                     return;
                }
                 updateRequest = updateRequest with { UpdateType = "PropertyUpdate" }; // Assuming sync is PropertyUpdate

                 await adaptor.UpdateTwinAsync(twinId, updateRequest); // twinId here is the DT's actual ID
                 _logger.LogInformation("Successfully synchronized data to Digital Twin '{TwinId}'.", twinId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing data for Digital Twin '{TwinId}'.", twinId);
            }
        }

        private void HandleIncomingTwinChange(object twinChangePayload, string sourceAdaptorId)
        {
             _logger.LogInformation("Received incoming Digital Twin change/command payload from adaptor '{SourceAdaptorId}'.", sourceAdaptorId);
             _logger.LogWarning("Placeholder: Received incoming Digital Twin change/command from '{SourceAdaptorId}'. Implement logic to process payload and interact with target systems.", sourceAdaptorId);
        }

        private object? GetLatestInternalDataForTwin(string twinId)
        {
             _logger.LogWarning("Placeholder method GetLatestInternalDataForTwin called for twin '{TwinId}'. Requires actual data retrieval logic.", twinId);
            return new { SimulatedValue1 = 123.45, SimulatedStatus = "Running", LastUpdate = DateTimeOffset.UtcNow };
        }

        private bool IsModelCompatible(DigitalTwinModelInfo currentModel, string targetModelId)
        {
             _logger.LogDebug("Checking compatibility for current model '{CurrentId}' (Version: {CurrentVersion}) vs target '{TargetId}'.",
                currentModel.ModelId, currentModel.Version, targetModelId);

            if (string.IsNullOrEmpty(targetModelId)) {
                _logger.LogDebug("No target model ID specified for twin, assuming compatible.");
                return true; // If no target specified, assume compatible
            }
            if (string.IsNullOrEmpty(currentModel.ModelId)) {
                _logger.LogWarning("Current model ID is empty for twin. Cannot verify compatibility with target '{TargetId}'.", targetModelId);
                return false; // If current model ID is unknown, cannot confirm
            }

            if (!currentModel.ModelId.Equals(targetModelId, StringComparison.OrdinalIgnoreCase))
            {
                 _logger.LogWarning("Model ID mismatch: '{CurrentId}' != '{TargetId}'.", currentModel.ModelId, targetModelId);
                 return false;
            }
             _logger.LogInformation("Placeholder: Model ID matches. Implement robust version compatibility check if versions are used.");
            return true;
        }

        public async Task ConnectAllAdaptorsAsync()
        {
             _logger.LogInformation("Attempting to connect all initialized Digital Twin adaptors...");
            var connectTasks = _adaptors.Values.Select(adaptor => adaptor.ConnectAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                     _logger.LogError(task.Exception, "Failed to connect Digital Twin adaptor '{AdaptorId}'.", adaptor.Id);
                } else {
                     _logger.LogInformation("Digital Twin adaptor '{AdaptorId}' connection task completed. IsConnected: {IsConnected}", adaptor.Id, adaptor.IsConnected);
                }
            })).ToList();

            await Task.WhenAll(connectTasks);
             _logger.LogInformation("Finished attempting to connect all Digital Twin adaptors.");
        }

         public async Task DisconnectAllAdaptorsAsync()
        {
             _logger.LogInformation("Attempting to disconnect all initialized Digital Twin adaptors...");
            var disconnectTasks = _adaptors.Values.Select(adaptor => adaptor.DisconnectAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                     _logger.LogError(task.Exception, "Failed to disconnect Digital Twin adaptor '{AdaptorId}'.", adaptor.Id);
                } else {
                     _logger.LogInformation("Digital Twin adaptor '{AdaptorId}' disconnection task completed.", adaptor.Id);
                }
            })).ToList();

            await Task.WhenAll(disconnectTasks);
             _logger.LogInformation("Finished attempting to disconnect all Digital Twin adaptors.");
        }
    }
}