using OPC.Client.Core.Domain.ValueObjects;
using System;

namespace OPC.Client.Core.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for OPC tag values, used in API contracts.
    /// Represents an OPC tag's data (value, quality, timestamp) for facade method parameters and return types.
    /// REQ-CSVC-003, REQ-CSVC-004
    /// </summary>
    public class OpcTagValueDto
    {
        /// <summary>
        /// The address or identifier of the OPC node/tag.
        /// </summary>
        public NodeAddress NodeAddress { get; set; } = new NodeAddress(string.Empty, null);

        /// <summary>
        /// The actual value of the tag. Can be various types.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// The quality status of the tag value (e.g., Opc.Ua.StatusCodes.Good).
        /// For simplicity, using ushort to represent OPC quality.
        /// </summary>
        public ushort Quality { get; set; }

        /// <summary>
        /// The timestamp when the value was acquired or generated.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}