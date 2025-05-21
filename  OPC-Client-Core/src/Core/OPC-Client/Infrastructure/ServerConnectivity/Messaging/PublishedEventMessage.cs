using System;

namespace OPC.Client.Core.Infrastructure.ServerConnectivity.Messaging
{
    /// <summary>
    /// POCO representing the structure of messages serialized and sent via RabbitMQ or other message buses.
    /// Defines the common schema for messages published to the message bus.
    /// Implements REQ-SAP-003.
    /// </summary>
    public class PublishedEventMessage
    {
        /// <summary>
        /// Gets or sets the type of the event (e.g., "OpcTagDataChange", "OpcAlarmEvent", "ClientHealthUpdate").
        /// This can be used for routing or deserialization on the consumer side.
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the client instance that originated this event.
        /// </summary>
        public string SourceClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the event was generated or published.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the actual payload of the event. This will be serialized (e.g., to JSON).
        /// The type of this object depends on the EventType.
        /// </summary>
        public object? Payload { get; set; }

        /// <summary>
        /// Gets or sets a unique identifier for this specific message instance (optional).
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedEventMessage"/> class.
        /// </summary>
        public PublishedEventMessage()
        {
            MessageId = Guid.NewGuid();
            TimestampUtc = DateTime.UtcNow; // Default to current time, can be overridden
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedEventMessage"/> class with specified parameters.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="sourceClientId">The identifier of the source client.</param>
        /// <param name="payload">The event payload.</param>
        public PublishedEventMessage(string eventType, string sourceClientId, object? payload)
            : this() // Calls the default constructor
        {
            EventType = eventType;
            SourceClientId = sourceClientId;
            Payload = payload;
        }
    }
}