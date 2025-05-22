using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using System;

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Represents an active OPC UA subscription, holding its configuration, 
    /// list of monitored items, and status information.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Unique identifier for this subscription instance.
        /// </summary>
        public required string Id { get; init; }

        /// <summary>
        /// Identifier of the OPC UA server this subscription belongs to.
        /// </summary>
        public required string ServerId { get; init; }

        /// <summary>
        /// The configuration DTO used to create this subscription.
        /// </summary>
        public required UaSubscriptionConfigDto Config { get; set; }

        /// <summary>
        /// Current operational status of the subscription (e.g., "Active", "Connecting", "Error").
        /// </summary>
        public string Status { get; set; } = "Unknown";

        /// <summary>
        /// Timestamp of the last data change received for this subscription.
        /// </summary>
        public DateTime? LastDataChange { get; set; }

        /// <summary>
        /// The last error message encountered by this subscription, if any.
        /// </summary>
        public string? LastErrorMessage { get; set; }

        /// <summary>
        /// OPC UA specific subscription ID from the server.
        /// </summary>
        public uint? ServerSubscriptionId { get; set; }
    }
}