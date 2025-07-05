using System;
using System.Text.Json;

namespace Opc.System.Services.Integration.Domain.Aggregates;

/// <summary>
/// Represents a data transformation configuration, defining how to map data
/// from a source schema to a target schema. This is an aggregate root.
/// </summary>
public class DataMap
{
    /// <summary>
    /// Unique identifier for the data map.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// A user-friendly name for the data map.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// A JSON schema or example document defining the expected structure of the source data.
    /// </summary>
    public JsonDocument SourceModelDefinition { get; private set; }

    /// <summary>
    /// A JSON schema or example document defining the structure of the target data.
    /// </summary>
    public JsonDocument TargetModelDefinition { get; private set; }

    /// <summary>
    /// A JSON document containing the rules that define the mapping logic.
    /// For example, using JSON Path-based key-value pairs.
    /// </summary>
    public JsonDocument TransformationRules { get; private set; }

    /// <summary>
    /// The version number of the data map, to track changes over time.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private DataMap()
    {
        Name = string.Empty;
        SourceModelDefinition = JsonDocument.Parse("{}");
        TargetModelDefinition = JsonDocument.Parse("{}");
        TransformationRules = JsonDocument.Parse("[]");
    }

    /// <summary>
    /// Creates a new instance of a DataMap.
    /// </summary>
    /// <param name="id">The unique ID for the map.</param>
    /// <param name="name">The name of the map.</param>
    /// <param name="sourceModelDefinition">The source data schema.</param>
    /// <param name="targetModelDefinition">The target data schema.</param>
    /// <param name="transformationRules">The mapping rules.</param>
    public DataMap(Guid id, string name, JsonDocument sourceModelDefinition, JsonDocument targetModelDefinition, JsonDocument transformationRules)
    {
        if (id == Guid.Empty) throw new ArgumentException("DataMap ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("DataMap name cannot be empty.", nameof(name));
        
        Id = id;
        Name = name;
        SourceModelDefinition = sourceModelDefinition ?? throw new ArgumentNullException(nameof(sourceModelDefinition));
        TargetModelDefinition = targetModelDefinition ?? throw new ArgumentNullException(nameof(targetModelDefinition));
        TransformationRules = transformationRules ?? throw new ArgumentNullException(nameof(transformationRules));
        Version = 1;
    }

    /// <summary>
    /// Updates the data map configuration and increments its version.
    /// </summary>
    /// <param name="name">The new name.</param>
    /// <param name="sourceModelDefinition">The new source model definition.</param>
    /// <param name="targetModelDefinition">The new target model definition.</param>
    /// <param name="transformationRules">The new transformation rules.</param>
    public void Update(string name, JsonDocument sourceModelDefinition, JsonDocument targetModelDefinition, JsonDocument transformationRules)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("DataMap name cannot be empty.", nameof(name));

        Name = name;
        SourceModelDefinition = sourceModelDefinition ?? throw new ArgumentNullException(nameof(sourceModelDefinition));
        TargetModelDefinition = targetModelDefinition ?? throw new ArgumentNullException(nameof(targetModelDefinition));
        TransformationRules = transformationRules ?? throw new ArgumentNullException(nameof(transformationRules));
        Version++;
    }
}