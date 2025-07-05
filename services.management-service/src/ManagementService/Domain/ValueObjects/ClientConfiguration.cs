namespace ManagementService.Domain.ValueObjects;

/// <summary>
/// A value object representing a single tag's configuration. Immutable.
/// </summary>
public record TagConfig(string TagName, int ScanRate);

/// <summary>
/// A value object representing the configuration settings for an OPC client instance.
/// This is an immutable object.
/// </summary>
public record ClientConfiguration(int PollingIntervalSeconds, IReadOnlyList<TagConfig> TagConfigurations)
{
    /// <summary>
    /// Provides a default, initial configuration for new clients.
    /// </summary>
    public static ClientConfiguration Default => new(60, new List<TagConfig>().AsReadOnly());
}