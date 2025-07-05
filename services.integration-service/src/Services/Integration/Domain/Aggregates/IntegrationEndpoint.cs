using Services.Integration.Domain.ValueObjects;

namespace Services.Integration.Domain.Aggregates;

// Placeholder for a complex value object representing an endpoint address.
public record EndpointAddress(string Uri, Dictionary<string, string> Properties);

// Placeholder for an entity representing a data mapping rule.
public record DataMappingRule(string SourcePath, string TargetPath, string TransformationLogic);


/// <summary>
/// Represents a single configured external system. It is the aggregate root for the integration context.
/// Encapsulates all configuration, data mapping rules, and state for one integration point.
/// </summary>
public class IntegrationEndpoint
{
    private readonly List<DataMappingRule> _dataMappingRules = new();

    /// <summary>
    /// Gets the unique identifier for the integration endpoint.
    /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    /// Gets the human-readable name of the endpoint.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type of the endpoint, determining which connector/adapter to use.
    /// </summary>
    public EndpointType EndpointType { get; private init; }

    /// <summary>
    /// Gets the address and connection properties for the external system.
    /// </summary>
    public EndpointAddress Address { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the endpoint is currently active.
    /// </summary>
    public bool IsEnabled { get; private set; }
    
    /// <summary>
    /// Gets the collection of data mapping rules for this endpoint.
    /// </summary>
    public IReadOnlyCollection<DataMappingRule> DataMappingRules => _dataMappingRules.AsReadOnly();

    /// <summary>
    /// Private constructor to enforce creation via the factory method.
    /// </summary>
    private IntegrationEndpoint() { }

    /// <summary>
    /// Factory method to create a new IntegrationEndpoint, ensuring initial validation.
    /// </summary>
    /// <param name="name">The name for the new endpoint.</param>
    /// <param name="type">The type of the new endpoint.</param>
    /// <param name="address">The address and connection details for the new endpoint.</param>
    /// <returns>A new, valid <see cref="IntegrationEndpoint"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if name or address are null.</exception>
    /// <exception cref="ArgumentException">Thrown if name is empty or whitespace.</exception>
    public static IntegrationEndpoint Create(string name, EndpointType type, EndpointAddress address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Endpoint name cannot be null or whitespace.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(address, nameof(address));
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        return new IntegrationEndpoint
        {
            Id = Guid.NewGuid(),
            Name = name,
            EndpointType = type,
            Address = address,
            IsEnabled = true // Endpoints are enabled by default on creation.
        };
    }

    /// <summary>
    /// Updates the configuration of the endpoint.
    /// </summary>
    /// <param name="name">The new name for the endpoint.</param>
    /// <param name="address">The new address details for the endpoint.</param>
    /// <exception cref="ArgumentException">Thrown if the new name is invalid.</exception>
    public void UpdateConfiguration(string name, EndpointAddress address)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Endpoint name cannot be null or whitespace.", nameof(name));
        }
        ArgumentNullException.ThrowIfNull(address, nameof(address));

        Name = name;
        Address = address;
    }

    /// <summary>
    /// Adds a data mapping rule to the endpoint.
    /// </summary>
    /// <param name="rule">The data mapping rule to add.</param>
    public void AddDataMappingRule(DataMappingRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        if (!_dataMappingRules.Contains(rule))
        {
            _dataMappingRules.Add(rule);
        }
    }

    /// <summary>
    /// Enables the endpoint, allowing it to process data.
    /// </summary>
    public void Enable()
    {
        if (!IsEnabled)
        {
            IsEnabled = true;
            // In a full implementation, this would raise a domain event, e.g., IntegrationEndpointEnabledEvent
        }
    }

    /// <summary>
    /// Disables the endpoint, preventing it from processing data.
    /// </summary>
    public void Disable()
    {
        if (IsEnabled)
        {
            IsEnabled = false;
            // In a full implementation, this would raise a domain event, e.g., IntegrationEndpointDisabledEvent
        }
    }
}