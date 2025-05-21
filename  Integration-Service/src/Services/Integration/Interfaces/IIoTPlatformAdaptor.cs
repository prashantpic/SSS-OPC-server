using IntegrationService.Adapters.IoT.Models;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    /// <summary>
    /// Interface for IoT Platform adaptors, defining contract for sending/receiving data.
    /// Abstracts the protocol-specific communication details with various IoT platforms.
    /// Defines methods for connecting, disconnecting, sending telemetry/data, receiving commands, 
    /// and subscribing to topics, ensuring bi-directional data flow where applicable.
    /// </summary>
    public interface IIoTPlatformAdaptor
    {
        /// <summary>
        /// Gets the unique identifier for this adaptor instance.
        /// Corresponds to the IoTPlatformConfig.Id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Establishes a connection to the IoT platform.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the IoT platform.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Sends telemetry data to the IoT platform.
        /// </summary>
        /// <param name="message">The telemetry message to send.</param>
        Task SendTelemetryAsync(IoTDataMessage message);

        /// <summary>
        /// Subscribes to topics or endpoints for receiving commands from the IoT platform.
        /// Bi-directional communication must be enabled.
        /// </summary>
        /// <param name="onCommandReceived">Callback invoked when a command is received.</param>
        Task SubscribeToCommandsAsync(Action<IoTCommand> onCommandReceived);

        /// <summary>
        /// Sends a response back to the IoT platform for a received command.
        /// Applicable if the platform supports command responses and bi-directional flow is enabled.
        /// </summary>
        /// <param name="commandId">The identifier of the command being responded to (e.g., CorrelationId).</param>
        /// <param name="responsePayload">The payload of the response.</param>
        Task SendCommandResponseAsync(string commandId, object responsePayload);

        /// <summary>
        /// Gets a value indicating whether the adaptor is currently connected to the platform.
        /// </summary>
        bool IsConnected { get; }
    }
}