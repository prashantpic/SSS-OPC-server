using OPC.Client.Core.Application; // For ClientConfiguration
using OPC.Client.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPC.Client.Core.Infrastructure.Protocols.Common
{
    /// <summary>
    /// Defines a generic contract for OPC protocol client operations, abstracting specific protocol implementations.
    /// </summary>
    /// <remarks>
    /// Provides a common interface for interacting with different OPC protocol implementations (DA, UA, XML-DA, HDA, A&C),
    /// standardizing operations like connect, disconnect, read, write, and browse.
    /// Implements REQ-CSVC-001, REQ-CSVC-002, REQ-CSVC-003, REQ-CSVC-004.
    /// Note: Protocol-specific features like OPC UA Subscriptions, HDA queries, or A&C alarm acknowledgements
    /// are handled by specific communicator implementations and orchestrated by the Facade, not part of this common interface.
    /// </remarks>
    public interface IOpcProtocolClient : IDisposable
    {
        /// <summary>
        /// Connects to the OPC server using the provided configuration.
        /// </summary>
        /// <param name="config">The client configuration relevant to this protocol and specific connection.</param>
        /// <returns>A task representing the asynchronous connection operation.</returns>
        Task ConnectAsync(ClientConfiguration config);

        /// <summary>
        /// Disconnects from the OPC server.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnection operation.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Browses the address space of the OPC server, starting from a specified node.
        /// </summary>
        /// <param name="startNodeId">The <see cref="NodeAddress"/> of the node to start browsing from.
        /// If null or empty, browsing typically starts from the server's root or default browse entry point.</param>
        /// <returns>A task that resolves to a list of <see cref="NodeAddress"/> objects representing the browsed nodes.</returns>
        Task<IEnumerable<NodeAddress>> BrowseNodesAsync(NodeAddress? startNodeId);

        /// <summary>
        /// Reads the current data values (value, quality, timestamp) for a list of OPC tags.
        /// </summary>
        /// <param name="tagAddresses">A list of <see cref="NodeAddress"/> objects representing the OPC tags to read.</param>
        /// <returns>A task that resolves to a list of <see cref="OpcDataValue"/> objects containing the read data.</returns>
        Task<IEnumerable<OpcDataValue>> ReadTagsAsync(IEnumerable<NodeAddress> tagAddresses);

        /// <summary>
        /// Writes data values to a list of OPC tags.
        /// </summary>
        /// <param name="tagValues">A list of <see cref="OpcDataValue"/> objects containing the tag addresses and the values to write.</param>
        /// <returns>A task that resolves to true if all write operations were successful; otherwise, false.
        /// Specific error details for partial failures should be logged by the implementation.</returns>
        Task<bool> WriteTagsAsync(IEnumerable<OpcDataValue> tagValues);

        // Events for status changes or data could be defined here if common across protocols,
        // but typically they are more specific (e.g., OPC UA subscriptions data changes).
        // event EventHandler<ConnectionStatusEventArgs> StatusChanged;
    }
}