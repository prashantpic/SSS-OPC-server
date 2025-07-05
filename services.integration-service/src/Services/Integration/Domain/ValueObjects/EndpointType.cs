using System.Reflection;

namespace Services.Integration.Domain.ValueObjects;

/// <summary>
/// Represents a strongly-typed classification of integration endpoint types.
/// This implementation uses the smart enum pattern.
/// </summary>
public abstract class EndpointType
{
    /// <summary>
    /// Gets the name of the endpoint type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the unique identifier for the endpoint type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Represents an Azure IoT Hub integration.
    /// </summary>
    public static readonly EndpointType AzureIot = new AzureIotType();

    /// <summary>
    /// Represents an AWS IoT Core integration.
    /// </summary>
    public static readonly EndpointType AwsIot = new AwsIotType();

    /// <summary>
    /// Represents an Augmented Reality data streaming endpoint.
    /// </summary>
    public static readonly EndpointType AugmentedReality = new AugmentedRealityType();

    /// <summary>
    /// Represents a Blockchain network integration.
    /// </summary>
    public static readonly EndpointType Blockchain = new BlockchainType();

    /// <summary>
    /// Represents a Digital Twin integration.
    /// </summary>
    public static readonly EndpointType DigitalTwin = new DigitalTwinType();

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointType"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The descriptive name.</param>
    protected EndpointType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Lists all available endpoint types.
    /// </summary>
    /// <returns>An enumerable of all defined endpoint types.</returns>
    public static IEnumerable<EndpointType> List() =>
        typeof(EndpointType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<EndpointType>();

    /// <summary>
    /// Gets an endpoint type from its name.
    /// </summary>
    /// <param name="name">The name of the endpoint type.</param>
    /// <returns>The matching EndpointType.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no matching type is found.</exception>
    public static EndpointType FromName(string name)
    {
        var state = List().SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (state == null)
        {
            throw new InvalidOperationException($"'{name}' is not a valid endpoint type name.");
        }
        return state;
    }

    /// <summary>
    /// Gets an endpoint type from its ID.
    /// </summary>
    /// <param name="id">The ID of the endpoint type.</param>
    /// <returns>The matching EndpointType.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no matching type is found.</exception>
    public static EndpointType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);
        if (state == null)
        {
            throw new InvalidOperationException($"'{id}' is not a valid endpoint type ID.");
        }
        return state;
    }
    
    private sealed class AzureIotType : EndpointType { public AzureIotType() : base(1, "AzureIot") { } }
    private sealed class AwsIotType : EndpointType { public AwsIotType() : base(2, "AwsIot") { } }
    private sealed class AugmentedRealityType : EndpointType { public AugmentedRealityType() : base(3, "AugmentedReality") { } }
    private sealed class BlockchainType : EndpointType { public BlockchainType() : base(4, "Blockchain") { } }
    private sealed class DigitalTwinType : EndpointType { public DigitalTwinType() : base(5, "DigitalTwin") { } }
}