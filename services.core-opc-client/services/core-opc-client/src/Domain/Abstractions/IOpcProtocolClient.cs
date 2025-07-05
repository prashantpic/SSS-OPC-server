using services.opc.client.Domain.Models;

namespace services.opc.client.Domain.Abstractions;

/// <summary>
/// Defines a common contract for all OPC protocol-specific clients (UA, DA, HDA, A&C).
/// This abstraction allows the application layer to interact with different OPC servers
/// in a protocol-agnostic manner for common operations.
/// </summary>
public interface IOpcProtocolClient : IAsyncDisposable
{
    /// <summary>
    /// Event triggered when a new data point is received from a subscription.
    /// </summary>
    event Action<DataPoint> OnDataReceived;

    /// <summary>
    /// Event triggered when a new alarm or event is received.
    /// </summary>
    event Action<AlarmEvent> OnAlarmReceived;

    /// <summary>
    /// Establishes a connection to the OPC server using the provided settings.
    /// </summary>
    /// <param name="settings">The configuration for the connection.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    Task ConnectAsync(OpcConnectionSettings settings, CancellationToken cancellationToken);

    /// <summary>
    /// Disconnects from the OPC server gracefully.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous disconnection operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Performs a synchronous read of one or more tags from the server.
    /// </summary>
    /// <param name="tagIds">A collection of tag identifiers to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that resolves to an enumerable of DataPoint objects.</returns>
    Task<IEnumerable<DataPoint>> ReadAsync(IEnumerable<string> tagIds, CancellationToken cancellationToken);
    
    /// <summary>
    /// Performs a synchronous write to a single tag on the server.
    /// </summary>
    /// <param name="tagId">The identifier of the tag to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync(string tagId, object value, CancellationToken cancellationToken);
}

// NOTE: The following domain models are defined here temporarily to ensure the project compiles.
// In a complete project structure, these would reside in their own files under 'src/Domain/Models/'.

namespace services.opc.client.Domain.Models
{
    /// <summary>
    /// Represents a single data point collected from any OPC server.
    /// </summary>
    /// <param name="TagId">Unique identifier for the tag within the client's context.</param>
    /// <param name="Value">The value of the tag.</param>
    /// <param name="SourceTimestamp">Timestamp from the originating device (OPC Server).</param>
    /// <param name="StatusCode">OPC status code (e.g., Good, Bad, Uncertain).</param>
    /// <param name="CollectionTimestamp">Timestamp (UTC ticks) when the data was collected by this service.</param>
    public record DataPoint(
        string TagId,
        object Value,
        DateTime SourceTimestamp,
        uint StatusCode,
        long CollectionTimestamp
    );

    /// <summary>
    /// Represents an alarm or event from an OPC A&C server.
    /// </summary>
    public record AlarmEvent(
        string SourceNode,
        string ConditionName,
        string Message,
        int Severity,
        DateTime OccurrenceTime,
        bool IsAcknowledged
    );
    
    /// <summary>
    /// Represents the settings for an OPC UA subscription.
    /// </summary>
    public record SubscriptionSettings(
        int PublishingInterval,
        int SamplingInterval,
        List<string> Tags
    );
    
    /// <summary>
    /// Represents the configuration for a single OPC Server connection.
    /// </summary>
    public record OpcConnectionSettings(
        string ServerId,
        string Protocol,
        string EndpointUrl,
        string? SecurityPolicy,
        string? SecurityMode,
        List<SubscriptionSettings>? Subscriptions,
        List<string>? Tags
    );
    
    /// <summary>
    /// Represents the input for an AI model inference task.
    /// </summary>
    public record ModelInput(float[] Features);

    /// <summary>
    /// Represents the output from an AI model inference task.
    /// </summary>
    public record ModelOutput(float[] Predictions);
}