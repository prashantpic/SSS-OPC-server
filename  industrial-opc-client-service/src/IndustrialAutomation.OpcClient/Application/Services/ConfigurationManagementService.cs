using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Domain.Models;
using IndustrialAutomation.OpcClient.Domain.Exceptions;
using IndustrialAutomation.OpcClient.Infrastructure.Configuration;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class ConfigurationManagementService : IConfigurationManagementService
    {
        private readonly ILogger<ConfigurationManagementService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITagConfigurationImporter _tagImporter;
        private readonly IServerAppGrpcClient? _grpcClient; // Optional, for server-pushed config
        private readonly AppSettingsProvider _appSettingsProvider;

        private ClientConfigurationDto _currentConfiguration;
        private ConcurrentDictionary<string, TagDefinitionDto> _tagDefinitions;

        public ConfigurationManagementService(
            ILogger<ConfigurationManagementService> logger,
            IConfiguration configuration,
            ITagConfigurationImporter tagImporter,
            AppSettingsProvider appSettingsProvider,
            // IServerAppGrpcClient grpcClient = null // DI will provide if registered
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tagImporter = tagImporter ?? throw new ArgumentNullException(nameof(tagImporter));
            _appSettingsProvider = appSettingsProvider ?? throw new ArgumentNullException(nameof(appSettingsProvider));
            // _grpcClient = grpcClient;
            _grpcClient = serviceProvider.GetService(typeof(IServerAppGrpcClient)) as IServerAppGrpcClient;


            _currentConfiguration = new ClientConfigurationDto
            {
                ServerConnections = new List<ServerConnectionConfigDto>(),
                TagDefinitions = new List<TagDefinitionDto>(),
                UaSubscriptions = new List<DTOs.Ua.UaSubscriptionConfigDto>(),
                ValidationRules = new List<ValidationRule>(),
                WriteLimitPolicies = new List<WriteLimitPolicy>()
            };
            _tagDefinitions = new ConcurrentDictionary<string, TagDefinitionDto>();
        }

        public async Task LoadInitialConfigurationAsync()
        {
            _logger.LogInformation("Loading initial client configuration...");

            try
            {
                _currentConfiguration.ClientId = _appSettingsProvider.GetOpcClientConfig()?.ClientId ?? Guid.NewGuid().ToString();
                _currentConfiguration.ServerConnections = _appSettingsProvider.GetServerConnectionConfigs()?.ToList() ?? new List<ServerConnectionConfigDto>();
                _currentConfiguration.UaSubscriptions = _appSettingsProvider.GetUaSubscriptionConfigs()?.ToList() ?? new List<DTOs.Ua.UaSubscriptionConfigDto>();
                _currentConfiguration.ValidationRules = _appSettingsProvider.GetValidationRules()?.ToList() ?? new List<ValidationRule>();
                _currentConfiguration.WriteLimitPolicies = _appSettingsProvider.GetWriteLimitPolicies()?.ToList() ?? new List<WriteLimitPolicy>();
                _currentConfiguration.ActiveEdgeModel = _appSettingsProvider.GetEdgeAiConfig()?.ActiveModel != null ? 
                    new DTOs.EdgeAi.EdgeModelMetadataDto { 
                        ModelName = _appSettingsProvider.GetEdgeAiConfig()!.ActiveModel!, 
                        Version = _appSettingsProvider.GetEdgeAiConfig()!.ActiveModelVersion ?? "latest" // Example
                    } : null;


                var tagImportConfigs = _appSettingsProvider.GetTagImportConfigs();
                if (tagImportConfigs != null)
                {
                    foreach (var importConfig in tagImportConfigs)
                    {
                        if (!string.IsNullOrEmpty(importConfig.FilePath))
                        {
                             _logger.LogInformation("Importing tags from file: {FilePath}", importConfig.FilePath);
                            var importedTags = await _tagImporter.ImportTagsAsync(importConfig);
                            MergeTagDefinitions(importedTags);
                        }
                    }
                }
                
                _currentConfiguration.TagDefinitions = _tagDefinitions.Values.ToList();

                _logger.LogInformation("Initial configuration loaded successfully. ClientId: {ClientId}, {TagCount} tags loaded.", _currentConfiguration.ClientId, _tagDefinitions.Count);
            }
            catch (ConfigurationImportException cie)
            {
                _logger.LogError(cie, "Failed to import tag configuration from file: {FilePath}", cie.FilePath);
                // Decide if this is fatal or if the service can continue with partial/no tags
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load initial configuration.");
                // This might be a fatal error depending on requirements
                throw;
            }
        }
        
        public async Task FetchAndApplyServerConfigurationAsync()
        {
            if (_grpcClient == null)
            {
                _logger.LogInformation("gRPC client not available. Skipping server configuration fetch.");
                return;
            }

            _logger.LogInformation("Fetching configuration from server for ClientId: {ClientId}...", _currentConfiguration.ClientId);
            try
            {
                var serverConfig = await _grpcClient.GetConfigurationAsync(_currentConfiguration.ClientId ?? Guid.NewGuid().ToString());
                if (serverConfig != null)
                {
                    _logger.LogInformation("Successfully fetched configuration from server. Applying...");
                    await ApplyServerConfigurationAsync(serverConfig);
                }
                else
                {
                    _logger.LogWarning("No configuration received from server or server configuration was null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch or apply configuration from server.");
                // Potentially retry or use cached/default config
            }
        }


        public Task ApplyServerConfigurationAsync(ClientConfigurationDto config)
        {
            if (config == null)
            {
                _logger.LogWarning("Received null configuration from server. No changes applied.");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Applying new configuration received from server.");

            // Perform a merge or replacement strategy based on requirements
            // Example: Replace lists, update specific properties
            if (config.ServerConnections != null) _currentConfiguration.ServerConnections = config.ServerConnections;
            if (config.UaSubscriptions != null) _currentConfiguration.UaSubscriptions = config.UaSubscriptions;
            if (config.ValidationRules != null) _currentConfiguration.ValidationRules = config.ValidationRules;
            if (config.WriteLimitPolicies != null) _currentConfiguration.WriteLimitPolicies = config.WriteLimitPolicies;
            if (config.ActiveEdgeModel != null) _currentConfiguration.ActiveEdgeModel = config.ActiveEdgeModel;

            if (config.TagDefinitions != null)
            {
                MergeTagDefinitions(config.TagDefinitions);
                _currentConfiguration.TagDefinitions = _tagDefinitions.Values.ToList();
            }
            
            // TODO: Notify other services (OpcCommunicationService, EdgeIntelligenceService) about the configuration changes
            // This might involve raising an event or calling methods on those services.
            // e.g., _opcCommunicationService.Reconfigure(newConfig);
            // e.g., _edgeIntelligenceService.ApplyModelConfiguration(newConfig.ActiveEdgeModel);

            _logger.LogInformation("Server configuration applied. {TagCount} total tags.", _tagDefinitions.Count);
            LogCurrentConfiguration();
            return Task.CompletedTask;
        }

        private void MergeTagDefinitions(List<TagDefinitionDto> newTags)
        {
            if (newTags == null) return;

            foreach (var tag in newTags)
            {
                if (string.IsNullOrEmpty(tag.TagId))
                {
                    _logger.LogWarning("Skipping tag with null or empty TagId during merge: OpcAddress {OpcAddress}", tag.OpcAddress);
                    continue;
                }
                _tagDefinitions.AddOrUpdate(tag.TagId, tag, (key, oldValue) => tag); // Replace existing
                 _logger.LogDebug("Merged tag: {TagId} - {OpcAddress}", tag.TagId, tag.OpcAddress);
            }
        }

        public TagDefinitionDto? GetTagDefinition(string tagId)
        {
            if (string.IsNullOrEmpty(tagId)) return null;
            _tagDefinitions.TryGetValue(tagId, out var tagDefinition);
            return tagDefinition;
        }

        public IEnumerable<TagDefinitionDto> GetAllTagDefinitions()
        {
            return _tagDefinitions.Values.ToList();
        }

        public ClientConfigurationDto GetCurrentConfiguration()
        {
            // Ensure the TagDefinitions list in currentConfiguration is up-to-date
            _currentConfiguration.TagDefinitions = _tagDefinitions.Values.ToList();
            return _currentConfiguration;
        }

        private void LogCurrentConfiguration()
        {
            // Be careful about logging sensitive data like passwords if they are part of the DTOs
            _logger.LogInformation("Current Client ID: {ClientId}", _currentConfiguration.ClientId);
            _logger.LogInformation("Number of Server Connections: {Count}", _currentConfiguration.ServerConnections.Count);
            _logger.LogInformation("Number of Tag Definitions: {Count}", _currentConfiguration.TagDefinitions.Count);
            _logger.LogInformation("Number of UA Subscriptions: {Count}", _currentConfiguration.UaSubscriptions.Count);
            // Log other important config details as needed
        }
    }
}