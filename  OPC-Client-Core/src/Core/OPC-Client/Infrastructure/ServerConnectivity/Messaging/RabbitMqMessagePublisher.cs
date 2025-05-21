namespace OPC.Client.Core.Infrastructure.ServerConnectivity.Messaging
{
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.Logging;
    using OPC.Client.Core.Exceptions;
    using System;
    using System.Text;
    using System.Text.Json; // Using System.Text.Json for serialization
    using System.Threading.Tasks;

    /// <summary>
    /// Implements IServerMessageBusPublisher for publishing messages to RabbitMQ exchanges.
    /// Handles publishing messages (e.g., OPC data, alarms, events) to specified RabbitMQ
    /// exchanges/queues for asynchronous communication with the server-side application.
    /// Implements REQ-SAP-003.
    /// </summary>
    public class RabbitMqMessagePublisher : IServerMessageBusPublisher
    {
        private readonly ILogger<RabbitMqMessagePublisher> _logger;
        private readonly RabbitMqConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IModel? _channel;
        private RabbitMqConfiguration? _rabbitMqConfig; // Store specific config

        public RabbitMqMessagePublisher(
            ILogger<RabbitMqMessagePublisher> logger,
            RabbitMqConnectionFactory connectionFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Configures the RabbitMQ publisher with connection settings.
        /// This method must be called before publishing messages.
        /// </summary>
        /// <param name="config">The RabbitMQ configuration.</param>
        public void Configure(RabbitMqConfiguration config)
        {
            _rabbitMqConfig = config ?? throw new ArgumentNullException(nameof(config));
            _logger.LogInformation("RabbitMQMessagePublisher configured for Host: {Hostname}, Port: {Port}, DataExchange: {DataExchange}, AlarmExchange: {AlarmExchange}",
                _rabbitMqConfig.Hostname, _rabbitMqConfig.Port, _rabbitMqConfig.DataExchangeName, _rabbitMqConfig.AlarmEventExchangeName);

            EnsureConnectionAndChannel();
        }

        private void EnsureConnectionAndChannel()
        {
            if (_rabbitMqConfig == null)
            {
                _logger.LogError("RabbitMQ configuration is not set. Call Configure() first.");
                throw new InvalidOperationException("RabbitMQ publisher is not configured.");
            }

            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogInformation("RabbitMQ channel is null or closed. Attempting to establish connection and channel.");
                try
                {
                    _connection = _connectionFactory.GetConnection(_rabbitMqConfig);
                    _channel = _connection.CreateModel();

                    // Declare exchanges if they might not exist (idempotent operation)
                    // Type "topic" is flexible for routing.
                    _channel.ExchangeDeclare(exchange: _rabbitMqConfig.DataExchangeName, type: ExchangeType.Topic, durable: true);
                    _channel.ExchangeDeclare(exchange: _rabbitMqConfig.AlarmEventExchangeName, type: ExchangeType.Topic, durable: true);

                    _logger.LogInformation("RabbitMQ connection and channel established. Exchanges declared.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to establish RabbitMQ connection or channel.");
                    _channel = null; // Ensure channel is null on failure
                    throw new ServerConnectivityException("Failed to initialize RabbitMQ publisher connection.", ex);
                }
            }
        }


        /// <summary>
        /// Publishes a message asynchronously to a specified RabbitMQ exchange with a routing key.
        /// The message payload will be serialized to JSON.
        /// </summary>
        /// <param name="exchangeName">The name of the exchange to publish to.</param>
        /// <param name="routingKey">The routing key for the message.</param>
        /// <param name="message">The message object to publish (will be JSON serialized).</param>
        /// <exception cref="ServerConnectivityException">If publishing fails due to connection issues or RabbitMQ errors.</exception>
        public Task PublishAsync(string exchangeName, string routingKey, object message)
        {
            return Task.Run(() => // Run on a background thread to avoid blocking caller if sync context exists
            {
                EnsureConnectionAndChannel();
                if (_channel == null) // Should be caught by EnsureConnectionAndChannel, but as a safeguard
                {
                    _logger.LogError("Cannot publish message: RabbitMQ channel is not available.");
                    throw new ServerConnectivityException("RabbitMQ channel is not available for publishing.");
                }

                _logger.LogTrace("Publishing message to Exchange: {Exchange}, RoutingKey: {RoutingKey}", exchangeName, routingKey);

                try
                {
                    var messageBodyBytes = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions { WriteIndented = false });

                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true; // Make messages persistent
                    properties.ContentType = "application/json";
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    _channel.BasicPublish(
                        exchange: exchangeName,
                        routingKey: routingKey,
                        mandatory: false, // Set to true to handle unroutable messages via BasicReturn
                        basicProperties: properties,
                        body: messageBodyBytes);

                    _logger.LogTrace("Message published successfully to Exchange: {Exchange}, RoutingKey: {RoutingKey}, Size: {Size} bytes",
                                     exchangeName, routingKey, messageBodyBytes.Length);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to serialize message to JSON for RabbitMQ publishing.");
                    throw new ArgumentException("Failed to serialize message payload.", nameof(message), jsonEx);
                }
                catch (global::RabbitMQ.Client.Exceptions.AlreadyClosedException acEx)
                {
                    _logger.LogError(acEx, "RabbitMQ connection or channel was already closed while publishing.");
                    _channel = null; // Force re-creation on next attempt
                    throw new ServerConnectivityException("RabbitMQ connection/channel closed during publish.", acEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error publishing message to RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}", exchangeName, routingKey);
                    // Consider if channel/connection should be reset here
                    throw new ServerConnectivityException($"Unexpected error publishing to RabbitMQ: {ex.Message}", ex);
                }
            });
        }


        /// <summary>
        /// Publishes a standardized event message to RabbitMQ.
        /// The specific exchange and routing key are determined by the event type or configuration.
        /// </summary>
        /// <param name="eventMessage">The event message to publish.</param>
        public Task PublishEventAsync(PublishedEventMessage eventMessage)
        {
            if (_rabbitMqConfig == null)
            {
                _logger.LogError("RabbitMQ configuration is not set. Cannot determine exchange for event.");
                throw new InvalidOperationException("RabbitMQ publisher is not configured.");
            }

            string exchangeName;
            string routingKey; // Routing key can be derived from EventType or other properties

            // Example: Determine exchange based on event type
            if (eventMessage.EventType.StartsWith("OpcTagDataChange", StringComparison.OrdinalIgnoreCase))
            {
                exchangeName = _rabbitMqConfig.DataExchangeName;
                routingKey = $"opc.data.{eventMessage.SourceId?.ToLowerInvariant() ?? "unknown"}.{eventMessage.EventType.ToLowerInvariant()}";
            }
            else if (eventMessage.EventType.StartsWith("OpcAlarm", StringComparison.OrdinalIgnoreCase) ||
                     eventMessage.EventType.StartsWith("OpcEvent", StringComparison.OrdinalIgnoreCase))
            {
                exchangeName = _rabbitMqConfig.AlarmEventExchangeName;
                routingKey = $"opc.alarm.{eventMessage.SourceId?.ToLowerInvariant() ?? "unknown"}.{eventMessage.EventType.ToLowerInvariant()}";
            }
            else
            {
                // Default or error
                _logger.LogWarning("Unknown event type '{EventType}'. Using default data exchange and generic routing key.", eventMessage.EventType);
                exchangeName = _rabbitMqConfig.DataExchangeName; // Fallback to data exchange
                routingKey = $"opc.event.unknown.{eventMessage.SourceId?.ToLowerInvariant() ?? "unknown"}";
            }

            return PublishAsync(exchangeName, routingKey, eventMessage);
        }


        public void Dispose()
        {
            _logger.LogInformation("Disposing RabbitMqMessagePublisher.");
            try
            {
                // Channel and Connection are managed by RabbitMqConnectionFactory now,
                // so this publisher might not need to dispose them directly if it's
                // just borrowing them per operation. However, if it holds onto them,
                // it should dispose. Assuming it holds for now for simplicity of EnsureConnectionAndChannel.
                _channel?.Close(); // Close the channel
                _channel?.Dispose();
                _channel = null;

                // The connection itself is managed by the factory and should be reused/disposed there.
                // Do not dispose the connection here if it's shared.
                // If this publisher created its own connection, it should dispose it.
                // _connection?.Close();
                // _connection?.Dispose();
                // _connection = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during RabbitMqMessagePublisher disposal.");
            }
        }
    }

    /// <summary>
    /// Interface for asynchronous message publishing to a message bus (e.g., RabbitMQ).
    /// </summary>
    public interface IServerMessageBusPublisher : IDisposable
    {
        void Configure(RabbitMqConfiguration config);
        Task PublishAsync(string exchangeName, string routingKey, object message);
        Task PublishEventAsync(PublishedEventMessage eventMessage);
    }

    /// <summary>
    /// Standardized message structure for events published to RabbitMQ.
    /// </summary>
    public class PublishedEventMessage
    {
        /// <summary>
        /// Type of the event (e.g., "OpcTagDataChange", "OpcAlarmEvent").
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the source client or connection that generated the event.
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Timestamp of when the event was generated (UTC).
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// The actual event payload, which will be serialized (e.g., to JSON).
        /// </summary>
        public object? Payload { get; set; }

        public PublishedEventMessage()
        {
            TimestampUtc = DateTime.UtcNow;
        }
    }
}