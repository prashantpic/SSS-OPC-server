using OPC.Client.Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace OPC.Client.Core.Infrastructure.LocalDataBuffering
{
    /// <summary>
    /// Interface for buffering OPC UA subscription data during short network interruptions.
    /// Defines a contract for temporarily storing OPC UA subscription data locally.
    /// REQ-CSVC-026
    /// </summary>
    public interface ISubscriptionDataBuffer
    {
        /// <summary>
        /// Adds a data change to the buffer for a specific subscription.
        /// </summary>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <param name="dataValue">The OPC data value to buffer.</param>
        void BufferDataChange(string subscriptionId, OpcDataValue dataValue);

        /// <summary>
        /// Retrieves all buffered data changes for a specific subscription and clears them from the buffer.
        /// </summary>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <returns>A list of buffered OPC data values. Returns an empty list if no data for the subscription.</returns>
        IEnumerable<OpcDataValue> GetAndClearBuffer(string subscriptionId);

        /// <summary>
        /// Checks if there is any buffered data for a specific subscription.
        /// </summary>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <returns>True if buffered data exists, false otherwise.</returns>
        bool HasBufferedData(string subscriptionId);

        /// <summary>
        /// Gets the count of buffered items for a specific subscription.
        /// </summary>
        /// <param name="subscriptionId">The identifier of the subscription.</param>
        /// <returns>The number of buffered items.</returns>
        int GetBufferedDataCount(string subscriptionId);

        /// <summary>
        /// Sets the capacity of the buffer for a specific subscription or globally.
        /// </summary>
        /// <param name="capacity">The maximum number of items to buffer.</param>
        /// <param name="subscriptionId">Optional. If provided, sets capacity for a specific subscription.</param>
        void SetCapacity(int capacity, string? subscriptionId = null);

        /// <summary>
        /// Clears all buffered data for all subscriptions.
        /// </summary>
        void ClearAllBuffers();
    }
}