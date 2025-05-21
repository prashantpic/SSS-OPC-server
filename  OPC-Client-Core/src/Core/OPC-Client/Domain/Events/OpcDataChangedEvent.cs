namespace OPC.Client.Core.Domain.Events
{
    using OPC.Client.Core.Domain.ValueObjects;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Domain event raised when an OPC UA subscription reports a data change for one or more monitored items.
    /// Represents a data change notification from an OPC UA subscription, containing the updated NodeAddress
    /// and OpcDataValue for each changed item.
    /// Implements REQ-CSVC-023.
    /// </summary>
    public class OpcDataChangedEvent
    {
        /// <summary>
        /// The client's unique identifier for the connection from which this data change originated.
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// The client's unique identifier for the subscription from which this data change originated (OPC UA specific).
        /// </summary>
        public string SubscriptionId { get; }

        /// <summary>
        /// A list of data values that have changed.
        /// </summary>
        public IReadOnlyList<OpcDataValue> DataChanges { get; }

        /// <summary>
        /// The timestamp when the event was created on the client side.
        /// </summary>
        public DateTime EventTimestampUtc { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcDataChangedEvent"/> class for a single data change.
        /// </summary>
        /// <param name="connectionId">The ID of the connection.</param>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <param name="dataValue">The changed data value.</param>
        public OpcDataChangedEvent(string connectionId, string subscriptionId, OpcDataValue dataValue)
        {
            ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
            SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
            DataChanges = new List<OpcDataValue> { dataValue ?? throw new ArgumentNullException(nameof(dataValue)) }.AsReadOnly();
            EventTimestampUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcDataChangedEvent"/> class for a batch of data changes.
        /// </summary>
        /// <param name="connectionId">The ID of the connection.</param>
        /// <param name="subscriptionId">The ID of the subscription.</param>
        /// <param name="dataChanges">A list of changed data values.</param>
        public OpcDataChangedEvent(string connectionId, string subscriptionId, IEnumerable<OpcDataValue> dataChanges)
        {
            ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
            SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
            DataChanges = (dataChanges ?? throw new ArgumentNullException(nameof(dataChanges))).ToList().AsReadOnly();
            if (!DataChanges.Any())
            {
                throw new ArgumentException("DataChanges list cannot be empty.", nameof(dataChanges));
            }
            EventTimestampUtc = DateTime.UtcNow;
        }
    }
}