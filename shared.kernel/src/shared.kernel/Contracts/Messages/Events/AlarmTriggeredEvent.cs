namespace SharedKernel.Contracts.Messages.Events;

/// <summary>
/// Represents an event published when an alarm is triggered or its state changes.
/// </summary>
/// <param name="ClientId">The unique identifier of the OPC client that sourced the alarm.</param>
/// <param name="SourceNode">The OPC node that the alarm is associated with.</param>
/// <param name="EventType">The type of the event (e.g., 'HighHigh', 'DeviceFailure').</param>
/// <param name="Severity">The severity of the alarm, typically a numerical value.</param>
/// <param name="Message">The descriptive message associated with the alarm.</param>
/// <param name="Timestamp">The timestamp of the alarm occurrence.</param>
public record AlarmTriggeredEvent(
    Guid ClientId,
    string SourceNode,
    string EventType,
    int Severity,
    string Message,
    DateTimeOffset Timestamp);