namespace Opc.System.Services.Integration.Application.Contracts.External;

/// <summary>
/// Defines the contract for streaming real-time data to connected Augmented Reality (AR) devices.
/// This abstracts the underlying real-time communication mechanism, such as WebSockets.
/// </summary>
public interface IArDataStreamer
{
    /// <summary>
    /// Streams a data payload to all currently connected AR devices.
    /// </summary>
    /// <param name="payload">The string-based data payload to stream.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StreamDataToAllAsync(string payload);

    /// <summary>
    /// Streams a data payload to a specific connected AR device.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the target device.</param>
    /// <param name="payload">The string-based data payload to stream.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task StreamDataToDeviceAsync(string deviceId, string payload);

    /// <summary>
    /// Gets the current number of connected AR devices.
    /// </summary>
    /// <returns>The total count of active connections.</returns>
    int GetConnectedDeviceCount();
}