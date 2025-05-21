using System.Threading.Tasks;
using OPC.Client.Core.Infrastructure.ServerConnectivity.Messaging; // For PublishedEventMessage

namespace OPC.Client.Core.Infrastructure.ServerConnectivity
{
    /// <summary>
    /// Defines the contract for publishing messages to the server-side application via a message queue (e.g., RabbitMQ).
    /// Abstracts the mechanism for sending asynchronous messages/events.
    /// REQ-SAP-003
    /// </summary>
    public interface IServerMessageBusPublisher
    {
        /// <summary>
        /// Publishes a message asynchronously to a specified topic/routing key on the message bus.
        /// </summary>
        /// <param name="message">The message to publish, wrapped in PublishedEventMessage structure.</param>
        /// <param name="topic">The topic or routing key for the message.</param>
        Task PublishAsync(PublishedEventMessage message, string topic);

        /// <summary>
        /// Connects to the message bus.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Disconnects from the message bus.
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Gets the connection status of the message bus.
        /// </summary>
        bool IsConnected { get; }
    }
}