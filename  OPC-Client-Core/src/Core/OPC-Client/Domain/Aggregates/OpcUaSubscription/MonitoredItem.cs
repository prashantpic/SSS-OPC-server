namespace OPC.Client.Core.Domain.Aggregates.OpcUaSubscription
{
    using OPC.Client.Core.Domain.ValueObjects;
    using System;

    /// <summary>
    /// Entity representing a monitored item within an OPC UA subscription.
    /// Defines a specific OPC UA node to be monitored, including its configuration.
    /// Implements REQ-CSVC-023.
    /// </summary>
    public class MonitoredItem
    {
        /// <summary>
        /// Unique identifier for the monitored item within the client's subscription context.
        /// </summary>
        public string ClientHandle { get; }

        /// <summary>
        /// The OPC UA NodeId to monitor.
        /// </summary>
        public NodeAddress NodeAddress { get; }

        /// <summary>
        /// Requested sampling interval in milliseconds.
        /// </summary>
        public double SamplingIntervalMs { get; }

        /// <summary>
        /// Type of deadband to apply (e.g., "None", "Absolute", "Percent").
        /// Corresponds to Opc.Ua.DeadbandType.
        /// </summary>
        public string DeadbandType { get; }

        /// <summary>
        /// Value for the deadband filter. Interpretation depends on DeadbandType.
        /// </summary>
        public double DeadbandValue { get; }

        /// <summary>
        /// The size of the queue for data change notifications for this item on the server.
        /// A value of 0 or 1 usually means the server default or latest value only.
        /// </summary>
        public uint QueueSize { get; }

        /// <summary>
        /// Specifies whether the server should discard the oldest or newest item in the queue when it overflows.
        /// True to discard oldest.
        /// </summary>
        public bool DiscardOldest { get; }

        /// <summary>
        /// The filter to use for data changes (e.g., "StatusValue", "Status", "Value").
        /// Corresponds to Opc.Ua.DataChangeTrigger.
        /// </summary>
        public string DataChangeFilter { get; }

        /// <summary>
        /// The last known value received for this monitored item.
        /// </summary>
        public OpcDataValue? LastKnownValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoredItem"/> class.
        /// </summary>
        /// <param name="clientHandle">A unique client-side identifier for this item.</param>
        /// <param name="nodeAddress">The NodeId of the item to monitor.</param>
        /// <param name="samplingIntervalMs">The requested sampling interval.</param>
        /// <param name="deadbandType">The type of deadband filter.</param>
        /// <param name="deadbandValue">The value for the deadband filter.</param>
        /// <param name="queueSize">The server-side queue size for notifications.</param>
        /// <param name="discardOldest">True to discard the oldest item in a full queue, false to discard the newest.</param>
        /// <param name="dataChangeFilter">The data change filter trigger.</param>
        public MonitoredItem(
            string clientHandle,
            NodeAddress nodeAddress,
            double samplingIntervalMs,
            string deadbandType,
            double deadbandValue,
            uint queueSize = 1, // Default to 1 for latest value semantics
            bool discardOldest = true,
            string dataChangeFilter = "StatusValue")
        {
            ClientHandle = clientHandle ?? throw new ArgumentNullException(nameof(clientHandle));
            NodeAddress = nodeAddress ?? throw new ArgumentNullException(nameof(nodeAddress));
            SamplingIntervalMs = samplingIntervalMs;
            DeadbandType = deadbandType ?? "None";
            DeadbandValue = deadbandValue;
            QueueSize = queueSize;
            DiscardOldest = discardOldest;
            DataChangeFilter = dataChangeFilter ?? "StatusValue";
        }

        /// <summary>
        /// Updates the last known value of the monitored item.
        /// Called by the OpcUaSubscription aggregate when a data change is processed.
        /// </summary>
        /// <param name="newValue">The new data value received.</param>
        internal void UpdateLastValue(OpcDataValue newValue)
        {
            LastKnownValue = newValue ?? throw new ArgumentNullException(nameof(newValue));
        }
    }
}