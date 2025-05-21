using IntegrationService.Adapters.DigitalTwin.Models;
using System;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    /// <summary>
    /// Interface for Digital Twin adaptors, defining contract for data synchronization.
    /// Abstracts communication with Digital Twin platforms. Defines methods for sending real-time data updates
    /// to the twin, receiving setpoints/commands from the twin, querying twin properties,
    /// and handling model version awareness and secure communication.
    /// </summary>
    public interface IDigitalTwinAdaptor
    {
        /// <summary>
        /// Gets the unique identifier for this adaptor instance.
        /// Corresponds to the DigitalTwinConfig.Id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Establishes a connection to the Digital Twin platform.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the Digital Twin platform.
        /// </summary
        Task DisconnectAsync();

        /// <summary>
        /// Updates a digital twin's properties or sends telemetry to it.
        /// </summary>
        /// <param name="twinId">The unique identifier of the digital twin to update.</param>
        /// <param name="updateRequest">The request containing the update payload and type.</param>
        Task UpdateTwinAsync(string twinId, DigitalTwinUpdateRequest updateRequest);

        /// <summary>
        /// Retrieves information about the model of a specific digital twin.
        /// This helps in checking model version compatibility.
        /// </summary>
        /// <param name="twinId">The unique identifier of the digital twin.</param>
        /// <returns>Information about the twin's model.</returns>
        Task<DigitalTwinModelInfo> GetTwinModelInfoAsync(string twinId);

        /// <summary>
        /// Subscribes to changes or commands from a specific digital twin.
        /// Bi-directional communication must be enabled.
        /// </summary>
        /// <param name="twinId">The unique identifier of the digital twin to subscribe to.</param>
        /// <param name="onTwinChangeReceived">Callback invoked when a change or command is received from the twin.</param>
        Task SubscribeToTwinChangesAsync(string twinId, Action<object> onTwinChangeReceived);
        
        /// <summary>
        /// Gets a value indicating whether the adaptor is currently connected to the platform.
        /// </summary>
        bool IsConnected { get; }
    }
}