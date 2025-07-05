using Opc.Client.Core.Domain.Aggregates;
using Opc.Client.Core.Domain.ValueObjects;

namespace Opc.Client.Core.Application.Interfaces;

/// <summary>
/// Provides a unified contract for all OPC protocol clients, abstracting away the specific details of each standard.
/// </summary>
/// <remarks>
/// This interface decouples the application logic from the concrete OPC SDKs (e.g., OPC UA, Classic DA) 
/// by defining a common set of operations. It is a key part of the Strategy and Adapter patterns.
/// </remarks>
public interface IOpcProtocolClient
{
    /// <summary>
    /// Asynchronously connects to an OPC server using the provided configuration.
    /// </summary>
    /// <param name="config">The server connection configuration.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the final server status.</returns>
    Task<ServerStatus> ConnectAsync(ServerConfiguration config, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously disconnects from the OPC server.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously reads the current values of one or more tags (nodes).
    /// </summary>
    /// <param name="nodeIds">A collection of node identifiers to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of tag values.</returns>
    Task<IEnumerable<TagValue>> ReadAsync(IEnumerable<string> nodeIds, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously writes values to one or more tags (nodes).
    /// </summary>
    /// <param name="values">A dictionary mapping node identifiers to the values to be written.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the write operation.</returns>
    Task<WriteResult> WriteAsync(IDictionary<string, object> values, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously browses the address space of the OPC server starting from a given node.
    /// </summary>
    /// <param name="nodeId">The identifier of the node to browse from. Can be null for the root.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a collection of found nodes.</returns>
    Task<IEnumerable<NodeInfo>> BrowseAsync(string? nodeId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously creates a subscription on the OPC server.
    /// </summary>
    /// <param name="parameters">The parameters for the subscription (e.g., publishing interval, monitored items).</param>
    /// <param name="onNotification">The callback action to execute when a data change notification is received.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the unique identifier for the created subscription.</returns>
    Task<Guid> CreateSubscriptionAsync(SubscriptionParameters parameters, Action<DataChangeNotification> onNotification, CancellationToken cancellationToken);
}

// --- Placeholder types for compilation ---

/// <summary>
/// Contains the results of a write operation.
/// </summary>
/// <param name="IsSuccess">Indicates whether the overall operation was successful.</param>
/// <param name="ErrorMessage">An error message if the operation failed.</param>
/// <param name="PerNodeStatus">The status code for each individual node write.</param>
public record WriteResult(bool IsSuccess, string? ErrorMessage, IReadOnlyDictionary<string, uint>? PerNodeStatus);

/// <summary>
/// Represents information about a node discovered during a browse operation.
/// </summary>
public record NodeInfo(string NodeId, string DisplayName, string NodeClass);

/// <summary>
/// Contains parameters for creating a new subscription.
/// </summary>
public record SubscriptionParameters(double PublishingInterval, IEnumerable<string> NodeIdsToMonitor);

/// <summary>
/// Represents a notification of a data change from a subscription.
/// </summary>
public record DataChangeNotification(string NodeId, TagValue NewValue);