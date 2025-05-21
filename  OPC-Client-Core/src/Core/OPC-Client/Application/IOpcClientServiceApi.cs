using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OPC.Client.Core.Application.DTOs;
using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Domain.Aggregates.OpcUaSubscription;
// Assuming OpcDataChangedEvent will be defined in OPC.Client.Core.Domain.Events
// For AlarmEventDto and CommunicationStatusEvent, using object as placeholder if DTOs are not defined in this iteration.
// using OPC.Client.Core.Domain.Events; // For OpcDataChangedEvent
// using OPC.Client.Core.Application.DTOs; // For AlarmEventDto
// using OPC.Client.Core.Application.Events; // For CommunicationStatusEvent

namespace OPC.Client.Core.Application
{
    /// <summary>
    /// Defines the public contract (API) of the OPC Client Core library,
    /// exposing its functionalities to consuming applications.
    /// Specifies the set of operations that the OPC Client Core library provides.
    /// REQ-CSVC-001, REQ-CSVC-002, REQ-CSVC-003, REQ-CSVC-004,
    /// REQ-CSVC-011, REQ-CSVC-017, REQ-CSVC-023.
    /// </summary>
    public interface IOpcClientServiceApi : IAsyncDisposable
    {
        /// <summary>
        /// Establishes connection to an OPC server based on the provided configuration.
        /// </summary>
        /// <param name="serverConfig">Configuration for the OPC server connection.</param>
        /// <returns>A unique identifier for the established connection.</returns>
        Task<string> ConnectAsync(ClientConfiguration serverConfig);

        /// <summary>
        /// Disconnects from a specific OPC server connection.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection to disconnect.</param>
        Task DisconnectAsync(string connectionId);

        /// <summary>
        /// Browses the address space of a connected OPC server.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection.</param>
        /// <param name="startNodeId">The address of the node to start browsing from. If null, browse from the root.</param>
        /// <returns>A list of child node addresses.</returns>
        Task<List<NodeAddress>> BrowseNamespaceAsync(string connectionId, NodeAddress? startNodeId = null);

        /// <summary>
        /// Reads current data values from specified OPC tags.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection.</param>
        /// <param name="tagAddresses">A list of OPC tag addresses to read.</param>
        /// <returns>A list of OPC tag values.</returns>
        Task<List<OpcTagValueDto>> ReadTagsAsync(string connectionId, List<NodeAddress> tagAddresses);

        /// <summary>
        /// Writes data values to specified OPC tags.
        /// </summary>
        /// <param name="connectionId">The identifier of the connection.</param>
        /// <param name="tagValues">A list of tag addresses and values to write.</param>
        Task WriteTagsAsync(string connectionId, List<OpcTagValueDto> tagValues);

        /// <summary>
        /// Creates an OPC UA subscription on a connected server.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC UA connection.</param>
        /// <param name="subscriptionConfig">Configuration parameters for the subscription.</param>
        /// <returns>A unique identifier for the created subscription.</returns>
        Task<string> CreateUaSubscriptionAsync(string connectionId, UaSubscriptionConfigDto subscriptionConfig);

        /// <summary>
        /// Adds monitored items to an existing OPC UA subscription.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC UA connection.</param>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <param name="monitoredItems">A list of items to add to the subscription.</param>
        Task AddMonitoredItemsToSubscriptionAsync(string connectionId, string subscriptionId, List<MonitoredItem> monitoredItems);

        /// <summary>
        /// Removes monitored items from an existing OPC UA subscription.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC UA connection.</param>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <param name="tagAddresses">A list of tag addresses of the items to remove.</param>
        Task RemoveMonitoredItemsFromSubscriptionAsync(string connectionId, string subscriptionId, List<NodeAddress> tagAddresses);

        /// <summary>
        /// Deletes an existing OPC UA subscription.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC UA connection.</param>
        /// <param name="subscriptionId">The identifier of the subscription to delete.</param>
        Task DeleteSubscriptionAsync(string connectionId, string subscriptionId);

        /// <summary>
        /// Retrieves historical data from an OPC HDA server.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC HDA connection.</param>
        /// <param name="queryParams">Parameters for the historical data query.</param>
        /// <returns>A list of historical OPC tag values.</returns>
        Task<List<OpcTagValueDto>> QueryHistoricalDataAsync(string connectionId, HistoricalDataQueryDto queryParams);

        /// <summary>
        /// Acknowledges an alarm on an OPC A&C server.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC A&C connection.</param>
        /// <param name="acknowledgementDetails">Details for acknowledging the alarm.</param>
        Task AcknowledgeAlarmAsync(string connectionId, AlarmAcknowledgementDto acknowledgementDetails);

        /// <summary>
        /// Gets the current status of a specific OPC UA subscription.
        /// </summary>
        /// <param name="connectionId">The identifier of the OPC UA connection.</param>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <returns>Status information for the subscription.</returns>
        Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(string connectionId, string subscriptionId);

        /// <summary>
        /// Event raised when OPC data changes are received from a subscription.
        /// The event argument type `object` is a placeholder. It should be `OPC.Client.Core.Domain.Events.OpcDataChangedEvent`
        /// once that type is defined.
        /// </summary>
        event EventHandler<object> OpcDataChanged; // Placeholder: Replace object with Domain.Events.OpcDataChangedEvent

        /// <summary>
        /// Event raised when an alarm or event is received from an OPC A&C server.
        /// The event argument type `object` is a placeholder. It should be `OPC.Client.Core.Application.DTOs.AlarmEventDto`
        /// once that DTO is defined.
        /// </summary>
        event EventHandler<object> AlarmEventReceived; // Placeholder: Replace object with Application.DTOs.AlarmEventDto

        /// <summary>
        /// Event raised when the communication status of a connection or subscription changes.
        /// The event argument type `object` is a placeholder. It should be `OPC.Client.Core.Application.Events.CommunicationStatusEvent`
        /// once that type is defined.
        /// </summary>
        event EventHandler<object> CommunicationStatusChanged; // Placeholder: Replace object with Application.Events.CommunicationStatusEvent
    }
}