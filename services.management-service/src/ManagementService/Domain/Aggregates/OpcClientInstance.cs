using ManagementService.Domain.ValueObjects;

namespace ManagementService.Domain.Aggregates;

/// <summary>
/// Represents a single managed OPC client. It is the consistency boundary for all operations related to a client.
/// This is the Aggregate Root for the OpcClientInstance aggregate.
/// </summary>
public class OpcClientInstance
{
    /// <summary>
    /// The unique identifier for the client instance.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// A user-friendly name for the client instance.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The physical or logical location of the client.
    /// </summary>
    public string Site { get; private set; }

    /// <summary>
    /// Timestamp of the last health report received from the client.
    /// </summary>
    public DateTimeOffset LastSeen { get; private set; }

    /// <summary>
    /// The last reported health status (value object).
    /// </summary>
    public HealthStatus HealthStatus { get; private set; }

    /// <summary>
    /// The current configuration for the client (value object).
    /// </summary>
    public ClientConfiguration Configuration { get; private set; }

    // Private constructor for ORM/repository mapping and for the factory method.
    private OpcClientInstance(Guid id, string name, string site)
    {
        Id = id;
        Name = name;
        Site = site;
        Configuration = ClientConfiguration.Default;
        HealthStatus = HealthStatus.Initial;
        LastSeen = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new, valid OPC Client Instance.
    /// </summary>
    /// <param name="id">The unique identifier for the new client.</param>
    /// <param name="name">The name of the client.</param>
    /// <param name="site">The site where the client is located.</param>
    /// <returns>A new instance of OpcClientInstance.</returns>
    public static OpcClientInstance Register(Guid id, string name, string site)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Client ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Client name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(site))
            throw new ArgumentException("Client site cannot be empty.", nameof(site));

        var instance = new OpcClientInstance(id, name, site);
        // In a real implementation, a "ClientRegistered" domain event would be raised here.
        return instance;
    }

    /// <summary>
    /// Applies a new configuration to the client instance.
    /// </summary>
    /// <param name="newConfiguration">The new configuration to apply.</param>
    public void UpdateConfiguration(ClientConfiguration newConfiguration)
    {
        if (newConfiguration is null)
            throw new ArgumentNullException(nameof(newConfiguration));
        
        // Add any validation logic for the configuration here.
        // For example:
        if (newConfiguration.PollingIntervalSeconds <= 0)
            throw new ArgumentException("Polling interval must be greater than zero.", nameof(newConfiguration.PollingIntervalSeconds));

        Configuration = newConfiguration;
        
        // In a real implementation, a "ClientConfigurationUpdated" domain event would be raised here.
    }

    /// <summary>
    /// Updates the client's health status and LastSeen timestamp based on a new report.
    /// </summary>
    /// <param name="newStatus">The new health status reported by the client.</param>
    /// <param name="reportTime">The timestamp when the report was generated.</param>
    public void ReportHealth(HealthStatus newStatus, DateTimeOffset reportTime)
    {
        if (newStatus is null)
            throw new ArgumentNullException(nameof(newStatus));
        if (reportTime > DateTimeOffset.UtcNow.AddMinutes(5)) // Some clock skew tolerance
            throw new ArgumentException("Report time cannot be in the future.", nameof(reportTime));

        HealthStatus = newStatus;
        LastSeen = reportTime;

        // In a real implementation, a "ClientHealthReported" domain event would be raised here.
    }
    
    // Parameterless constructor for deserialization frameworks
    private OpcClientInstance() { }
}