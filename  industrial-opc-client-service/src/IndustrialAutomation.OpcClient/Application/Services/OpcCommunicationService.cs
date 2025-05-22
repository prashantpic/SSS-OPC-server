using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda;
using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Domain.Enums;
using IndustrialAutomation.OpcClient.Domain.Exceptions;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ua;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Infrastructure.Policies;
using IndustrialAutomation.OpcClient.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class OpcCommunicationService : IOpcCommunicationService
    {
        private readonly ILogger<OpcCommunicationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClientSideDataValidator _validator;
        private readonly IWriteOperationLimiter _writeLimiter;
        private readonly ICriticalWriteLogger _criticalLogger;

        private readonly ConcurrentDictionary<string, IOpcConnection> _connections = new();
        private readonly ConcurrentDictionary<string, OpcStandard> _serverStandards = new();

        public OpcCommunicationService(
            ILogger<OpcCommunicationService> logger,
            IServiceProvider serviceProvider,
            IClientSideDataValidator validator,
            IWriteOperationLimiter writeLimiter,
            ICriticalWriteLogger criticalLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _writeLimiter = writeLimiter ?? throw new ArgumentNullException(nameof(writeLimiter));
            _criticalLogger = criticalLogger ?? throw new ArgumentNullException(nameof(criticalLogger));
        }

        public async Task ConnectAsync(ServerConnectionConfigDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(config.ServerId)) throw new ArgumentException("ServerId cannot be null or whitespace.", nameof(config.ServerId));

            _logger.LogInformation("Attempting to connect to OPC server: {ServerId} ({Standard}) at {EndpointUrl}", config.ServerId, config.Standard, config.EndpointUrl);

            IOpcConnection client = config.Standard switch
            {
                OpcStandard.Da => _serviceProvider.GetService(typeof(IOpcDaClient)) as IOpcDaClient ?? throw new InvalidOperationException($"IOpcDaClient not registered."),
                OpcStandard.Ua => _serviceProvider.GetService(typeof(IOpcUaClient)) as IOpcUaClient ?? throw new InvalidOperationException($"IOpcUaClient not registered."),
                OpcStandard.XmlDa => _serviceProvider.GetService(typeof(IOpcXmlDaClient)) as IOpcXmlDaClient ?? throw new InvalidOperationException($"IOpcXmlDaClient not registered."),
                OpcStandard.Hda => _serviceProvider.GetService(typeof(IOpcHdaClient)) as IOpcHdaClient ?? throw new InvalidOperationException($"IOpcHdaClient not registered."),
                OpcStandard.Ac => _serviceProvider.GetService(typeof(IOpcAcClient)) as IOpcAcClient ?? throw new InvalidOperationException($"IOpcAcClient not registered."),
                _ => throw new OpcCommunicationException($"Unsupported OPC standard: {config.Standard}", config.ServerId)
            };

            try
            {
                await client.ConnectAsync(config);
                _connections[config.ServerId] = client;
                _serverStandards[config.ServerId] = config.Standard;
                _logger.LogInformation("Successfully connected to OPC server: {ServerId}", config.ServerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to OPC server: {ServerId}", config.ServerId);
                throw new OpcCommunicationException($"Failed to connect to OPC server {config.ServerId}", config.ServerId, innerException: ex);
            }
        }

        public async Task DisconnectAsync(string serverId)
        {
            if (string.IsNullOrWhiteSpace(serverId)) throw new ArgumentException("ServerId cannot be null or whitespace.", nameof(serverId));

            if (_connections.TryRemove(serverId, out var client))
            {
                _serverStandards.TryRemove(serverId, out _);
                try
                {
                    await client.DisconnectAsync();
                    _logger.LogInformation("Disconnected from OPC server: {ServerId}", serverId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during disconnection from OPC server: {ServerId}", serverId);
                    // Potentially rethrow or handle as needed
                }
                finally
                {
                    if (client is IDisposable disposableClient)
                    {
                        disposableClient.Dispose();
                    }
                }
            }
            else
            {
                _logger.LogWarning("Attempted to disconnect from a non-existent or already disconnected server: {ServerId}", serverId);
            }
        }

        private async Task<T> GetClientAsync<T>(string serverId) where T : class, IOpcConnection
        {
            if (!_connections.TryGetValue(serverId, out var client))
            {
                throw new OpcCommunicationException($"No active connection found for server: {serverId}. Please connect first.", serverId);
            }

            if (!client.IsConnected)
            {
                 _logger.LogWarning("Client for server {ServerId} is not connected. Attempting to reconnect.", serverId);
                 // TODO: Add reconnection logic based on stored config if desired, or rely on higher-level retry for the operation.
                 // For now, we assume ConnectAsync was called and it might be a transient issue.
                 // Or, throw indicating it's disconnected.
                 throw new OpcCommunicationException($"Connection to server {serverId} is not active.", serverId);
            }

            if (client is T typedClient)
            {
                return typedClient;
            }
            throw new OpcCommunicationException($"Mismatched client type for server: {serverId}. Expected {typeof(T).Name}, got {client.GetType().Name}.", serverId);
        }


        public async Task<UaBrowseNodeDto[]> BrowseNamespaceAsync(string serverId, string nodeId)
        {
            var client = await GetClientAsync<IOpcUaClient>(serverId); // Browsing is typically UA, DA might need separate handling
             if (_serverStandards.TryGetValue(serverId, out var standard))
            {
                if (standard == OpcStandard.Ua)
                {
                    var uaClient = await GetClientAsync<IOpcUaClient>(serverId);
                    return await uaClient.BrowseAsync(nodeId);
                }
                else if (standard == OpcStandard.Da)
                {
                     var daClient = await GetClientAsync<IOpcDaClient>(serverId);
                     // DA Browsing is different, it might return itemIds. This needs to map to UaBrowseNodeDto if possible or have a different DTO.
                     // For simplicity, assuming DA browse returns string itemIds.
                     var itemIds = await daClient.BrowseAsync(nodeId); // NodeId here might be parent item id
                     return itemIds.Select(id => new UaBrowseNodeDto(id, id, "Variable", false)).ToArray(); // Simplified mapping
                }
                // Add other standards if they support browsing
            }
            throw new OpcCommunicationException($"Browsing not supported or standard mismatch for server: {serverId}", serverId);
        }

        public async Task<ReadResponseDto> ReadTagsAsync(string serverId, ReadRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (_serverStandards.TryGetValue(serverId, out var standard))
            {
                try
                {
                    List<OpcPointDto> points;
                    switch (standard)
                    {
                        case OpcStandard.Da:
                            var daClient = await GetClientAsync<IOpcDaClient>(serverId);
                            points = await daClient.ReadAsync(request.TagIds); // Assuming ReadRequestDto.TagIds are DA ItemIDs
                            break;
                        case OpcStandard.Ua:
                            var uaClient = await GetClientAsync<IOpcUaClient>(serverId);
                            points = await uaClient.ReadAsync(request.TagIds); // Assuming ReadRequestDto.TagIds are UA NodeIds
                            break;
                        case OpcStandard.XmlDa:
                            var xmlDaClient = await GetClientAsync<IOpcXmlDaClient>(serverId);
                            points = await xmlDaClient.ReadAsync(request.TagIds);
                            break;
                        default:
                            throw new OpcCommunicationException($"Read operation not supported for OPC standard: {standard}", serverId);
                    }
                    return new ReadResponseDto(serverId, points, true, "200", null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading tags from server {ServerId}", serverId);
                    return new ReadResponseDto(serverId, new List<OpcPointDto>(), false, "500", ex.Message);
                }
            }
            throw new OpcCommunicationException($"Server not found or standard not identified: {serverId}", serverId);
        }

        public async Task<WriteResponseDto> WriteTagsAsync(string serverId, WriteRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validate data (using IClientSideDataValidator)
            // For simplicity, assuming single tag write in WriteRequestDto
            // var validationResult = _validator.Validate(new OpcPointDto(request.TagId, request.Value, request.Timestamp, "Unknown"), GetValidationRulesForTag(request.TagId));
            // if (!validationResult.IsValid)
            // {
            //     _logger.LogWarning("Write validation failed for tag {TagId} on server {ServerId}: {Reason}", request.TagId, serverId, validationResult.ErrorMessage);
            //     return new WriteResponseDto(request.TagId, false, "ValidationFailed", validationResult.ErrorMessage);
            // }
            // Note: IClientSideDataValidator's signature was changed in prompt to Validate(OpcPointDto, List<ValidationRule>)
            // This service should probably get rules from ConfigurationManagementService and pass them.

            // 2. Check write limits (using IWriteOperationLimiter)
            if (!_writeLimiter.IsWriteAllowed(request, out var denialReason))
            {
                _logger.LogWarning("Write operation denied for tag {TagId} on server {ServerId}: {Reason}", request.TagId, serverId, denialReason);
                return new WriteResponseDto(request.TagId, false, "WriteLimitExceeded", denialReason);
            }

            // 3. Log critical write (using ICriticalWriteLogger)
            // Assuming some configuration determines if a write is "critical"
            var criticalWriteLog = new CriticalWriteLogDto(
                "ClientId_Placeholder", // Should come from config
                DateTime.UtcNow,
                request.TagId,
                null, // Old value - might need a read-before-write or be optional
                request.Value,
                request.InitiatingUser,
                request.Context,
                false, // Placeholder, will be updated after write
                null, null); 
            // _criticalLogger.LogCriticalWrite(criticalWriteLog); // Log before attempt or after success? SDS implies before.

            if (_serverStandards.TryGetValue(serverId, out var standard))
            {
                try
                {
                    Dictionary<string, object> valuesToWrite = new() { { request.TagId, request.Value } };
                    List<WriteResponseDto> responses;
                    WriteResponseDto singleResponse;

                    switch (standard)
                    {
                        case OpcStandard.Da:
                            var daClient = await GetClientAsync<IOpcDaClient>(serverId);
                            responses = await daClient.WriteAsync(valuesToWrite);
                            singleResponse = responses.FirstOrDefault();
                            break;
                        case OpcStandard.Ua:
                            var uaClient = await GetClientAsync<IOpcUaClient>(serverId);
                            responses = await uaClient.WriteAsync(valuesToWrite);
                            singleResponse = responses.FirstOrDefault();
                            break;
                        case OpcStandard.XmlDa:
                            var xmlDaClient = await GetClientAsync<IOpcXmlDaClient>(serverId);
                            responses = await xmlDaClient.WriteAsync(valuesToWrite);
                            singleResponse = responses.FirstOrDefault();
                            break;
                        default:
                            throw new OpcCommunicationException($"Write operation not supported for OPC standard: {standard}", serverId);
                    }
                    
                    if(singleResponse != null && singleResponse.Success)
                    {
                        _writeLimiter.RecordSuccessfulWrite(request);
                        criticalWriteLog = criticalWriteLog with { Success = true, StatusCode = singleResponse.StatusCode };
                    }
                    else if (singleResponse != null)
                    {
                         criticalWriteLog = criticalWriteLog with { Success = false, StatusCode = singleResponse.StatusCode, ErrorMessage = singleResponse.ErrorMessage };
                    }
                    _criticalLogger.LogCriticalWrite(criticalWriteLog); // Log actual result

                    return singleResponse ?? new WriteResponseDto(request.TagId, false, "WriteFailed", "No response from client implementation.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing tag {TagId} to server {ServerId}", request.TagId, serverId);
                    criticalWriteLog = criticalWriteLog with { Success = false, StatusCode = "Exception", ErrorMessage = ex.Message };
                    _criticalLogger.LogCriticalWrite(criticalWriteLog);
                    return new WriteResponseDto(request.TagId, false, "Exception", ex.Message);
                }
            }
            throw new OpcCommunicationException($"Server not found or standard not identified: {serverId}", serverId);
        }

        public async Task<string> CreateSubscriptionAsync(string serverId, UaSubscriptionConfigDto config)
        {
            var client = await GetClientAsync<IOpcUaClient>(serverId);
            return await client.CreateSubscriptionAsync(config);
        }

        public async Task RemoveSubscriptionAsync(string subscriptionId)
        {
            // This requires knowing which server the subscriptionId belongs to.
            // The UaSubscriptionManager or IOpcUaClient should handle this.
            // For now, assuming subscriptionId is unique across all UA clients or client handles it.
            foreach (var conn in _connections.Values)
            {
                if (conn is IOpcUaClient uaClient)
                {
                    try
                    {
                        // IOpcUaClient should have a method like RemoveSubscription(subscriptionId)
                        // await uaClient.RemoveSubscriptionAsync(subscriptionId);
                        // For now, this is a placeholder
                        _logger.LogInformation("Attempting to remove subscription {SubscriptionId}", subscriptionId);
                        // If IOpcUaClient.RemoveSubscriptionAsync doesn't exist, this method needs rethinking
                        // It might be that UaSubscriptionManager handles this, and OpcCommunicationService calls into it.
                        // The current IOpcUaClient interface has RemoveSubscriptionAsync
                        bool removed = await uaClient.RemoveSubscriptionAsync(subscriptionId);
                        if (removed) {
                             _logger.LogInformation("Subscription {SubscriptionId} removed request sent to client for server {ServerId}", subscriptionId, uaClient.ServerId);
                             return;
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove subscription {SubscriptionId} from a client. It might belong to another client or already removed.", subscriptionId);
                    }
                }
            }
            _logger.LogWarning("Subscription {SubscriptionId} not found or not removable through any active UA client.", subscriptionId);
            // throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");
        }

        public async Task<HdaReadResponseDto> QueryHistoricalDataAsync(string serverId, HdaReadRequestDto request)
        {
             var client = await GetClientAsync<IOpcHdaClient>(serverId);
             try
             {
                // Assuming HdaReadRequestDto.TagIds are HDA ItemIDs
                if (request.DataRetrievalMode == "Raw")
                {
                    return await client.ReadRawAsync(request.TagIds, request.StartTime, request.EndTime, true); // includeBounds typically true
                }
                else if (request.DataRetrievalMode == "Processed")
                {
                    return await client.ReadProcessedAsync(request.TagIds, request.StartTime, request.EndTime, request.AggregationType, request.ResampleIntervalMs);
                }
                else
                {
                    throw new ArgumentException($"Invalid DataRetrievalMode: {request.DataRetrievalMode}");
                }
             }
             catch (Exception ex)
             {
                _logger.LogError(ex, "Error querying historical data from server {ServerId}", serverId);
                return new HdaReadResponseDto(serverId, new List<OpcPointDto>(), false, "500", ex.Message);
             }
        }

        public async Task<bool> AcknowledgeAlarmAsync(string serverId, AcAcknowledgeRequestDto request)
        {
            var client = await GetClientAsync<IOpcAcClient>(serverId);
            try
            {
                return await client.AcknowledgeEventAsync(request.EventId, request.User, request.Comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alarm on server {ServerId}", serverId);
                return false;
            }
        }
    }
}