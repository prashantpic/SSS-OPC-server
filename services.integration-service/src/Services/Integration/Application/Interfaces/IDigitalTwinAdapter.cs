using Services.Integration.Domain.Aggregates;

namespace Services.Integration.Application.Interfaces;

/// <summary>
/// Defines the contract for bi-directional communication with Digital Twin platforms.
/// </summary>
public interface IDigitalTwinAdapter
{
    /// <summary>
    /// Asynchronously sends data to update the state of a digital twin.
    /// </summary>
    /// <param name="endpoint">The configured integration endpoint for the target digital twin.</param>
    /// <param name="data">The data object to be sent to the twin.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendDataToTwinAsync(IntegrationEndpoint endpoint, object data);

    /// <summary>
    /// Asynchronously receives data from a digital twin.
    /// </summary>
    /// <param name="endpoint">The configured integration endpoint for the target digital twin.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the data received from the twin as an object.
    /// </returns>
    Task<object> ReceiveDataFromTwinAsync(IntegrationEndpoint endpoint);
}