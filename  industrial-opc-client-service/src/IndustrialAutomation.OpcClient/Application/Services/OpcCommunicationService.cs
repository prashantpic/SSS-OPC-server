using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda;
using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using IndustrialAutomation.OpcClient.Domain.Enums;
using IndustrialAutomation.OpcClient.Domain.Models;
using IndustrialAutomation.OpcClient.Domain.Exceptions;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ua;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class OpcCommunicationService : IOpcCommunicationService
    {
        private readonly ILogger<OpcCommunicationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IOpcConnection> _activeConnections;
        private readonly UaSubscriptionManager _uaSubscriptionManager; // Assuming UaSubscriptionManager is directly used or wrapped

        public OpcCommunicationService(
            ILogger<OpcCommunicationService> logger,
            IServiceProvider serviceProvider,
            UaSubscriptionManager uaSubscriptionManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _uaSubscriptionManager = uaSubscriptionManager ?? throw new ArgumentNullException(nameof(uaSubscriptionManager));
            _activeConnections = new ConcurrentDictionary<string, IOpcConnection>();
        }

        public async Task<bool> ConnectAsync(ServerConnectionConfigDto config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(config.ServerId)) throw new ArgumentException("ServerId cannot be null or empty.", nameof(config.ServerId));

            _logger.LogInformation("Attempting to connect to OPC server: {ServerId} ({Standard}) at {EndpointUrl}", config.ServerId, config.Standard, config.EndpointUrl);

            try
            {
                IOpcConnection client = GetOpcClient(config.Standard);
                if (client == null)
                {
                    _logger.LogError("Unsupported OPC standard: {Standard} for server: {ServerId}", config.Standard, config.ServerId);
                    return false;
                }

                bool connected = await client.ConnectAsync(config);
                if (connected)
                {
                    _activeConnections.TryAdd(config.ServerId, client);
                    _logger.LogInformation("Successfully connected to OPC server: {ServerId}", config.ServerId);
                }
                else
                {
                    _logger.LogWarning("Failed to connect to OPC server: {ServerId}", config.ServerId);
                }
                return connected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to OPC server {ServerId}: {ErrorMessage}", config.ServerId, ex.Message);
                throw new OpcCommunicationException($"Failed to connect to server {config.ServerId}", config.ServerId, ex.Message, ex);
            }
        }

        public async Task DisconnectAsync(string serverId)
        {
            if (string.IsNullOrEmpty(serverId)) throw new ArgumentException("ServerId cannot be null or empty.", nameof(serverId));

            if (_activeConnections.TryRemove(serverId, out IOpcConnection? client) && client != null)
            {
                try
                {
                    await client.DisconnectAsync();
                    _logger.LogInformation("Successfully disconnected from OPC server: {ServerId}", serverId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disconnecting from OPC server {ServerId}: {ErrorMessage}", serverId, ex.Message);
                    // Potentially re-throw or handle depending on desired behavior
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
                _logger.LogWarning("No active connection found for server ID: {ServerId} to disconnect.", serverId);
            }
        }

        public async Task<List<UaBrowseNodeDto>> BrowseNamespaceAsync(string serverId, string? nodeId = null)
        {
            var client = GetActiveClient<IOpcUaClient>(serverId, OpcStandard.Ua); // Browsing usually richer for UA
            // Or could be IOpcDaClient if browsing DA
            if (client is IOpcUaClient uaClient)
            {
                return await uaClient.BrowseAsync(nodeId);
            }
            // else if (client is IOpcDaClient daClient) { /* ... DA browse logic ... */ }

            _logger.LogWarning("BrowseNamespaceAsync called for non-UA or unsupported client for server: {ServerId}", serverId);
            return new List<UaBrowseNodeDto>();
        }

        public async Task<ReadResponseDto> ReadTagsAsync(string serverId, ReadRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            IOpcConnection client = GetActiveClient(serverId);

            try
            {
                List<OpcPointDto> values = new List<OpcPointDto>();
                bool success = true;
                string errorMessage = string.Empty;
                string statusCode = "Good";

                if (client is IOpcDaClient daClient)
                {
                    var daResult = await daClient.ReadAsync(request.TagIds);
                    // TODO: map DA result to OpcPointDto and handle errors
                    values.AddRange(daResult.Select(item => new OpcPointDto { TagId = item.TagId, Value = item.Value, Timestamp = item.Timestamp, QualityStatus = item.QualityStatus }));
                }
                else if (client is IOpcUaClient uaClient)
                {
                     var uaResult = await uaClient.ReadAsync(request.TagIds);
                    // TODO: map UA result
                    values.AddRange(uaResult.Select(item => new OpcPointDto { TagId = item.TagId, Value = item.Value, Timestamp = item.Timestamp, QualityStatus = item.QualityStatus }));
                }
                else if (client is IOpcXmlDaClient xmlDaClient)
                {
                    var xmlDaResult = await xmlDaClient.ReadAsync(request.TagIds);
                    // TODO: map XML-DA result
                    values.AddRange(xmlDaResult.Select(item => new OpcPointDto { TagId = item.TagId, Value = item.Value, Timestamp = item.Timestamp, QualityStatus = item.QualityStatus }));
                }
                else
                {
                    success = false;
                    errorMessage = "Unsupported OPC client type for read operation.";
                    statusCode = "Bad";
                    _logger.LogError("ReadTagsAsync: Unsupported client type for server {ServerId}", serverId);
                }
                return new ReadResponseDto { ServerId = serverId, Values = values, Success = success, ErrorMessage = errorMessage, StatusCode = statusCode };

            }
            catch (OpcCommunicationException opcEx)
            {
                _logger.LogError(opcEx, "OPC Communication error reading tags from server {ServerId}: {OpcStatusCode}", serverId, opcEx.OpcStatusCode);
                return new ReadResponseDto { ServerId = serverId, Values = new List<OpcPointDto>(), Success = false, ErrorMessage = opcEx.Message, StatusCode = opcEx.OpcStatusCode ?? "Bad" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading tags from server {ServerId}", serverId);
                return new ReadResponseDto { ServerId = serverId, Values = new List<OpcPointDto>(), Success = false, ErrorMessage = ex.Message, StatusCode = "Bad" };
            }
        }

        public async Task<WriteResponseDto> WriteTagsAsync(string serverId, WriteRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            IOpcConnection client = GetActiveClient(serverId);

            try
            {
                // Implement client-side validation and write limiting here if not handled by a dedicated service upstream
                // For example:
                // if (!_clientSideValidator.ValidateWrite(request, out var validationErrors)) { ... }
                // if (!_writeOperationLimiter.TryAllowWrite(request, out var denialReason)) { ... }

                bool success = false;
                string statusCode = "Uncertain";
                string errorMessage = string.Empty;

                var valuesToWrite = new Dictionary<string, object> { { request.TagId, request.Value } };


                if (client is IOpcDaClient daClient)
                {
                    var responses = await daClient.WriteAsync(valuesToWrite);
                    var response = responses.FirstOrDefault(); // Assuming single tag write for simplicity
                    success = response?.Success ?? false;
                    statusCode = response?.StatusCode ?? "Bad";
                    errorMessage = response?.ErrorMessage ?? string.Empty;
                }
                else if (client is IOpcUaClient uaClient)
                {
                    var responses = await uaClient.WriteAsync(valuesToWrite);
                    var response = responses.FirstOrDefault();
                    success = response?.Success ?? false;
                    statusCode = response?.StatusCode ?? "Bad";
                    errorMessage = response?.ErrorMessage ?? string.Empty;
                }
                else if (client is IOpcXmlDaClient xmlDaClient)
                {
                    var responses = await xmlDaClient.WriteAsync(valuesToWrite);
                    var response = responses.FirstOrDefault();
                    success = response?.Success ?? false;
                    statusCode = response?.StatusCode ?? "Bad";
                    errorMessage = response?.ErrorMessage ?? string.Empty;
                }
                else
                {
                    errorMessage = "Unsupported OPC client type for write operation.";
                    statusCode = "Bad";
                    _logger.LogError("WriteTagsAsync: Unsupported client type for server {ServerId}", serverId);
                }

                // Critical write logging if needed here or upstream
                // _criticalWriteLogger.Log(new CriticalWriteLogDto { ... });

                return new WriteResponseDto { TagId = request.TagId, Success = success, StatusCode = statusCode, ErrorMessage = errorMessage };
            }
            catch (OpcCommunicationException opcEx)
            {
                _logger.LogError(opcEx, "OPC Communication error writing tag {TagId} to server {ServerId}: {OpcStatusCode}", request.TagId, serverId, opcEx.OpcStatusCode);
                return new WriteResponseDto { TagId = request.TagId, Success = false, StatusCode = opcEx.OpcStatusCode ?? "Bad", ErrorMessage = opcEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing tag {TagId} to server {ServerId}", request.TagId, serverId);
                return new WriteResponseDto { TagId = request.TagId, Success = false, StatusCode = "Bad", ErrorMessage = ex.Message };
            }
        }


        public async Task<string> CreateSubscriptionAsync(string serverId, UaSubscriptionConfigDto config)
        {
            var uaClient = GetActiveClient<IOpcUaClient>(serverId, OpcStandard.Ua);
            if (config == null) throw new ArgumentNullException(nameof(config));

            try
            {
                string subscriptionId = await _uaSubscriptionManager.CreateSubscriptionAsync(uaClient, serverId, config);
                _logger.LogInformation("Successfully created UA subscription {SubscriptionId} on server {ServerId}", subscriptionId, serverId);
                return subscriptionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating UA subscription on server {ServerId}", serverId);
                throw new OpcCommunicationException($"Failed to create subscription on server {serverId}", serverId, ex.Message, ex);
            }
        }

        public async Task RemoveSubscriptionAsync(string subscriptionId)
        {
            // UaSubscriptionManager should handle the client interaction
            try
            {
                await _uaSubscriptionManager.RemoveSubscriptionAsync(subscriptionId);
                _logger.LogInformation("Successfully removed UA subscription {SubscriptionId}", subscriptionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing UA subscription {SubscriptionId}", subscriptionId);
                // Decide on exception propagation
            }
        }


        public async Task<HdaReadResponseDto> QueryHistoricalDataAsync(string serverId, HdaReadRequestDto request)
        {
            var hdaClient = GetActiveClient<IOpcHdaClient>(serverId, OpcStandard.Hda);
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                // Assuming HDA client directly returns HdaReadResponseDto or similar
                HdaReadResponseDto response;
                if (request.DataRetrievalMode?.Equals("Raw", StringComparison.OrdinalIgnoreCase) == true)
                {
                    response = await hdaClient.ReadRawAsync(request.TagIds, request.StartTime, request.EndTime, true); // includeBounds example
                }
                else if (request.DataRetrievalMode?.Equals("Processed", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (string.IsNullOrEmpty(request.AggregationType))
                    {
                        throw new ArgumentException("AggregationType is required for Processed HDA reads.");
                    }
                    response = await hdaClient.ReadProcessedAsync(request.TagIds, request.StartTime, request.EndTime, request.AggregationType, request.ResampleIntervalMs);
                }
                else
                {
                    throw new ArgumentException($"Unsupported HDA DataRetrievalMode: {request.DataRetrievalMode}");
                }
                
                _logger.LogInformation("Successfully queried historical data from HDA server {ServerId} for {TagCount} tags.", serverId, request.TagIds.Count);
                return response;
            }
            catch (OpcCommunicationException opcEx)
            {
                _logger.LogError(opcEx, "OPC HDA Communication error querying historical data from server {ServerId}: {OpcStatusCode}", serverId, opcEx.OpcStatusCode);
                return new HdaReadResponseDto { ServerId = serverId, HistoricalData = new List<OpcPointDto>(), Success = false, StatusCode = opcEx.OpcStatusCode ?? "Bad", ErrorMessage = opcEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying historical data from HDA server {ServerId}", serverId);
                 return new HdaReadResponseDto { ServerId = serverId, HistoricalData = new List<OpcPointDto>(), Success = false, StatusCode = "Bad", ErrorMessage = ex.Message };
            }
        }

        public async Task<bool> AcknowledgeAlarmAsync(string serverId, AcAcknowledgeRequestDto request)
        {
            var acClient = GetActiveClient<IOpcAcClient>(serverId, OpcStandard.Ac);
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                bool success = await acClient.AcknowledgeEventAsync(request.EventId, request.User, request.Comment);
                if (success)
                {
                    _logger.LogInformation("Successfully acknowledged alarm/event {EventId} on A&C server {ServerId} by user {User}", request.EventId, serverId, request.User);
                }
                else
                {
                    _logger.LogWarning("Failed to acknowledge alarm/event {EventId} on A&C server {ServerId}", request.EventId, serverId);
                }
                return success;
            }
            catch (OpcCommunicationException opcEx)
            {
                _logger.LogError(opcEx, "OPC A&C Communication error acknowledging event {EventId} on server {ServerId}: {OpcStatusCode}", request.EventId, serverId, opcEx.OpcStatusCode);
                return false; // Or rethrow
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acknowledging alarm/event {EventId} on A&C server {ServerId}", request.EventId, serverId);
                return false; // Or rethrow
            }
        }

        private IOpcConnection GetOpcClient(OpcStandard standard)
        {
            return standard switch
            {
                OpcStandard.Da => _serviceProvider.GetService(typeof(IOpcDaClient)) as IOpcDaClient ?? throw new InvalidOperationException($"IOpcDaClient not registered."),
                OpcStandard.Ua => _serviceProvider.GetService(typeof(IOpcUaClient)) as IOpcUaClient ?? throw new InvalidOperationException($"IOpcUaClient not registered."),
                OpcStandard.XmlDa => _serviceProvider.GetService(typeof(IOpcXmlDaClient)) as IOpcXmlDaClient ?? throw new InvalidOperationException($"IOpcXmlDaClient not registered."),
                OpcStandard.Hda => _serviceProvider.GetService(typeof(IOpcHdaClient)) as IOpcHdaClient ?? throw new InvalidOperationException($"IOpcHdaClient not registered."),
                OpcStandard.Ac => _serviceProvider.GetService(typeof(IOpcAcClient)) as IOpcAcClient ?? throw new InvalidOperationException($"IOpcAcClient not registered."),
                _ => throw new ArgumentOutOfRangeException(nameof(standard), $"Unsupported OPC standard: {standard}")
            };
        }

        private IOpcConnection GetActiveClient(string serverId)
        {
            if (_activeConnections.TryGetValue(serverId, out IOpcConnection? client) && client != null)
            {
                if (!client.GetStatus().IsConnected) // Assuming GetStatus().IsConnected exists
                {
                    _logger.LogWarning("Client for server {ServerId} is registered but not connected.", serverId);
                    throw new OpcCommunicationException($"Client for server {serverId} is not connected.", serverId);
                }
                return client;
            }
            _logger.LogError("No active and connected client found for server ID: {ServerId}", serverId);
            throw new OpcCommunicationException($"No active client found for server ID: {serverId}. Please connect first.", serverId);
        }

        private T GetActiveClient<T>(string serverId, OpcStandard expectedStandard) where T : class, IOpcConnection
        {
            IOpcConnection client = GetActiveClient(serverId);
            if (client is T typedClient)
            {
                // Optionally, verify standard from config if client doesn't expose it
                return typedClient;
            }
            _logger.LogError("Active client for server ID: {ServerId} is not of expected type {ExpectedType}", serverId, typeof(T).Name);
            throw new OpcCommunicationException($"Client for server {serverId} is not of expected type {typeof(T).Name}.", serverId);
        }
    }
}