namespace Opc.System.Shared.Kernel.Messaging.Events;

/// <summary>
/// Represents an alarm or condition event triggered by an OPC A&E server and captured by a client instance.
/// </summary>
public record AlarmTriggeredEvent(
    Guid EventId,
    DateTimeOffset CreationDate,
    Guid ClientId,
    string SourceNode,
    string Message,
    int Severity,
    bool Acknowledged,
    DateTimeOffset OccurrenceTime) : IIntegrationEvent;