using System;

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Strongly-typed representation of an OPC tag's value, quality, and timestamp, 
    /// ensuring data integrity within the domain.
    /// </summary>
    public class OpcValue
    {
        /// <summary>
        /// The actual data value.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// The timestamp when the value was generated or received.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The quality status of the value (e.g., "Good", "Bad", "Uncertain").
        /// </summary>
        public string QualityStatus { get; }

        public OpcValue(object? value, DateTime timestamp, string qualityStatus)
        {
            Value = value;
            Timestamp = timestamp;
            QualityStatus = qualityStatus ?? throw new ArgumentNullException(nameof(qualityStatus));
        }
    }
}