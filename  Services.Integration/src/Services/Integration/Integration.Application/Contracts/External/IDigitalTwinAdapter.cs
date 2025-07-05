namespace Opc.System.Services.Integration.Application.Contracts.External;

/// <summary>
/// Defines the contract for interacting with a digital twin platform.
/// This abstracts the protocol (e.g., AAS, DTDL) used for bi-directional communication.
/// </summary>
public interface IDigitalTwinAdapter
{
    /// <summary>
    /// Sends data to update a specific digital twin instance.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="twinId">The unique identifier of the target digital twin.</param>
    /// <param name="payload">The data payload, typically a JSON Patch document, to apply to the twin.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendDataAsync(Guid connectionId, string twinId, string payload);

    /// <summary>
    /// Establishes a listener for commands originating from the digital twin platform.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="onCommandReceived">A callback function to process incoming commands.</param>
    /// <returns>A task that represents the asynchronous operation of starting the listener.</returns>
    Task StartReceivingCommandsAsync(Guid connectionId, Func<string, Task> onCommandReceived);
}