using Services.Integration.Domain.Aggregates;
using Services.Integration.Domain.ValueObjects;

namespace Services.Integration.Application.Interfaces;

/// <summary>
/// Defines the contract for sending data to various IoT platforms.
/// This interface supports the Strategy pattern for selecting the correct connector at runtime.
/// </summary>
public interface IIotPlatformConnector
{
    /// <summary>
    /// Determines whether this connector implementation supports the given endpoint type.
    /// </summary>
    /// <param name="type">The type of the integration endpoint.</param>
    /// <returns><c>true</c> if this connector can handle the endpoint type; otherwise, <c>false</c>.</returns>
    bool Supports(EndpointType type);

    /// <summary>
    /// Asynchronously sends a data payload to the specified IoT platform endpoint.
    /// </summary>
    /// <param name="endpoint">The configured integration endpoint, containing connection details.</param>
    /// <param name="payload">The JSON payload to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the data was sent successfully; otherwise, <c>false</c>.</returns>
    Task<bool> SendDataAsync(IntegrationEndpoint endpoint, string payload, CancellationToken cancellationToken);
}