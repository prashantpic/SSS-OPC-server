using OPC.Client.Core.Application.DTOs; // For OpcTagValueDto, AlarmEventDto etc.
using OPC.Client.Core.Infrastructure.ServerConnectivity.Grpc; // For IServerRpcClient
using OPC.Client.Core.Infrastructure.ServerConnectivity.Messaging; // For IServerMessageBusPublisher
using OPC.Client.Core.Infrastructure.ServerConnectivity.Models; // For DTOs specific to server comms
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPC.Client.Core.Infrastructure.ServerConnectivity
{
    /// <summary>
    /// Facade orchestrating communication (gRPC and RabbitMQ) with the backend server application
    /// for configuration, data synchronization, and commands.
    /// </summary>
    /// <remarks>
    /// Manages and coordinates both synchronous gRPC calls to, and asynchronous event publishing
    /// via RabbitMQ to, the server-side application, abstracting the underlying gRPC client
    /// and message publisher implementations.
    /// Implements REQ-SAP-003.
    /// </remarks>
    public class ClientServerCommFacade
    {
        private readonly IServerRpcClient _serverRpcClient;
        private readonly IServerMessageBusPublisher _messageBusPublisher;
        private readonly ILogger<ClientServerCommFacade> _logger;
        private readonly string _clientInstanceId; // To identify this client instance to the server

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientServerCommFacade"/> class.
        /// </summary>
        /// <param name="serverRpcClient">The gRPC client for synchronous communication.</param>
        /// <param name="messageBusPublisher">The message bus publisher for asynchronous communication.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="clientInstanceId">A unique identifier for this client instance.</param>
        public ClientServerCommFacade(
            IServerRpcClient serverRpcClient,
            IServerMessageBusPublisher messageBusPublisher,
            ILogger<ClientServerCommFacade> logger,
            string clientInstanceId)
        {
            _serverRpcClient = serverRpcClient ?? throw new ArgumentNullException(nameof(serverRpcClient));
            _messageBusPublisher = messageBusPublisher ?? throw new ArgumentNullException(nameof(messageBusPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clientInstanceId = clientInstanceId ?? throw new ArgumentNullException(nameof(clientInstanceId));
        }

        /// <summary>
        /// Requests the client's configuration from the backend server using gRPC.
        /// </summary>
        /// <returns>The server configuration DTO, or null if retrieval fails.</returns>
        public async Task<ServerConfigurationDto?> RequestConfigurationAsync()
        {
            _logger.LogInformation("Requesting configuration from server for client ID {ClientId}...", _clientInstanceId);
            try
            {
                var config = await _serverRpcClient.GetConfigurationAsync(_clientInstanceId);
                _logger.LogInformation("Configuration received successfully from server for client ID {ClientId}.", _clientInstanceId);
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request configuration from server for client ID {ClientId}.", _clientInstanceId);
                return null;
            }
        }

        /// <summary>
        /// Sends the client's health status to the backend server using gRPC.
        /// </summary>
        /// <param name="status">The client health status DTO.</param>
        public async Task SendHealthStatusAsync(ClientHealthStatusDto status)
        {
            _logger.LogDebug("Sending health status to server for client ID {ClientId}...", _clientInstanceId);
            try
            {
                status.ClientId = _clientInstanceId; // Ensure client ID is set
                await _serverRpcClient.SendHealthStatusAsync(status);
                _logger.LogDebug("Health status sent successfully to server for client ID {ClientId}.", _clientInstanceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send health status to server for client ID {ClientId}.", _clientInstanceId);
            }
        }

        /// <summary>
        /// Publishes a batch of real-time OPC tag data to the backend server via RabbitMQ.
        /// </summary>
        /// <param name="dataBatch">A list of OPC tag value DTOs.</param>
        public async Task PublishRealtimeDataBatchAsync(List<OpcTagValueDto> dataBatch)
        {
            if (dataBatch == null || !dataBatch.Any())
            {
                _logger.LogTrace("No real-time data to publish.");
                return;
            }

            _logger.LogDebug("Publishing {Count} real-time data items to message bus...", dataBatch.Count);
            try
            {
                var message = new PublishedEventMessage
                {
                    EventType = "RealtimeDataBatch",
                    SourceClientId = _clientInstanceId,
                    Timestamp = DateTime.UtcNow,
                    Payload = dataBatch // The DTO itself is the payload
                };
                // Assuming topic/routing key is handled by IServerMessageBusPublisher implementation or config
                await _messageBusPublisher.PublishAsync("opc.data.realtime", message);
                _logger.LogDebug("Real-time data batch published successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish real-time data batch.");
                // Consider buffering/retry logic here or in the publisher implementation
            }
        }

        /// <summary>
        /// Publishes an OPC alarm or event to the backend server via RabbitMQ.
        /// </summary>
        /// <param name="alarmEvent">The alarm event DTO.</param>
        public async Task PublishAlarmEventAsync(AlarmEventDto alarmEvent)
        {
            _logger.LogDebug("Publishing alarm event (ID: {EventId}) to message bus...", alarmEvent.EventId);
            try
            {
                 var message = new PublishedEventMessage
                {
                    EventType = "AlarmEvent",
                    SourceClientId = _clientInstanceId,
                    Timestamp = DateTime.UtcNow, // Or use alarmEvent.Timestamp if more appropriate
                    Payload = alarmEvent
                };
                await _messageBusPublisher.PublishAsync("opc.events.alarms", message);
                _logger.LogDebug("Alarm event published successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish alarm event (ID: {EventId}).", alarmEvent.EventId);
            }
        }

        /// <summary>
        /// Publishes a critical audit log entry to the backend server via RabbitMQ.
        /// </summary>
        /// <param name="logEntry">The audit log entry DTO.</param>
        public async Task PublishCriticalAuditLogAsync(AuditLogEntryDto logEntry)
        {
            _logger.LogInformation("Publishing critical audit log (Type: {EventType}) to message bus...", logEntry.EventType);
            try
            {
                 var message = new PublishedEventMessage
                {
                    EventType = "CriticalAuditLog",
                    SourceClientId = _clientInstanceId,
                    Timestamp = logEntry.Timestamp, // Use timestamp from log entry
                    Payload = logEntry
                };
                await _messageBusPublisher.PublishAsync("opc.logs.audit.critical", message);
                _logger.LogInformation("Critical audit log published successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish critical audit log (Type: {EventType}).", logEntry.EventType);
            }
        }
    }
}