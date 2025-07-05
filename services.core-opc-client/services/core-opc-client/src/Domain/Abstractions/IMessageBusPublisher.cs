using services.opc.client.Domain.Models;

namespace services.opc.client.Domain.Abstractions;

/// <summary>
/// Defines the contract for publishing messages to the central message bus.
/// This decouples the core application logic from the specific message queue technology being used.
/// </summary>
public interface IMessageBusPublisher
{
    /// <summary>
    /// Publishes a single data point to the message bus.
    /// </summary>
    /// <param name="dataPoint">The data point to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishDataPointAsync(DataPoint dataPoint);

    /// <summary>
    /// Publishes a batch of data points to the message bus.
    /// </summary>
    /// <param name="dataPoints">The collection of data points to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishDataPointsAsync(IEnumerable<DataPoint> dataPoints);
    
    /// <summary>
    /// Publishes a single alarm event to the message bus.
    /// </summary>
    /// <param name="alarmEvent">The alarm event to publish.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task PublishAlarmAsync(AlarmEvent alarmEvent);
}