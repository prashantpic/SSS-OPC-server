using OPC.Client.Core.Domain.ValueObjects;
using System.Collections.Generic;

namespace OPC.Client.Core.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for OPC UA subscription configuration parameters.
    /// Encapsulates parameters for creating or modifying an OPC UA subscription.
    /// REQ-CSVC-023
    /// </summary>
    public class UaSubscriptionConfigDto
    {
        /// <summary>
        /// The desired publishing interval in milliseconds.
        /// </summary>
        public double PublishingIntervalMs { get; set; } = 1000;

        /// <summary>
        /// The number of publishing intervals for the lifetime of the subscription.
        /// </summary>
        public uint LifetimeCount { get; set; } = 120; // e.g., 120 * 1000ms = 2 minutes

        /// <summary>
        /// The maximum number of keep-alive messages that may be sent before the server declares the subscription lost.
        /// </summary>
        public uint MaxKeepAliveCount { get; set; } = 3;

        /// <summary>
        /// The maximum number of notifications per publish.
        /// </summary>
        public uint MaxNotificationsPerPublish { get; set; } = 1000;

        /// <summary>
        /// The priority of the subscription. Lower values are higher priority.
        /// </summary>
        public byte Priority { get; set; } = 0;

        /// <summary>
        /// Indicates if publishing should be enabled for the subscription.
        /// </summary>
        public bool PublishingEnabled { get; set; } = true;

        /// <summary>
        /// The list of items to monitor within this subscription.
        /// </summary>
        public List<MonitoredItemConfigDto> MonitoredItems { get; set; } = new List<MonitoredItemConfigDto>();
    }

    /// <summary>
    /// Data Transfer Object for configuring a single monitored item within an OPC UA subscription.
    /// </summary>
    public class MonitoredItemConfigDto
    {
        /// <summary>
        /// The address or identifier of the OPC UA node to monitor.
        /// </summary>
        public NodeAddress NodeAddress { get; set; } = new NodeAddress(string.Empty, null);

        /// <summary>
        /// The desired sampling interval in milliseconds. A value of 0 indicates server default.
        /// </summary>
        public double SamplingIntervalMs { get; set; } = 1000;

        /// <summary>
        /// The deadband type for reporting data changes (e.g., None, Absolute, Percent).
        /// This should align with Opc.Ua.DeadbandType enum values if possible or be mapped.
        /// For DTO, string is simpler.
        /// </summary>
        public string DeadbandType { get; set; } = "None"; // "None", "Absolute", "Percent"

        /// <summary>
        /// The deadband value, interpretation depends on DeadbandType.
        /// </summary>
        public double DeadbandValue { get; set; } = 0.0;

        /// <summary>
        /// The size of the queue for data change notifications for this item.
        /// A value of 0 or 1 indicates the server default.
        /// </summary>
        public uint QueueSize { get; set; } = 1;

        /// <summary>
        /// Indicates if the oldest notification should be discarded if the queue overflows.
        /// </summary>
        public bool DiscardOldest { get; set; } = true;
    }
}