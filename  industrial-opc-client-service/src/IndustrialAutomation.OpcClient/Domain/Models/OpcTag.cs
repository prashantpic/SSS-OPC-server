using IndustrialAutomation.OpcClient.Domain.Enums;
using System;

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Core representation of an OPC tag within the client's domain, 
    /// encapsulating its identity, addressing information, configuration, and current state.
    /// </summary>
    public class OpcTag
    {
        /// <summary>
        /// Unique identifier for the tag within the client.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// The server-specific address of the tag (e.g., NodeId for UA, ItemId for DA).
        /// </summary>
        public required string OpcAddress { get; set; }

        /// <summary>
        /// The OPC standard this tag belongs to (DA, UA, etc.).
        /// </summary>
        public required OpcStandard Standard { get; set; }
        
        /// <summary>
        /// Identifier of the OPC server this tag belongs to.
        /// </summary>
        public required string ServerId { get; init; }

        /// <summary>
        /// Expected data type of the tag (as a string, e.g., "Int32", "Double", "String").
        /// </summary>
        public required string DataType { get; set; }

        /// <summary>
        /// The last known value read from the OPC server for this tag.
        /// </summary>
        public object? LastKnownValue { get; set; }

        /// <summary>
        /// The timestamp of the last known value.
        /// </summary>
        public DateTime? LastKnownTimestamp { get; set; }

        /// <summary>
        /// The quality status of the last known value.
        /// </summary>
        public string? LastKnownQuality { get; set; }

        /// <summary>
        /// Indicates if the tag is actively being monitored or used.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional scaling factor to be applied to the raw value.
        /// </summary>
        public double? ScaleFactor { get; set; }

        /// <summary>
        /// Optional offset to be applied to the raw value (after scaling).
        /// </summary>
        public double? Offset { get; set; }

        /// <summary>
        /// Description of the tag.
        /// </summary>
        public string? Description { get; set; }
    }
}