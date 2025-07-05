namespace Opc.System.Shared.Kernel.Messaging.Events;

/// <summary>
/// Represents a batch of real-time data points received from an OPC client instance.
/// This event is published for asynchronous ingestion and processing.
/// </summary>
public record OpcDataReceivedEvent(
    Guid EventId,
    DateTimeOffset CreationDate,
    Guid ClientId,
    Guid ServerId,
    IReadOnlyList<DataPointDto> DataPoints) : IIntegrationEvent;

/// <summary>
/// DTO representing a single OPC tag data point.
/// </summary>
public record DataPointDto(
    string TagIdentifier,
    string Value,
    string Quality,
    DateTimeOffset Timestamp);