using SharedKernel.Contracts.Dtos;

namespace SharedKernel.Contracts.Messages.Events;

/// <summary>
/// Represents an event published when a batch of OPC data is received.
/// </summary>
/// <param name="ClientId">The unique identifier of the OPC client that sourced the data.</param>
/// <param name="DataPoints">A collection of OPC data points in this batch.</param>
/// <param name="EventTimestamp">The timestamp when the event was created.</param>
public record OpcDataReceivedEvent(
    Guid ClientId,
    IReadOnlyList<OpcDataPointDto> DataPoints,
    DateTimeOffset EventTimestamp);