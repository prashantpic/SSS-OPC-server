using Opc.Client.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Opc.Client.Core.Domain.Aggregates;

/// <summary>
/// The ServerConnection aggregate root. It models an OPC server connection, its configuration, and its lifecycle.
/// </summary>
/// <remarks>
/// Represents a connection to a single OPC Server. This aggregate root manages its own state 
/// (e.g., Connected, Disconnected), configuration, and holds collections of associated tags and subscriptions.
/// </remarks>
public class ServerConnection
{
    private readonly Dictionary<string, Tag> _tags = new();
    private readonly Dictionary<Guid, Subscription> _subscriptions = new();

    /// <summary>
    /// Unique identifier for the server connection.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Configuration details for the OPC server endpoint.
    /// </summary>
    public ServerConfiguration Configuration { get; }

    /// <summary>
    /// Current status of the connection.
    /// </summary>
    public ServerStatus Status { get; private set; }

    /// <summary>
    /// A read-only view of the tags associated with this server connection.
    /// </summary>
    public IReadOnlyDictionary<string, Tag> Tags => new ReadOnlyDictionary<string, Tag>(_tags);

    /// <summary>
    /// A read-only view of the subscriptions associated with this server connection.
    /// </summary>
    public IReadOnlyDictionary<Guid, Subscription> Subscriptions => new ReadOnlyDictionary<Guid, Subscription>(_subscriptions);

    public ServerConnection(Guid id, ServerConfiguration configuration)
    {
        Id = id;
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Status = ServerStatus.Disconnected;
    }

    /// <summary>
    /// Initiates the connection process.
    /// </summary>
    /// <remarks>This method should trigger application logic to perform the actual connection.</remarks>
    public void Connect()
    {
        if (Status is ServerStatus.Connected or ServerStatus.Connecting) return;
        
        UpdateStatus(ServerStatus.Connecting, "Connection process initiated.");
        // Domain Event for "Connecting" would be raised here by a domain event dispatcher
    }

    /// <summary>
    /// Initiates the disconnection process.
    /// </summary>
    /// <remarks>This method should trigger application logic to perform the actual disconnection.</remarks>
    public void Disconnect()
    {
        if (Status is ServerStatus.Disconnected or ServerStatus.Disconnecting) return;

        UpdateStatus(ServerStatus.Disconnecting, "Disconnection process initiated.");
        // Domain Event for "Disconnecting" would be raised here
    }

    /// <summary>
    /// Adds a subscription to the server connection.
    /// </summary>
    /// <param name="subscription">The subscription to add.</param>
    public void AddSubscription(Subscription subscription)
    {
        if (!_subscriptions.ContainsKey(subscription.Id))
        {
            _subscriptions.Add(subscription.Id, subscription);
        }
    }
    
    /// <summary>
    /// Updates the connection status and raises a domain event.
    /// </summary>
    /// <param name="newStatus">The new status of the server connection.</param>
    /// <param name="reason">The reason for the status change.</param>
    public void UpdateStatus(ServerStatus newStatus, string reason)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;

        // Domain Event for "StatusChanged" would be raised here, e.g.:
        // AddDomainEvent(new ServerStatusChangedEvent(this.Id, oldStatus, newStatus, reason));
    }
}

// --- Placeholder types for compilation ---

/// <summary>
/// Represents the configuration for connecting to an OPC server.
/// </summary>
public record ServerConfiguration(string EndpointUrl, string SecurityPolicy, string UserName, string Password);

/// <summary>
/// Represents the connection status of an OPC server.
/// </summary>
public enum ServerStatus
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting,
    Faulted
}

/// <summary>
/// Represents a configured tag on an OPC server.
/// </summary>
public record Tag(string NodeId, string Name);

/// <summary>
/// Represents a subscription to data changes on an OPC server.
/// </summary>
public record Subscription(Guid Id, double PublishingInterval);