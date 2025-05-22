using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Infrastructure.Configuration;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IServerAppGrpcClient _grpcClient; // For fetching updates from server
        private readonly AppSettingsProvider _appSettingsProvider;


        private ClientConfigurationDto _currentConfiguration = new();
        private readonly ConcurrentDictionary<string, TagDefinitionDto> _tagDefinitions = new();
        private readonly ConcurrentDictionary<string, ServerConnectionConfigDto> _serverConnections = new();
        private readonly ConcurrentDictionary<string, UaSubscriptionConfigDto> _uaSubscriptions = new();
        // Add other configuration items like ValidationRules, WriteLimitPolicies etc. as needed

        public string ClientId { get; private set; }

        public ConfigurationManagementService(
            ILogger<ConfigurationManagementService> logger,
            IConfiguration configuration,
            ITagConfigurationImporter tagImporter,
            IServerAppGrpcClient grpcClient,
            IOptions<ClientConfigurationDto> initialClientConfigOptions, /* If bound directly */
            AppSettingsProvider appSettingsProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tagImporter = tagImporter ?? throw new ArgumentNullException(nameof(tagImporter));
            _grpcClient = grpcClient; // Optional, if server-side config pull is implemented
            _appSettingsProvider = appSettingsProvider ?? throw new ArgumentNullException(nameof(appSettingsProvider));

            // Initialize with directly bound options if available, then override with appsettings
            if (initialClientConfigOptions?.Value != null)
            {
                _currentConfiguration = initialClientConfigOptions.Value;
                ClientId = _currentConfiguration.ClientId;
                UpdateInternalCollections(_currentConfiguration);
            }
        }

        public async Task LoadInitialConfigurationAsync()
        {
            _logger.LogInformation("Loading initial client configuration...");

            // 1. Load from appsettings.json (handled by AppSettingsProvider)
            ClientId = _appSettingsProvider.GetOpcClientConfig()?.ClientId ?? Guid.NewGuid().ToString();
            _currentConfiguration.ClientId = ClientId;

            var serverConnConfigs = _appSettingsProvider.GetServerConnectionConfigs();
            if (serverConnConfigs != null)
            {
                _currentConfiguration.ServerConnections = serverConnConfigs;
                foreach (var sc in serverConnConfigs) { _serverConnections[sc.ServerId] = sc; }
            }
            
            var uaSubConfigs = _appSettingsProvider.GetUaSubscriptionConfigs();
             if (uaSubConfigs != null)
            {
                _currentConfiguration.UaSubscriptions = uaSubConfigs;
                foreach (var sub in uaSubConfigs) { _uaSubscriptions[sub.SubscriptionId] = sub; }
            }


            // 2. Import tags if configured
            var tagImportConfigs = _appSettingsProvider.GetTagImportConfigs();
            if (tagImportConfigs != null)
            {
                foreach (var importConfig in tagImportConfigs)
                {
                    try
                    {
                        _logger.LogInformation("Importing tags from {FilePath} ({FileType})", importConfig.FilePath, importConfig.FileType);
                        var importedTags = await _tagImporter.ImportTagsAsync(importConfig);
                        if (_currentConfiguration.TagDefinitions == null) _currentConfiguration.TagDefinitions = new List<TagDefinitionDto>();
                        _currentConfiguration.TagDefinitions.AddRange(importedTags);
                        foreach (var tag in importedTags) { _tagDefinitions[tag.TagId] = tag; }
                        _logger.LogInformation("Successfully imported {Count} tags from {FilePath}", importedTags.Count, importConfig.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to import tags from {FilePath}", importConfig.FilePath);
                        // Decide if this is a fatal error or if the service can continue
                    }
                }
            }
            
            // Load other configurations like validation rules, write limits from AppSettingsProvider
            _currentConfiguration.ValidationRules = _appSettingsProvider.GetValidationRules() ?? new List<ValidationRule>();
            _currentConfiguration.WriteLimitPolicies = _appSettingsProvider.GetWriteLimitPolicies() ?? new List<WriteLimitPolicy>();
            _currentConfiguration.ActiveEdgeModel = _appSettingsProvider.GetEdgeAiConfig()?.ActiveModel != null ? 
                new EdgeModelMetadataDto(_appSettingsProvider.GetEdgeAiConfig().ActiveModel, "1.0", _appSettingsProvider.GetEdgeAiConfig().ActiveModel, Path.Combine(_appSettingsProvider.GetEdgeAiConfig().ModelPath ?? "", _appSettingsProvider.GetEdgeAiConfig().ActiveModel), DateTime.UtcNow, new Dictionary<string, string>(), new Dictionary<string, string>()) 
                : null;


            _logger.LogInformation("Initial client configuration loaded for ClientId: {ClientId}. {TagCount} tags, {ServerCount} servers.", ClientId, _tagDefinitions.Count, _serverConnections.Count);

            // Optionally, fetch configuration from server if gRPC client is available
            if (_grpcClient != null)
            {
                try
                {
                    _logger.LogInformation("Attempting to fetch configuration from server for ClientId: {ClientId}", ClientId);
                    var serverConfig = await _grpcClient.GetConfigurationAsync(ClientId);
                    if (serverConfig != null)
                    {
                        _logger.LogInformation("Successfully fetched configuration from server. Applying...");
                        await ApplyServerConfigurationAsync(serverConfig);
                    }
                    else
                    {
                        _logger.LogWarning("No configuration returned from server for ClientId: {ClientId}", ClientId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch or apply configuration from server for ClientId: {ClientId}. Using local configuration.", ClientId);
                }
            }
        }

        public Task ApplyServerConfigurationAsync(ClientConfigurationDto config)
        {
            if (config == null)
            {
                _logger.LogWarning("Received null configuration from server. No changes applied.");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Applying new configuration received from server for ClientId: {ClientId}", config.ClientId);
            
            // Merge or replace strategy for different parts of config
            // Example: For tags, server connections, subscriptions, could be a full replace or a merge
            // For simplicity, this example will largely replace lists but preserve ClientId
            
            _currentConfiguration = config with { ClientId = this.ClientId }; // Preserve original ClientId
            UpdateInternalCollections(_currentConfiguration);

            // TODO: Notify other services (OpcCommunicationService, EdgeIntelligenceService, etc.)
            // about the configuration changes so they can adapt (e.g., reconnect servers, update subscriptions, load new AI model).
            // This can be done via events, direct calls if tightly coupled, or by services re-querying this service.
            // For example:
            // _eventAggregator.Publish(new ConfigurationUpdatedEvent(_currentConfiguration));

            _logger.LogInformation("Server configuration applied. {TagCount} tags, {ServerCount} servers.", _tagDefinitions.Count, _serverConnections.Count);
            return Task.CompletedTask;
        }


        private void UpdateInternalCollections(ClientConfigurationDto config)
        {
            _tagDefinitions.Clear();
            if (config.TagDefinitions != null)
            {
                foreach (var tag in config.TagDefinitions) _tagDefinitions[tag.TagId] = tag;
            }

            _serverConnections.Clear();
            if (config.ServerConnections != null)
            {
                foreach (var sc in config.ServerConnections) _serverConnections[sc.ServerId] = sc;
            }
            
            _uaSubscriptions.Clear();
            if (config.UaSubscriptions != null)
            {
                foreach (var sub in config.UaSubscriptions) _uaSubscriptions[sub.SubscriptionId] = sub;
            }
        }

        public TagDefinitionDto GetTagDefinition(string tagId)
        {
            return _tagDefinitions.TryGetValue(tagId, out var tagDef) ? tagDef : null;
        }

        public IEnumerable<TagDefinitionDto> GetAllTagDefinitions()
        {
            return _tagDefinitions.Values.ToList();
        }

        public IEnumerable<ServerConnectionConfigDto> GetServerConnections()
        {
            return _serverConnections.Values.ToList();
        }
        
        public IEnumerable<UaSubscriptionConfigDto> GetUaSubscriptions()
        {
            return _uaSubscriptions.Values.ToList();
        }

        public ClientConfigurationDto GetCurrentConfiguration()
        {
            // Ensure the _currentConfiguration reflects the dictionaries
             _currentConfiguration.TagDefinitions = _tagDefinitions.Values.ToList();
             _currentConfiguration.ServerConnections = _serverConnections.Values.ToList();
             _currentConfiguration.UaSubscriptions = _uaSubscriptions.Values.ToList();
            // ... and other parts like validation rules etc.
            return _currentConfiguration;
        }
    }
}