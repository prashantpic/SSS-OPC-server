using System;

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Represents a data item (e.g., subscription update, alarm, AI output) 
    /// queued in the local buffer for later transmission, typically due to network issues.
    /// </summary>
    public class BufferedDataItem
    {
        /// <summary>
        /// Unique identifier for the buffered item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type of data stored (e.g., "RealtimeDataBatch", "AlarmEventBatch", "EdgeAiOutput").
        /// This helps in deserialization and routing if payloads are generic objects.
        /// </summary>
        public required string DataType { get; init; }

        /// <summary>
        /// The actual data payload to be sent. This should be a serializable DTO.
        /// </summary>
        public required object Payload { get; init; }

        /// <summary>
        /// Timestamp when the item was added to the buffer.
        /// </summary>
        public DateTime TimestampUtc { get; }

        /// <summary>
        /// Number of times transmission of this item has been attempted.
        /// </summary>
        public int RetryCount { get; set; }

        public BufferedDataItem()
        {
            Id = Guid.NewGuid().ToString();
            TimestampUtc = DateTime.UtcNow;
            RetryCount = 0;
        }
    }
}