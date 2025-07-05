namespace Opc.Client.Core.Application.Interfaces;

/// <summary>
/// Provides a contract for publishing events from the OPC client to the centralized server application.
/// </summary>
/// <remarks>
/// This interface abstracts the mechanism of sending asynchronous events to the backend, 
/// decoupling the core logic from specific message broker technologies like RabbitMQ or Kafka.
/// It follows the Dependency Inversion Principle.
/// </remarks>
public interface IServerEventPublisher
{
    /// <summary>
    /// Asynchronously publishes a data change notification.
    /// </summary>
    /// <param name="notification">The data change event to publish.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishDataChangeAsync(DataChangeNotification notification);

    /// <summary>
    /// Asynchronously publishes an alarm event notification.
    /// </summary>
    /// <param name="notification">The alarm event to publish.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAlarmAsync(AlarmEventNotification notification);

    /// <summary>
    /// Asynchronously publishes the health status of the client.
    /// </summary>
    /// <param name="status">The client health status to publish.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishHealthStatusAsync(ClientHealthStatus status);
    
    /// <summary>
    /// Asynchronously publishes an audit log for a critical write operation.
    /// </summary>
    /// <param name="log">The critical write log entry to publish.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishCriticalWriteLogAsync(CriticalWriteLog log);
}

// --- Placeholder types for compilation ---

/// <summary>
/// Represents a data change from an OPC subscription.
/// </summary>
public record DataChangeNotification(Guid ServerId, string NodeId, object? Value, string Quality, DateTimeOffset Timestamp);

/// <summary>
/// Represents an alarm or condition event from an OPC A&C server.
/// </summary>
public record AlarmEventNotification(Guid ServerId, string SourceNode, int Severity, string Message, DateTimeOffset OccurrenceTime);

/// <summary>
/// Represents the health status of the OPC client service.
/// </summary>
public record ClientHealthStatus(Guid ClientId, string Status, double CpuUsage, double MemoryUsage, DateTimeOffset Timestamp);

/// <summary>
/// Represents an audit log for a critical write operation.
/// </summary>
public record CriticalWriteLog(Guid ServerId, string NodeId, string UserId, object? OldValue, object NewValue, DateTimeOffset Timestamp);