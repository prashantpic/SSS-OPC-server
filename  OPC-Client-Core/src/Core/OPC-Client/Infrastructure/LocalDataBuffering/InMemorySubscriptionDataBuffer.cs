namespace OPC.Client.Core.Infrastructure.LocalDataBuffering
{
    using OPC.Client.Core.Domain.ValueObjects;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using System;

    /// <summary>
    /// In-memory implementation of ISubscriptionDataBuffer for OPC UA subscription data.
    /// Provides an in-memory buffer for OPC UA subscription data, helping to prevent
    /// data loss during short-term connectivity issues with the backend.
    /// Implements REQ-CSVC-026.
    /// </summary>
    public class InMemorySubscriptionDataBuffer : ISubscriptionDataBuffer
    {
        private readonly ILogger<InMemorySubscriptionDataBuffer> _logger;

        // Key: SubscriptionId (client-defined string), Value: Queue of data values for that subscription
        private readonly ConcurrentDictionary<string, ConcurrentQueue<OpcDataValue>> _bufferStore;
        private int _globalCapacityPerSubscription = 1000; // Default capacity, can be configured

        public InMemorySubscriptionDataBuffer(ILogger<InMemorySubscriptionDataBuffer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bufferStore = new ConcurrentDictionary<string, ConcurrentQueue<OpcDataValue>>();
            _logger.LogInformation("InMemorySubscriptionDataBuffer initialized. Default capacity per subscription: {Capacity}", _globalCapacityPerSubscription);
        }

        /// <summary>
        /// Configures the buffer, e.g., setting capacity.
        /// </summary>
        /// <param name="capacityPerSubscription">The maximum number of items to buffer per subscription.</param>
        public void Configure(int capacityPerSubscription)
        {
            if (capacityPerSubscription <= 0)
            {
                _logger.LogWarning("Buffer capacity must be greater than zero. Using default: {DefaultCapacity}", _globalCapacityPerSubscription);
            }
            else
            {
                _globalCapacityPerSubscription = capacityPerSubscription;
                _logger.LogInformation("InMemorySubscriptionDataBuffer capacity set to {Capacity} per subscription.", _globalCapacityPerSubscription);
            }
        }

        /// <summary>
        /// Adds a data value to the buffer for a specific subscription.
        /// If the buffer for the subscription is full, the oldest item is discarded.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <param name="dataValue">The data value to add.</param>
        public void AddData(string subscriptionId, OpcDataValue dataValue)
        {
            if (string.IsNullOrEmpty(subscriptionId)) throw new ArgumentNullException(nameof(subscriptionId));
            if (dataValue == null) throw new ArgumentNullException(nameof(dataValue));

            var queue = _bufferStore.GetOrAdd(subscriptionId, _ => new ConcurrentQueue<OpcDataValue>());

            if (queue.Count >= _globalCapacityPerSubscription)
            {
                if (queue.TryDequeue(out var dequeuedItem))
                {
                    _logger.LogWarning("Buffer for subscription {SubscriptionId} is full (Capacity: {Capacity}). Discarding oldest item: {NodeAddress} @ {Timestamp}",
                        subscriptionId, _globalCapacityPerSubscription, dequeuedItem.NodeAddress, dequeuedItem.Timestamp);
                }
                else
                {
                    // This case should be rare with ConcurrentQueue if Count >= Capacity,
                    // but indicates a potential concurrency issue or unexpected state.
                    _logger.LogError("Buffer full for subscription {SubscriptionId}, but failed to dequeue oldest item. Data might be lost or buffer might exceed capacity.", subscriptionId);
                }
            }

            queue.Enqueue(dataValue);
            _logger.LogTrace("Buffered data for Subscription: {SubscriptionId}, Node: {NodeAddress}, Value: {Value}. Current buffer size: {CurrentSize}",
                subscriptionId, dataValue.NodeAddress, dataValue.Value, queue.Count);
        }

        /// <summary>
        /// Retrieves all buffered data for a specific subscription and clears the buffer for that subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <returns>A list of buffered OpcDataValue objects. Returns an empty list if no data or subscription found.</returns>
        public IEnumerable<OpcDataValue> GetAndClearBuffer(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) throw new ArgumentNullException(nameof(subscriptionId));

            if (_bufferStore.TryGetValue(subscriptionId, out var queue))
            {
                var items = new List<OpcDataValue>();
                while (queue.TryDequeue(out var item))
                {
                    items.Add(item);
                }

                if (items.Any())
                {
                    _logger.LogInformation("Retrieved and cleared {Count} buffered items for subscription {SubscriptionId}.", items.Count, subscriptionId);
                }
                else
                {
                    _logger.LogDebug("No buffered items to retrieve for subscription {SubscriptionId}.", subscriptionId);
                }
                return items;
            }

            _logger.LogDebug("No buffer found for subscription {SubscriptionId} to retrieve and clear.", subscriptionId);
            return Enumerable.Empty<OpcDataValue>();
        }

        /// <summary>
        /// Gets the current count of buffered items for a specific subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <returns>The number of items in the buffer for the subscription, or 0 if not found.</returns>
        public int GetBufferedItemCount(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) return 0;

            return _bufferStore.TryGetValue(subscriptionId, out var queue) ? queue.Count : 0;
        }

        /// <summary>
        /// Checks if there is any buffered data for a specific subscription.
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <returns>True if data is buffered, false otherwise.</returns>
        public bool HasBufferedData(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) return false;

            return _bufferStore.TryGetValue(subscriptionId, out var queue) && !queue.IsEmpty;
        }

        /// <summary>
        /// Clears all buffered data for all subscriptions.
        /// </summary>
        public void ClearAllBuffers()
        {
            _bufferStore.Clear();
            _logger.LogInformation("All subscription data buffers cleared.");
        }
    }

    /// <summary>
    /// Interface for buffering subscription data during connectivity issues.
    /// </summary>
    public interface ISubscriptionDataBuffer
    {
        void Configure(int capacityPerSubscription);
        void AddData(string subscriptionId, OpcDataValue dataValue);
        IEnumerable<OpcDataValue> GetAndClearBuffer(string subscriptionId);
        int GetBufferedItemCount(string subscriptionId);
        bool HasBufferedData(string subscriptionId);
        void ClearAllBuffers();
    }
}