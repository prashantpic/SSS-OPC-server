using System.Threading.Tasks;
// Assuming DTOs for request/response are defined elsewhere, e.g., in Application layer or shared contracts
// For example:
// using OPC.Client.Core.Application.DTOs.ServerCommunication;

namespace OPC.Client.Core.Infrastructure.ServerConnectivity
{
    // Define placeholder DTOs if not available from other layers yet
    public class ClientRegistrationInfo { public string ClientId { get; set; } = string.Empty; /* other details */ }
    public class ServerConfigResponse { public string ConfigurationJson { get; set; } = string.Empty; /* other config */ }
    public class ClientHealthStatusUpdate { public string ClientId { get; set; } = string.Empty; public string Status { get; set; } = string.Empty; /* other health metrics */ }
    public class RpcResponse { public bool Success { get; set; } public string? ErrorMessage { get; set; } }


    /// <summary>
    /// Defines the contract for making synchronous gRPC calls to the server-side application for request/response interactions.
    /// Abstracts synchronous gRPC communication.
    /// REQ-SAP-003
    /// </summary>
    public interface IServerRpcClient
    {
        /// <summary>
        /// Requests client configuration from the server.
        /// </summary>
        /// <param name="clientInfo">Information identifying the client making the request.</param>
        /// <returns>The server configuration response.</returns>
        Task<ServerConfigResponse> GetClientConfigurationAsync(ClientRegistrationInfo clientInfo);

        /// <summary>
        /// Sends client health status to the server.
        /// </summary>
        /// <param name="healthUpdate">The health status update from the client.</param>
        /// <returns>A response indicating if the update was received successfully.</returns>
        Task<RpcResponse> SendHealthStatusAsync(ClientHealthStatusUpdate healthUpdate);

        /// <summary>
        /// Connects the gRPC client to the server endpoint.
        /// </summary>
        Task ConnectAsync(string serverEndpoint);

        /// <summary>
        /// Disconnects the gRPC client.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Gets the connection status of the gRPC client.
        /// </summary>
        bool IsConnected { get; }
    }
}