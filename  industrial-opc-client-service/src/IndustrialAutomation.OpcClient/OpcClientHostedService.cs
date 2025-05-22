using IndustrialAutomation.OpcClient.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration; // For ClientConfigurationDto etc.

namespace IndustrialAutomation.OpcClient
{
    public class OpcClientHostedService : IHostedService
    {
        private readonly ILogger<OpcClientHostedService> _logger;
        private readonly IConfigurationManagementService _configurationManagementService;
        private readonly IOpcCommunicationService _opcCommunicationService;
        private readonly IEdgeIntelligenceService _edgeIntelligenceService;
        // IDataTransmissionService can be injected if this service needs to send status updates directly
        // private readonly IDataTransmissionService _dataTransmissionService;

        private ClientConfigurationDto? _clientConfiguration;

        public OpcClientHostedService(
            ILogger<OpcClientHostedService> logger,
            IConfigurationManagementService configurationManagementService,
            IOpcCommunicationService opcCommunicationService,
            IEdgeIntelligenceService edgeIntelligenceService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationManagementService = configurationManagementService ?? throw new ArgumentNullException(nameof(configurationManagementService));
            _opcCommunicationService = opcCommunicationService ?? throw new ArgumentNullException(nameof(opcCommunicationService));
            _edgeIntelligenceService = edgeIntelligenceService ?? throw new ArgumentNullException(nameof(edgeIntelligenceService));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OPC Client Hosted Service is starting.");

            try
            {
                await _configurationManagementService.LoadInitialConfigurationAsync();
                _clientConfiguration = await _configurationManagementService.GetClientConfigurationAsync(); // Assuming this method will be available

                if (_clientConfiguration == null)
                {
                    _logger.LogError("Client configuration could not be loaded. OPC Client Service will not start core functionalities.");
                    return;
                }

                _logger.LogInformation("Client ID: {ClientId}", _clientConfiguration.ClientId);

                // Connect to OPC Servers
                if (_clientConfiguration.ServerConnections != null)
                {
                    foreach (var serverConfig in _clientConfiguration.ServerConnections)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        try
                        {
                            _logger.LogInformation("Connecting to OPC server: {ServerId} at {EndpointUrl}", serverConfig.ServerId, serverConfig.EndpointUrl);
                            await _opcCommunicationService.ConnectAsync(serverConfig);
                            _logger.LogInformation("Successfully connected to OPC server: {ServerId}", serverConfig.ServerId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to connect to OPC server: {ServerId} at {EndpointUrl}", serverConfig.ServerId, serverConfig.EndpointUrl);
                        }
                    }
                }

                // Create OPC UA Subscriptions
                if (_clientConfiguration.UaSubscriptions != null)
                {
                    foreach (var subscriptionConfig in _clientConfiguration.UaSubscriptions)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        try
                        {
                            _logger.LogInformation("Creating OPC UA subscription: {SubscriptionId} on server: {ServerId}", subscriptionConfig.SubscriptionId, subscriptionConfig.ServerId);
                            await _opcCommunicationService.CreateSubscriptionAsync(subscriptionConfig.ServerId, subscriptionConfig);
                            _logger.LogInformation("Successfully created OPC UA subscription: {SubscriptionId}", subscriptionConfig.SubscriptionId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to create OPC UA subscription: {SubscriptionId} on server: {ServerId}", subscriptionConfig.SubscriptionId, subscriptionConfig.ServerId);
                        }
                    }
                }

                // Load Edge AI Model
                if (_clientConfiguration.ActiveEdgeModel != null && !string.IsNullOrEmpty(_clientConfiguration.ActiveEdgeModel.ModelName))
                {
                     if (cancellationToken.IsCancellationRequested) return;
                    try
                    {
                        _logger.LogInformation("Loading Edge AI model: {ModelName} version {Version}", _clientConfiguration.ActiveEdgeModel.ModelName, _clientConfiguration.ActiveEdgeModel.Version);
                        await _edgeIntelligenceService.LoadModelAsync(_clientConfiguration.ActiveEdgeModel.ModelName, _clientConfiguration.ActiveEdgeModel.Version);
                        _logger.LogInformation("Successfully loaded Edge AI model: {ModelName}", _clientConfiguration.ActiveEdgeModel.ModelName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load Edge AI model: {ModelName}", _clientConfiguration.ActiveEdgeModel.ModelName);
                    }
                }

                _logger.LogInformation("OPC Client Hosted Service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unhandled exception occurred during OPC Client Hosted Service startup.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OPC Client Hosted Service is stopping.");

            if (_clientConfiguration != null)
            {
                // Disconnect from OPC Servers
                if (_clientConfiguration.ServerConnections != null)
                {
                    foreach (var serverConfig in _clientConfiguration.ServerConnections.Reverse<ServerConnectionConfigDto>()) // Disconnect in reverse order of connection
                    {
                        try
                        {
                            _logger.LogInformation("Disconnecting from OPC server: {ServerId}", serverConfig.ServerId);
                            await _opcCommunicationService.DisconnectAsync(serverConfig.ServerId);
                            _logger.LogInformation("Successfully disconnected from OPC server: {ServerId}", serverConfig.ServerId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error disconnecting from OPC server: {ServerId}", serverConfig.ServerId);
                        }
                    }
                }

                // Optionally, remove subscriptions explicitly if not handled by DisconnectAsync or session cleanup
                if (_clientConfiguration.UaSubscriptions != null)
                {
                    foreach (var subConfig in _clientConfiguration.UaSubscriptions.Reverse<UaSubscriptionConfigDto>())
                    {
                         try
                        {
                            _logger.LogInformation("Removing OPC UA subscription: {SubscriptionId}", subConfig.SubscriptionId);
                            // Assuming RemoveSubscriptionAsync exists and is idempotent
                            await _opcCommunicationService.RemoveSubscriptionAsync(subConfig.SubscriptionId);
                            _logger.LogInformation("Successfully removed OPC UA subscription: {SubscriptionId}", subConfig.SubscriptionId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error removing OPC UA subscription: {SubscriptionId}", subConfig.SubscriptionId);
                        }
                    }
                }
            }
            
            // Unload Edge AI models or other cleanup for EdgeIntelligenceService if needed
            // Example: await _edgeIntelligenceService.UnloadAllModelsAsync();

            _logger.LogInformation("OPC Client Hosted Service stopped successfully.");
        }
    }
}