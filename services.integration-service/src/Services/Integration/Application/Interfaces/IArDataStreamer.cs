namespace Services.Integration.Application.Interfaces;

/// <summary>
/// Defines the contract for streaming real-time data to connected Augmented Reality (AR) clients.
/// </summary>
public interface IArDataStreamer
{
    /// <summary>
    /// Asynchronously streams data to all connected AR clients.
    /// </summary>
    /// <param name="data">The data object to be serialized and sent to clients.</param>
    /// <returns>A task that represents the asynchronous streaming operation.</returns>
    Task StreamDataToAllAsync(object data);

    /// <summary>
    /// Asynchronously streams data to a specific group of AR clients.
    /// </summary>
    /// <param name="groupName">The name of the group to target (e.g., technicians working on a specific asset).</param>
    /// <param name="data">The data object to be serialized and sent to clients in the group.</param>
    /// <returns>A task that represents the asynchronous streaming operation.</returns>
    Task StreamDataToGroupAsync(string groupName, object data);
}