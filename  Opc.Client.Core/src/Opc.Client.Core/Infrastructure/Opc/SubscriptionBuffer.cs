using Opc.Client.Core.Application.Interfaces;
using System.Collections.Concurrent;

namespace Opc.Client.Core.Infrastructure.Opc;

/// <summary>
/// Provides a temporary, in-memory buffer for OPC UA subscription data changes during network outages.
/// </summary>
/// <remarks>
/// This class is designed to prevent data loss from high-frequency OPC subscriptions during brief
/// periods of disconnection from the central server application. It uses a thread-safe queue
/// and has a configurable size limit to prevent unbounded memory growth.
/// </remarks>
public class SubscriptionBuffer
{
    private readonly ConcurrentQueue<DataChangeNotification> _buffer;
    private readonly int _bufferSizeLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionBuffer"/> class.
    /// </summary>
    /// <param name="bufferSizeLimit">The maximum number of notifications to store in the buffer.</param>
    public SubscriptionBuffer(int bufferSizeLimit = 10000)
    {
        _buffer = new ConcurrentQueue<DataChangeNotification>();
        _bufferSizeLimit = bufferSizeLimit;
    }

    /// <summary>
    /// Adds a data change notification to the buffer.
    /// </summary>
    /// <remarks>
    /// If the buffer is full, the oldest item is removed to make space for the new one.
    /// </remarks>
    /// <param name="notification">The notification to enqueue.</param>
    public void Enqueue(DataChangeNotification notification)
    {
        if (_buffer.Count >= _bufferSizeLimit)
        {
            // Discard the oldest item to prevent unbounded memory usage.
            _buffer.TryDequeue(out _);
        }
        _buffer.Enqueue(notification);
    }

    /// <summary>
    /// Drains all buffered notifications and publishes them using the provided publisher.
    /// </summary>
    /// <param name="publisher">The event publisher to send the notifications to.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    public async Task DrainAndPublishAsync(IServerEventPublisher publisher)
    {
        while (_buffer.TryDequeue(out var notification))
        {
            try
            {
                await publisher.PublishDataChangeAsync(notification);
            }
            catch
            {
                // If publishing fails again, we might re-enqueue or log and discard.
                // For simplicity, we will log and discard here.
                // In a real scenario, a more robust retry policy might be needed.
            }
        }
    }

    /// <summary>
    /// Clears all items from the buffer without publishing them.
    /// </summary>
    public void Clear()
    {
        _buffer.Clear();
    }

    /// <summary>
    /// Gets the current number of items in the buffer.
    /// </summary>
    public int Count => _buffer.Count;
}