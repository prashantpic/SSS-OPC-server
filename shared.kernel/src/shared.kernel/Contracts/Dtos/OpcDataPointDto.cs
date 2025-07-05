namespace SharedKernel.Contracts.Dtos;

/// <summary>
/// Represents a single data point reading from an OPC server.
/// This is an immutable data transfer object.
/// </summary>
/// <param name="TagIdentifier">The unique identifier for the OPC tag.</param>
/// <param name="Value">The value of the tag, serialized as a string for universal compatibility.</param>
/// <param name="Quality">The quality status of the reading (e.g., 'Good', 'Bad').</param>
/// <param name="Timestamp">The timestamp of when the value was recorded by the server.</param>
public record OpcDataPointDto(
    string TagIdentifier,
    string Value,
    string Quality,
    DateTimeOffset Timestamp);