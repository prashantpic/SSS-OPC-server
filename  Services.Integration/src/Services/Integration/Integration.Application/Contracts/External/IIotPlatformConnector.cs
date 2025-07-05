namespace Opc.System.Services.Integration.Application.Contracts.External;

/// <summary>
/// Defines the contract for connecting and exchanging data with any supported IoT platform.
/// This abstracts the specific communication protocol (e.g., MQTT, AMQP).
/// </summary>
public interface IIotPlatformConnector
{
    /// <summary>
    /// Sends a data payload to the specified IoT platform connection.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="payload">The string-based data payload to send.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success.</returns>
    Task<bool> SendDataAsync(Guid connectionId, string payload);

    /// <summary>
    /// Establishes a connection to receive data (e.g., commands from the cloud) and invokes a callback for each message.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="onMessageReceived">A callback function to process incoming messages.</param>
    /// <returns>A task that represents the asynchronous operation of starting the listener.</returns>
    Task StartReceivingAsync(Guid connectionId, Func<string, Task> onMessageReceived);
}