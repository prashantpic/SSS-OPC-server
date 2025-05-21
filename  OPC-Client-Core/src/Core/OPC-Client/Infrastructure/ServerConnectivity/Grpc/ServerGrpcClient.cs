namespace OPC.Client.Core.Infrastructure.ServerConnectivity.Grpc
{
    using global::Grpc.Net.Client;
    using Microsoft.Extensions.Logging;
    using OPC.Client.Core.Application; // For ClientConfiguration DTO
    using OPC.Client.Core.Exceptions;
    using OPC.Client.Core.Infrastructure.ServerConnectivity.Grpc.Protos; // Generated Protobuf classes
    using System;
    using System.Threading.Tasks;
    using Google.Protobuf.WellKnownTypes; // For Timestamp

    /// <summary>
    /// Implements IServerRpcClient using generated gRPC client stubs to communicate
    /// with the server-side application.
    /// Handles gRPC channel management and request/response serialization.
    /// Implements REQ-SAP-003.
    /// </summary>
    public class ServerGrpcClient : IServerRpcClient
    {
        private readonly ILogger<ServerGrpcClient> _logger;
        private GrpcChannel? _channel;
        private ConfigurationService.ConfigurationServiceClient? _configClient;
        private HealthService.HealthServiceClient? _healthClient;
        private string _serverEndpoint = string.Empty;

        public ServerGrpcClient(ILogger<ServerGrpcClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Configures the gRPC client with the server endpoint.
        /// This method must be called before any RPC calls can be made.
        /// </summary>
        /// <param name="serverEndpoint">The gRPC server endpoint URL.</param>
        public void Configure(string serverEndpoint)
        {
            if (string.IsNullOrWhiteSpace(serverEndpoint))
            {
                _logger.LogError("gRPC server endpoint cannot be null or empty.");
                throw new ArgumentNullException(nameof(serverEndpoint));
            }

            if (_serverEndpoint == serverEndpoint && _channel != null && _configClient != null && _healthClient != null)
            {
                _logger.LogDebug("gRPC client already configured for endpoint: {Endpoint}", serverEndpoint);
                return;
            }

            _serverEndpoint = serverEndpoint;
            _logger.LogInformation("Configuring gRPC client for server endpoint: {Endpoint}", _serverEndpoint);

            try
            {
                // Dispose existing channel if reconfiguring
                _channel?.Dispose();

                var grpcChannelOptions = new GrpcChannelOptions
                {
                    // Configure additional options if needed (e.g., credentials, load balancing)
                    // MaxReceiveMessageSize = ...
                    // MaxSendMessageSize = ...
                };

                _channel = GrpcChannel.ForAddress(_serverEndpoint, grpcChannelOptions);
                _configClient = new ConfigurationService.ConfigurationServiceClient(_channel);
                _healthClient = new HealthService.HealthServiceClient(_channel);
                _logger.LogInformation("gRPC channel and clients created successfully for {Endpoint}", _serverEndpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create gRPC channel or clients for endpoint {Endpoint}", _serverEndpoint);
                _channel = null; // Ensure channel is null on failure
                _configClient = null;
                _healthClient = null;
                throw new ServerConnectivityException($"Failed to initialize gRPC client for endpoint {_serverEndpoint}", ex);
            }
        }

        private void CheckClientInitialized()
        {
            if (_channel == null || _configClient == null || _healthClient == null)
            {
                _logger.LogError("gRPC client is not initialized. Call Configure() first.");
                throw new InvalidOperationException("gRPC client is not initialized. Call Configure() with a valid server endpoint.");
            }
        }

        /// <summary>
        /// Requests the client's configuration from the server.
        /// </summary>
        /// <param name="clientInstanceId">The unique ID of this client instance.</param>
        /// <returns>The client configuration DTO.</returns>
        /// <exception cref="ServerConnectivityException">If communication with the server fails.</exception>
        public async Task<ClientConfigurationResponse> RequestConfigurationAsync(string clientInstanceId)
        {
            CheckClientInitialized();
            _logger.LogDebug("Requesting client configuration for instance ID: {ClientInstanceId}", clientInstanceId);

            try
            {
                var request = new ClientIdentityRequest { ClientInstanceId = clientInstanceId };
                var response = await _configClient!.GetClientConfigurationAsync(request);
                _logger.LogInformation("Client configuration received successfully for instance ID: {ClientInstanceId}", clientInstanceId);
                return response; // This is the Protobuf generated class
            }
            catch (Grpc.Core.RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error requesting client configuration: {StatusCode} - {Detail}", rpcEx.StatusCode, rpcEx.Status.Detail);
                throw new ServerConnectivityException($"gRPC error requesting configuration: {rpcEx.Status.Detail}", rpcEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error requesting client configuration.");
                throw new ServerConnectivityException("Unexpected error requesting configuration.", ex);
            }
        }

        /// <summary>
        /// Sends a health status update to the server.
        /// </summary>
        /// <param name="healthUpdate">The health status update DTO.</param>
        /// <returns>The server's response to the health update.</returns>
        /// <exception cref="ServerConnectivityException">If communication with the server fails.</exception>
        public async Task<HealthStatusResponse> SendHealthStatusAsync(ClientHealthStatusUpdate healthUpdate)
        {
            CheckClientInitialized();
            _logger.LogTrace("Sending health status update for client ID: {ClientInstanceId}", healthUpdate.ClientInstanceId);

            try
            {
                var response = await _healthClient!.SendHealthStatusAsync(healthUpdate);
                _logger.LogTrace("Health status update acknowledged by server: {Acknowledged}", response.Acknowledged);
                return response;
            }
            catch (Grpc.Core.RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "gRPC error sending health status: {StatusCode} - {Detail}", rpcEx.StatusCode, rpcEx.Status.Detail);
                throw new ServerConnectivityException($"gRPC error sending health status: {rpcEx.Status.Detail}", rpcEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending health status.");
                throw new ServerConnectivityException("Unexpected error sending health status.", ex);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing ServerGrpcClient and gRPC channel.");
            _channel?.Dispose();
            _channel = null;
            _configClient = null;
            _healthClient = null;
        }
    }

    /// <summary>
    /// Interface for synchronous RPC communication with the server-side application.
    /// To be implemented by <see cref="ServerGrpcClient"/>.
    /// </summary>
    public interface IServerRpcClient : IDisposable
    {
        void Configure(string serverEndpoint);
        Task<ClientConfigurationResponse> RequestConfigurationAsync(string clientInstanceId);
        Task<HealthStatusResponse> SendHealthStatusAsync(ClientHealthStatusUpdate healthUpdate);
    }
}