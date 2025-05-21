using OPC.Client.Core.Application.DTOs;
using OPC.Client.Core.Domain.Aggregates;
using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Domain.DomainServices;
using OPC.Client.Core.Exceptions;
using OPC.Client.Core.Infrastructure.Protocols.Common;
using OPC.Client.Core.Infrastructure.Protocols.Ua;
using OPC.Client.Core.Infrastructure.Protocols.Hda;
using OPC.Client.Core.Infrastructure.Protocols.Ac;
using OPC.Client.Core.Infrastructure.ServerConnectivity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPC.Client.Core.Application
{
    // IOpcClientServiceApi is assumed to be defined in this namespace or a referenced one.
    // Minimal placeholder for compilation if IOpcClientServiceApi.cs is not generated in this pass:
    /*
    public interface IOpcClientServiceApi
    {
        Task<Guid> ConnectAsync(ClientConfiguration serverConfig);
        Task DisconnectAsync(Guid connectionId);
        Task<List<NodeAddress>> BrowseNamespaceAsync(Guid connectionId, NodeAddress? startNodeId = null);
        Task<List<OpcTagValueDto>> ReadTagsAsync(Guid connectionId, List<NodeAddress> tagAddresses);
        Task WriteTagsAsync(Guid connectionId, List<OpcTagValueDto> tagValues);
        Task<Guid> CreateUaSubscriptionAsync(Guid connectionId, UaSubscriptionConfigDto subscriptionConfig);
        // ... other methods from SDS 5.1 ...
    }
    */

    /// <summary>
    /// Main entry point and facade for the OPC Client Core library, implementing IOpcClientServiceApi
    /// and orchestrating application and domain logic.
    /// </summary>
    /// <remarks>
    /// Coordinates calls to protocol handlers, server communication components, and domain services.
    /// Implements REQ-CSVC-001, REQ-SAP-003, REQ-3-001, REQ-CSVC-023.
    /// </remarks>
    public class OpcCoreClientFacade : IOpcClientServiceApi
    {
        private readonly ProtocolClientFactory _protocolClientFactory;
        private readonly ClientServerCommFacade _clientServerCommFacade;
        private readonly TagConfigurationImporterService? _tagConfigurationImporterService; // Optional service
        private readonly DataValidationService _dataValidationService;
        private readonly CriticalWriteAuditor _criticalWriteAuditor;
        private readonly WriteOperationLimiterService _writeOperationLimiterService;
        private readonly ILogger<OpcCoreClientFacade> _logger;

        private readonly ConcurrentDictionary<Guid, OpcServerConnection> _connections = new ConcurrentDictionary<Guid, OpcServerConnection>();
        private readonly ConcurrentDictionary<Guid, OpcUaSubscription> _uaSubscriptions = new ConcurrentDictionary<Guid, OpcUaSubscription>(); // For managing domain subscription aggregates

        public event EventHandler<OpcDataChangedEvent>? OpcDataChanged;
        public event EventHandler<AlarmEventDto>? AlarmEventReceived; // Using DTO here for application event
        public event EventHandler<CommunicationStatusEvent>? CommunicationStatusChanged; // Define CommunicationStatusEvent as needed

        public OpcCoreClientFacade(
            ProtocolClientFactory protocolClientFactory,
            ClientServerCommFacade clientServerCommFacade,
            DataValidationService dataValidationService,
            CriticalWriteAuditor criticalWriteAuditor,
            WriteOperationLimiterService writeOperationLimiterService,
            ILogger<OpcCoreClientFacade> logger,
            TagConfigurationImporterService? tagConfigurationImporterService = null)
        {
            _protocolClientFactory = protocolClientFactory ?? throw new ArgumentNullException(nameof(protocolClientFactory));
            _clientServerCommFacade = clientServerCommFacade ?? throw new ArgumentNullException(nameof(clientServerCommFacade));
            _dataValidationService = dataValidationService ?? throw new ArgumentNullException(nameof(dataValidationService));
            _criticalWriteAuditor = criticalWriteAuditor ?? throw new ArgumentNullException(nameof(criticalWriteAuditor));
            _writeOperationLimiterService = writeOperationLimiterService ?? throw new ArgumentNullException(nameof(writeOperationLimiterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tagConfigurationImporterService = tagConfigurationImporterService;
        }

        public async Task<Guid> ConnectAsync(ClientConfiguration serverConfig)
        {
            var connectionId = Guid.NewGuid();
            _logger.LogInformation("Attempting to connect with new ID {ConnectionId} to OPC server: {EndpointUrl}, Protocol: {Protocol}",
                connectionId, serverConfig.ServerEndpoint, serverConfig.ProtocolType);

            try
            {
                var protocolClient = _protocolClientFactory.CreateClient(serverConfig);
                var connection = new OpcServerConnection(connectionId, serverConfig, protocolClient);

                // Subscribe to protocol client events before connecting
                // protocolClient.StatusChanged += (s, e) => HandleConnectionStatusChanged(connectionId, e);
                // protocolClient.OpcDataChanged += (s, e) => HandleSubscriptionDataChanged(connectionId, e); // If protocol client raises this directly
                // protocolClient.AlarmEventReceived += (s, e) => HandleAlarmEventReceived(connectionId, e);

                await connection.ConnectAsync(); // Connects the protocol client

                if (!_connections.TryAdd(connectionId, connection))
                {
                    _logger.LogError("Failed to add connection {ConnectionId} to dictionary. Disconnecting.", connectionId);
                    await connection.DisconnectAsync(); // Attempt to clean up
                    throw new InvalidOperationException("Failed to register new connection.");
                }

                _logger.LogInformation("Successfully established connection {ConnectionId}", connectionId);
                OnCommunicationStatusChanged(new CommunicationStatusEvent(connectionId, "Connection", true, "Connected successfully"));
                return connectionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to OPC server {EndpointUrl} with ID {ConnectionId}", serverConfig.ServerEndpoint, connectionId);
                OnCommunicationStatusChanged(new CommunicationStatusEvent(connectionId, "Connection", false, $"Connection failed: {ex.Message}"));
                throw; // Re-throw to be handled by caller
            }
        }

        public async Task DisconnectAsync(Guid connectionId)
        {
            _logger.LogInformation("Attempting to disconnect connection {ConnectionId}", connectionId);
            if (_connections.TryRemove(connectionId, out var connection))
            {
                try
                {
                    // Remove associated UA subscriptions first
                    var subsToRemove = _uaSubscriptions.Where(kvp => kvp.Value.ConnectionId == connectionId).Select(kvp => kvp.Key).ToList();
                    foreach (var subId in subsToRemove)
                    {
                        await DeleteSubscriptionAsync(connectionId, subId);
                    }
                    await connection.DisconnectAsync();
                    _logger.LogInformation("Successfully disconnected {ConnectionId}", connectionId);
                    OnCommunicationStatusChanged(new CommunicationStatusEvent(connectionId, "Connection", false, "Disconnected successfully"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during disconnection of {ConnectionId}", connectionId);
                    OnCommunicationStatusChanged(new CommunicationStatusEvent(connectionId, "Connection", false, $"Disconnection error: {ex.Message}"));
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("Attempted to disconnect non-existent connection ID: {ConnectionId}", connectionId);
            }
        }

        public async Task<List<NodeAddress>> BrowseNamespaceAsync(Guid connectionId, NodeAddress? startNodeId = null)
        {
            var connection = GetConnection(connectionId);
            _logger.LogDebug("Browsing namespace for connection {ConnectionId}, StartNode: {StartNode}", connectionId, startNodeId?.Identifier ?? "Root");
            var nodes = await connection.BrowseNamespaceAsync(startNodeId);
            return nodes.ToList();
        }

        public async Task<List<OpcTagValueDto>> ReadTagsAsync(Guid connectionId, List<NodeAddress> tagAddresses)
        {
            var connection = GetConnection(connectionId);
            _logger.LogDebug("Reading {TagCount} tags for connection {ConnectionId}", tagAddresses.Count, connectionId);
            var values = await connection.ReadTagsAsync(tagAddresses);
            // DTO mapping happens here or in ProtocolClient
            return values.Select(v => new OpcTagValueDto { NodeAddress = v.NodeAddress.Identifier, Value = v.Value, Timestamp = v.Timestamp, Quality = v.Quality.ToString() }).ToList();
        }

        public async Task WriteTagsAsync(Guid connectionId, List<OpcTagValueDto> tagValues)
        {
            var connection = GetConnection(connectionId);
            _logger.LogDebug("Attempting to write {TagCount} tags for connection {ConnectionId}", tagValues.Count, connectionId);

            var validValues = new List<OpcDataValue>();
            foreach (var dto in tagValues)
            {
                // Convert DTO to Domain Value Object
                var nodeAddr = new NodeAddress(dto.NodeAddress, null); // Assuming NodeAddress from DTO is string ID
                var opcVal = new OpcDataValue(nodeAddr, dto.Value, Quality.Good, dto.Timestamp); // Assuming Quality mapping

                if (!_dataValidationService.ValidateWriteValue(opcVal.NodeAddress, opcVal.Value)) // Implement ValidateWriteValue
                {
                    _logger.LogWarning("Write validation failed for tag {TagAddress} on connection {ConnectionId}", dto.NodeAddress, connectionId);
                    // Potentially throw or skip
                    continue;
                }
                validValues.Add(opcVal);
            }

            if (!validValues.Any())
            {
                _logger.LogInformation("No valid values to write after validation for connection {ConnectionId}", connectionId);
                return;
            }

            // Apply write limiting (simplified)
            if (!_writeOperationLimiterService.IsWriteAllowed(connectionId, validValues)) // Implement IsWriteAllowed
            {
                _logger.LogWarning("Write operation blocked by rate limit/threshold for connection {ConnectionId}", connectionId);
                throw new WriteLimitExceededException($"Write operation for connection {connectionId} blocked by limits.");
            }

            await connection.WriteTagsAsync(validValues); // ProtocolClient handles this

            // Auditing
            _criticalWriteAuditor.LogCriticalWrite(connectionId, validValues); // Implement LogCriticalWrite

            _logger.LogInformation("Successfully wrote {TagCount} tags for connection {ConnectionId}", validValues.Count, connectionId);
        }

        public async Task<Guid> CreateUaSubscriptionAsync(Guid connectionId, UaSubscriptionConfigDto subscriptionConfig)
        {
            var connection = GetConnection(connectionId);
            if (connection.Config.ProtocolType != OpcProtocolType.UA)
                throw new ProtocolNotSupportedException("Subscriptions are only supported for OPC UA protocol.");

            var uaCommunicator = connection.ProtocolClient as OpcUaCommunicator ??
                                 throw new InvalidOperationException("Internal error: UA connection does not have OpcUaCommunicator.");

            var subscriptionId = Guid.NewGuid();
            _logger.LogInformation("Creating OPC UA subscription {SubscriptionId} for connection {ConnectionId}", subscriptionId, connectionId);

            // Simplified: directly calling UA communicator's subscription management
            // A more domain-centric approach would involve OpcUaSubscription aggregate
            var domainSub = await uaCommunicator.UaSubscriptionHandler.CreateSubscriptionAsync(subscriptionId.ToString(), connectionId.ToString(), subscriptionConfig);
            _uaSubscriptions.TryAdd(subscriptionId, domainSub); // Store our domain aggregate

            _logger.LogInformation("OPC UA subscription {SubscriptionId} created.", subscriptionId);
            OnCommunicationStatusChanged(new CommunicationStatusEvent(subscriptionId, "Subscription", true, "Created successfully"));
            return subscriptionId;
        }

        public async Task AddMonitoredItemsToSubscriptionAsync(Guid connectionId, Guid subscriptionId, List<MonitoredItemConfigDto> monitoredItemConfigs)
        {
            var connection = GetConnection(connectionId);
            if (connection.Config.ProtocolType != OpcProtocolType.UA)
                throw new ProtocolNotSupportedException("Subscriptions are only supported for OPC UA protocol.");
            
            var uaCommunicator = connection.ProtocolClient as OpcUaCommunicator ??
                                 throw new InvalidOperationException("Internal error: UA connection does not have OpcUaCommunicator.");

            if (!_uaSubscriptions.TryGetValue(subscriptionId, out var domainSub))
                throw new ArgumentException($"Subscription with ID {subscriptionId} not found.");

            _logger.LogInformation("Adding {ItemCount} monitored items to subscription {SubscriptionId}", monitoredItemConfigs.Count, subscriptionId);
            await uaCommunicator.UaSubscriptionHandler.AddMonitoredItemsAsync(domainSub, monitoredItemConfigs);
            _logger.LogInformation("Monitored items added to subscription {SubscriptionId}", subscriptionId);
        }

        public async Task RemoveMonitoredItemsFromSubscriptionAsync(Guid connectionId, Guid subscriptionId, List<NodeAddress> tagAddresses)
        {
            var connection = GetConnection(connectionId);
            if (connection.Config.ProtocolType != OpcProtocolType.UA)
                throw new ProtocolNotSupportedException("Subscriptions are only supported for OPC UA protocol.");

            var uaCommunicator = connection.ProtocolClient as OpcUaCommunicator ??
                                 throw new InvalidOperationException("Internal error: UA connection does not have OpcUaCommunicator.");
            
            if (!_uaSubscriptions.TryGetValue(subscriptionId, out var domainSub))
                throw new ArgumentException($"Subscription with ID {subscriptionId} not found.");

            _logger.LogInformation("Removing {ItemCount} monitored items from subscription {SubscriptionId}", tagAddresses.Count, subscriptionId);
            await uaCommunicator.UaSubscriptionHandler.RemoveMonitoredItemsAsync(domainSub, tagAddresses);
            _logger.LogInformation("Monitored items removed from subscription {SubscriptionId}", subscriptionId);
        }

        public async Task DeleteSubscriptionAsync(Guid connectionId, Guid subscriptionId)
        {
            var connection = GetConnection(connectionId); // Ensure connection exists for context, even if sub is managed globally
             if (connection.Config.ProtocolType != OpcProtocolType.UA)
                throw new ProtocolNotSupportedException("Subscriptions are only supported for OPC UA protocol.");

            var uaCommunicator = connection.ProtocolClient as OpcUaCommunicator ??
                                 throw new InvalidOperationException("Internal error: UA connection does not have OpcUaCommunicator.");

            _logger.LogInformation("Deleting OPC UA subscription {SubscriptionId}", subscriptionId);
            await uaCommunicator.UaSubscriptionHandler.RemoveSubscriptionAsync(subscriptionId.ToString());
            _uaSubscriptions.TryRemove(subscriptionId, out _);
            _logger.LogInformation("OPC UA subscription {SubscriptionId} deleted.", subscriptionId);
            OnCommunicationStatusChanged(new CommunicationStatusEvent(subscriptionId, "Subscription", false, "Deleted successfully"));
        }

        public async Task<List<OpcTagValueDto>> QueryHistoricalDataAsync(Guid connectionId, HistoricalDataQueryDto queryParams)
        {
            var connection = GetConnection(connectionId);
             if (connection.Config.ProtocolType != OpcProtocolType.HDA)
                throw new ProtocolNotSupportedException("Historical data queries are only supported for OPC HDA protocol.");

            var hdaCommunicator = connection.ProtocolClient as OpcHdaCommunicator ??
                                  throw new InvalidOperationException("Internal error: HDA connection does not have OpcHdaCommunicator.");
            
            _logger.LogDebug("Querying historical data for connection {ConnectionId}", connectionId);
            var values = await hdaCommunicator.QueryHistoricalDataAsync(queryParams); // QueryHistoricalData needs to be on HDACommunicator
            return values.Select(v => new OpcTagValueDto { NodeAddress = v.NodeAddress.Identifier, Value = v.Value, Timestamp = v.Timestamp, Quality = v.Quality.ToString() }).ToList();
        }

        public async Task AcknowledgeAlarmAsync(Guid connectionId, AlarmAcknowledgementDto acknowledgementDetails)
        {
            var connection = GetConnection(connectionId);
            if (connection.Config.ProtocolType != OpcProtocolType.AC)
                throw new ProtocolNotSupportedException("Alarm acknowledgement is only supported for OPC A&C protocol.");

            var acCommunicator = connection.ProtocolClient as OpcAcCommunicator ??
                                  throw new InvalidOperationException("Internal error: A&C connection does not have OpcAcCommunicator.");
            
            _logger.LogInformation("Acknowledging alarm on connection {ConnectionId}, EventId: {EventId}", connectionId, acknowledgementDetails.EventId);
            await acCommunicator.AcknowledgeAlarmAsync(acknowledgementDetails); // AcknowledgeAlarm needs to be on AcCommunicator
        }

        public async Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(Guid connectionId, Guid subscriptionId)
        {
            GetConnection(connectionId); // Validate connection exists
            if (!_uaSubscriptions.TryGetValue(subscriptionId, out var domainSub))
                throw new ArgumentException($"Subscription with ID {subscriptionId} not found.");
            
            // Example status retrieval from domain subscription object
            return await Task.FromResult(new SubscriptionStatusDto
            {
                SubscriptionId = subscriptionId,
                IsConnected = domainSub.State == OpcUaSubscription.SubscriptionState.Active, // Simplified
                // LastKeepAliveTime = domainSub.LastKeepAliveReceived,
                // CurrentSequenceNumber = domainSub.SequenceNumber,
                // BufferedNotificationCount = _dataBuffer.GetCount(subscriptionId.ToString()) // Assuming ISubscriptionDataBuffer has GetCount
            });
        }

        private OpcServerConnection GetConnection(Guid connectionId)
        {
            if (!_connections.TryGetValue(connectionId, out var connection))
            {
                _logger.LogError("Connection ID {ConnectionId} not found.", connectionId);
                throw new ArgumentException($"Connection ID {connectionId} not found.");
            }
            return connection;
        }

        // Methods to raise events
        protected virtual void OnOpcDataChanged(OpcDataChangedEvent e) => OpcDataChanged?.Invoke(this, e);
        protected virtual void OnAlarmEventReceived(AlarmEventDto e) => AlarmEventReceived?.Invoke(this, e);
        protected virtual void OnCommunicationStatusChanged(CommunicationStatusEvent e) => CommunicationStatusChanged?.Invoke(this, e);

        // Example handler if events are directly routed from IOpcProtocolClient
        private void HandleConnectionStatusChanged(Guid connectionId, CommunicationStatusEvent statusEvent)
        {
            _logger.LogInformation("Connection status changed for {ConnectionId}: IsConnected={IsConnected}, Message='{Message}'",
                connectionId, statusEvent.IsConnected, statusEvent.Message);
            CommunicationStatusChanged?.Invoke(this, statusEvent);
        }
    }

    // Placeholder DTOs/Events if not defined elsewhere in this generation pass
    // In a full system, these would be in their own files in DTOs/Events namespaces.
    // public record OpcDataChangedEvent(Guid ConnectionId, Guid SubscriptionId, List<OpcDataValue> DataValues);
    // public record AlarmEventDto(/* ... properties ... */);
    public record CommunicationStatusEvent(Guid SourceId, string SourceType, bool IsConnected, string Message);
}