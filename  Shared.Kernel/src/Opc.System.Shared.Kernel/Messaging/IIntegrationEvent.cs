namespace Opc.System.Shared.Kernel.Messaging;

/// <summary>
/// Defines a base contract for integration events, ensuring essential metadata.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier for the event instance.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the UTC date and time the event was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; }
}