using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Infrastructure.Protocols.Common;
using OPC.Client.Core.Application; // For ClientConfiguration (or specific parts of it)
using System;
using Microsoft.Extensions.Logging; // For logging within the aggregate (if necessary, usually done by services)

namespace OPC.Client.Core.Domain.Aggregates
{
    /// <summary>
    /// Aggregate root representing a connection to an OPC server.
    /// Manages its lifecycle and associated protocol client.
    /// Encapsulates connection state, endpoint, protocol type, and status.
    /// REQ-CSVC-001, REQ-3-001.
    /// </summary>
    public class OpcServerConnection
    {
        public string Id { get; }
        public ClientConfiguration Configuration { get; private set; } // Holds the specific config for this server
        public IOpcProtocolClient ProtocolClient { get; }
        public ConnectionStatus Status { get; private set; }
        public DateTime? LastStatusChangeTimestamp { get; private set; }
        public string? LastError { get; private set; }

        // Consider a logger if complex internal logic needs tracing, otherwise logging is usually in Application/Infrastructure
        // private readonly ILogger<OpcServerConnection> _logger;

        public OpcServerConnection(string id, ClientConfiguration configuration, IOpcProtocolClient protocolClient /*, ILogger<OpcServerConnection> logger*/)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            ProtocolClient = protocolClient ?? throw new ArgumentNullException(nameof(protocolClient));
            // _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Status = ConnectionStatus.Disconnected;
            LastStatusChangeTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Initiates connection to the OPC server using the configured protocol client.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting)
            {
                // _logger.LogInformation("Connection {Id} is already connected or connecting.", Id);
                return;
            }

            SetStatus(ConnectionStatus.Connecting);
            try
            {
                await ProtocolClient.ConnectAsync(Configuration);
                SetStatus(ConnectionStatus.Connected);
                // _logger.LogInformation("Connection {Id} successfully established to {Endpoint}.", Id, Configuration.ServerEndpoint);
            }
            catch (Exception ex)
            {
                SetStatus(ConnectionStatus.Error, ex.Message);
                // _logger.LogError(ex, "Failed to connect {Id} to {Endpoint}.", Id, Configuration.ServerEndpoint);
                throw; // Re-throw to be handled by the caller (e.g., Facade)
            }
        }

        /// <summary>
        /// Disconnects from the OPC server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (Status == ConnectionStatus.Disconnected || Status == ConnectionStatus.Disconnecting)
            {
                // _logger.LogInformation("Connection {Id} is already disconnected or disconnecting.", Id);
                return;
            }

            SetStatus(ConnectionStatus.Disconnecting);
            try
            {
                await ProtocolClient.DisconnectAsync();
                SetStatus(ConnectionStatus.Disconnected);
                // _logger.LogInformation("Connection {Id} successfully disconnected from {Endpoint}.", Id, Configuration.ServerEndpoint);
            }
            catch (Exception ex)
            {
                // Even if disconnect fails, set status to Disconnected or Error.
                // Error might be more appropriate if the disconnect itself failed.
                SetStatus(ConnectionStatus.Error, $"Failed to disconnect cleanly: {ex.Message}");
                // _logger.LogError(ex, "Failed to disconnect {Id} from {Endpoint}.", Id, Configuration.ServerEndpoint);
                // Decide whether to re-throw. Usually, for disconnect, we might just log.
            }
        }

        /// <summary>
        /// Updates the configuration for this server connection.
        /// Note: Reconnection might be needed after configuration update.
        /// </summary>
        /// <param name="newConfiguration">The new configuration.</param>
        public void UpdateConfiguration(ClientConfiguration newConfiguration)
        {
            Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
            // _logger.LogInformation("Configuration updated for connection {Id}.", Id);
            // Consider if status should change or if a reconnect should be triggered.
        }


        private void SetStatus(ConnectionStatus newStatus, string? errorMessage = null)
        {
            if (Status == newStatus && LastError == errorMessage) return;

            Status = newStatus;
            LastError = errorMessage;
            LastStatusChangeTimestamp = DateTime.UtcNow;

            // Raise a domain event: ConnectionStatusChangedEvent(Id, newStatus, errorMessage)
            // This event would be handled by application services or event handlers.
        }
    }

    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Error,
        Reconnecting // If automatic reconnect logic is implemented
    }
}