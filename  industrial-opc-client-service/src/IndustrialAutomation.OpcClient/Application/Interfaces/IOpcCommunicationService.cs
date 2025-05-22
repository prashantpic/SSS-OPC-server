using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda;
using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.Interfaces
{
    /// <summary>
    /// Defines the primary contract for all OPC-related communication tasks, 
    /// abstracting specific OPC protocol details.
    /// </summary>
    public interface IOpcCommunicationService
    {
        /// <summary>
        /// Connects to an OPC server based on the provided configuration.
        /// </summary>
        /// <param name="config">The server connection configuration.</param>
        /// <returns>A task representing the asynchronous operation. True if connection successful, false otherwise.</returns>
        Task<bool> ConnectAsync(ServerConnectionConfigDto config);

        /// <summary>
        /// Disconnects from a specific OPC server.
        /// </summary>
        /// <param name="serverId">The identifier of the server to disconnect from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DisconnectAsync(string serverId);

        /// <summary>
        /// Browses the namespace of an OPC server.
        /// </summary>
        /// <param name="serverId">The identifier of the server.</param>
        /// <param name="nodeId">The node ID to browse from (null or empty for root).</param>
        /// <returns>A list of browse node DTOs.</returns>
        Task<IEnumerable<UaBrowseNodeDto>> BrowseNamespaceAsync(string serverId, string? nodeId); // Assuming UaBrowseNodeDto can be generic enough or specific for UA.

        /// <summary>
        /// Reads tag values from an OPC server.
        /// </summary>
        /// <param name="serverId">The identifier of the server.</param>
        /// <param name="request">The read request DTO.</param>
        /// <returns>A read response DTO containing the values or error information.</returns>
        Task<ReadResponseDto> ReadTagsAsync(string serverId, ReadRequestDto request);

        /// <summary>
        /// Writes tag values to an OPC server.
        /// </summary>
        /// <param name="serverId">The identifier of the server.</param>
        /// <param name="request">The write request DTO.</param>
        /// <returns>A write response DTO indicating the success or failure of the operation.</returns>
        Task<WriteResponseDto> WriteTagsAsync(string serverId, WriteRequestDto request); // SDS suggests WriteRequestDto contains a single tag. If multiple, this might need to be List<WriteRequestDto> or a batch DTO. Sticking to prompt.

        /// <summary>
        /// Creates an OPC UA subscription.
        /// </summary>
        /// <param name="serverId">The identifier of the UA server.</param>
        /// <param name="config">The subscription configuration DTO.</param>
        /// <returns>The ID of the created subscription, or null if failed.</returns>
        Task<string?> CreateSubscriptionAsync(string serverId, UaSubscriptionConfigDto config);

        /// <summary>
        /// Removes an OPC UA subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to remove.</param>
        /// <returns>A task representing the asynchronous operation. True if removal successful, false otherwise.</returns>
        Task<bool> RemoveSubscriptionAsync(string subscriptionId);

        /// <summary>
        /// Queries historical data from an OPC HDA server.
        /// </summary>
        /// <param name="serverId">The identifier of the HDA server.</param>
        /// <param name="request">The HDA read request DTO.</param>
        /// <returns>An HDA read response DTO containing historical data or error information.</returns>
        Task<HdaReadResponseDto> QueryHistoricalDataAsync(string serverId, HdaReadRequestDto request);

        /// <summary>
        /// Acknowledges an alarm on an OPC A&C server.
        /// </summary>
        /// <param name="serverId">The identifier of the A&C server.</param>
        /// <param name="request">The A&C acknowledge request DTO.</param>
        /// <returns>A task representing the asynchronous operation. True if acknowledgement successful, false otherwise.</returns>
        Task<bool> AcknowledgeAlarmAsync(string serverId, AcAcknowledgeRequestDto request);
    }
}