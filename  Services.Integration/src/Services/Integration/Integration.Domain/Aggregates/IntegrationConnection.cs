using System;
using System.Text.Json;
using Opc.System.Services.Integration.Domain.Enums;

namespace Opc.System.Services.Integration.Domain.Aggregates;

/// <summary>
/// Represents a configured connection to a single external system. This is an aggregate root.
/// </summary>
public class IntegrationConnection
{
    /// <summary>
    /// Unique identifier for the integration connection.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// A user-friendly name for the connection.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The type of external system this connection points to.
    /// </summary>
    public ConnectionType ConnectionType { get; private set; }

    /// <summary>
    /// The network endpoint of the external system (e.g., MQTT broker hostname, RPC endpoint URL).
    /// </summary>
    public string Endpoint { get; private set; }

    /// <summary>
    /// Flag indicating if the connection is currently active and should be used.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Stores security-sensitive configuration like credentials, keys, and certificates as a JSON document.
    /// This document should be encrypted before being persisted to the database.
    /// </summary>
    public JsonDocument SecurityConfiguration { get; private set; }

    /// <summary>
    /// Optional foreign key to a DataMap aggregate, used for data transformation.
    /// </summary>
    public Guid? DataMapId { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private IntegrationConnection()
    {
        Name = string.Empty;
        Endpoint = string.Empty;
        SecurityConfiguration = JsonDocument.Parse("{}");
    }

    /// <summary>
    /// Creates a new instance of an integration connection.
    /// </summary>
    /// <param name="id">The unique ID.</param>
    /// <param name="name">The connection name.</param>
    /// <param name="connectionType">The type of connection.</param>
    /// <param name="endpoint">The connection endpoint URL or address.</param>
    /// <param name="securityConfiguration">The security configuration.</param>
    public IntegrationConnection(Guid id, string name, ConnectionType connectionType, string endpoint, JsonDocument securityConfiguration)
    {
        if (id == Guid.Empty) throw new ArgumentException("Connection ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Connection name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("Connection endpoint cannot be empty.", nameof(endpoint));
        
        Id = id;
        Name = name;
        ConnectionType = connectionType;
        Endpoint = endpoint;
        SecurityConfiguration = securityConfiguration ?? throw new ArgumentNullException(nameof(securityConfiguration));
        IsEnabled = false; // Disabled by default
    }

    /// <summary>
    /// Enables the connection, allowing it to be used for data exchange.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// Disables the connection, preventing any data exchange.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Updates the core configuration of the connection.
    /// </summary>
    /// <param name="name">The new name for the connection.</param>
    /// <param name="endpoint">The new endpoint for the connection.</param>
    /// <param name="securityConfig">The new security configuration.</param>
    public void UpdateConfiguration(string name, string endpoint, JsonDocument securityConfig)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Connection name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("Connection endpoint cannot be empty.", nameof(endpoint));

        Name = name;
        Endpoint = endpoint;
        SecurityConfiguration = securityConfig ?? throw new ArgumentNullException(nameof(securityConfig));
    }

    /// <summary>
    /// Assigns a data map to this connection for data transformation purposes.
    /// </summary>
    /// <param name="dataMapId">The unique identifier of the DataMap.</param>
    public void AssignDataMap(Guid dataMapId)
    {
        if (dataMapId == Guid.Empty) throw new ArgumentException("DataMap ID cannot be empty.", nameof(dataMapId));
        DataMapId = dataMapId;
    }
    
    /// <summary>
    /// Removes the data map assignment from this connection.
    /// </summary>
    public void UnassignDataMap()
    {
        DataMapId = null;
    }
}