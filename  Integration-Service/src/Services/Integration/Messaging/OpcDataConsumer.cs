using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Interfaces; // For placeholder Integration Services
using Microsoft.Extensions.DependencyInjection;


namespace IntegrationService.Messaging
{
    // Placeholder for OPC Data Payload DTO
    public record OpcDataPayload(string TagId, DateTime Timestamp, object Value, string? Quality, string? SourceNodeId);

    // Placeholder for Message Queue Settings
    public class MessageQueueSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string QueueName { get; set; } = "opc-data-input";
        public string ExchangeName { get; set; } = ""; // Default exchange if empty
        public string RoutingKey { get; set; } = "opc-data-input"; // Use queue name if default exchange
    }


    public class OpcDataConsumer : BackgroundService
    {
        private readonly ILogger<OpcDataConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly MessageQueueSettings _mqSettings;
        private IConnection? _connection;
        private IModel? _channel;

        public OpcDataConsumer(
            ILogger<OpcDataConsumer> logger,
            IServiceProvider serviceProvider,
            IOptions<MessageQueueSettings> mqSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _mqSettings = mqSettings?.Value ?? throw new ArgumentNullException(nameof(mqSettings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OpcDataConsumer starting.");
            stoppingToken.Register(() => _logger.LogInformation("OpcDataConsumer is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        ConnectToRabbitMQ(stoppingToken);
                    }

                    if (_channel != null && _channel.IsOpen)
                    {
                        var consumer = new EventingBasicConsumer(_channel);
                        consumer.Received += async (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            _logger.LogDebug("Received message: {Message}", message);

                            try
                            {
                                await ProcessMessageAsync(message, stoppingToken);
                                _channel.BasicAck(ea.DeliveryTag, false);
                            }
                            catch (JsonException jsonEx)
                            {
                                _logger.LogError(jsonEx, "Failed to deserialize message: {Message}. Sending to dead-letter queue or NACKing.", message);
                                _channel.BasicNack(ea.DeliveryTag, false, false); // false for requeue: do not requeue malformed messages
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error processing message: {Message}. NACKing and requeueing.", message);
                                _channel.BasicNack(ea.DeliveryTag, false, true); // true for requeue: might be a transient issue
                                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Delay before retrying connection or processing
                            }
                        };

                        _channel.BasicConsume(queue: _mqSettings.QueueName, autoAck: false, consumer: consumer);
                        _logger.LogInformation("Consumer started on queue '{QueueName}'. Waiting for messages.", _mqSettings.QueueName);

                        // Keep the service alive while listening
                        while (!stoppingToken.IsCancellationRequested && _channel.IsOpen)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        }
                    }
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                {
                    _logger.LogError(ex, "Cannot connect to RabbitMQ. Retrying in 5 seconds...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred in OpcDataConsumer. Retrying in 5 seconds...");
                }
                finally
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Delay before retrying connection loop
                    }
                }
            }
            _logger.LogInformation("OpcDataConsumer ExecuteAsync loop finishing.");
        }

        private void ConnectToRabbitMQ(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested) return;

            var factory = new ConnectionFactory()
            {
                HostName = _mqSettings.HostName,
                UserName = _mqSettings.UserName,
                Password = _mqSettings.Password,
                DispatchConsumersAsync = true // Important for async consumer event handlers
            };

            _logger.LogInformation("Attempting to connect to RabbitMQ host: {HostName}", _mqSettings.HostName);
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _mqSettings.QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
            // If using an exchange other than default, declare it and bind the queue
            if (!string.IsNullOrEmpty(_mqSettings.ExchangeName) && _mqSettings.ExchangeName != "")
            {
                _channel.ExchangeDeclare(exchange: _mqSettings.ExchangeName, type: ExchangeType.Topic, durable: true); // Or other type as needed
                _channel.QueueBind(queue: _mqSettings.QueueName,
                                   exchange: _mqSettings.ExchangeName,
                                   routingKey: _mqSettings.RoutingKey); // Use a specific routing key or # for all
                 _logger.LogInformation("Queue '{QueueName}' bound to exchange '{ExchangeName}' with routing key '{RoutingKey}'", _mqSettings.QueueName, _mqSettings.ExchangeName, _mqSettings.RoutingKey);
            } else {
                 _logger.LogInformation("Queue '{QueueName}' declared, using default exchange.", _mqSettings.QueueName);
            }


            _logger.LogInformation("Successfully connected to RabbitMQ and channel opened.");
        }

        private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
        {
            OpcDataPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<OpcDataPayload>(message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize OPC data payload: {Message}", message);
                // Message will be NACKed by the caller
                throw; 
            }

            if (payload == null)
            {
                _logger.LogWarning("Deserialized payload is null for message: {Message}", message);
                // Message will be NACKed by the caller
                throw new ArgumentNullException(nameof(payload), "Deserialized payload cannot be null.");
            }

            _logger.LogInformation("Processing OPC Data for TagId: {TagId}, Timestamp: {Timestamp}", payload.TagId, payload.Timestamp);

            // Scope services for processing this message
            using var scope = _serviceProvider.CreateScope();

            // Placeholder: Dispatch to relevant integration services
            // These services (IIoTIntegrationService, etc.) would be defined elsewhere and registered.
            // For now, we just log. Actual dispatch logic would depend on payload content and configuration.

            var iotIntegrationService = scope.ServiceProvider.GetService<IIoTIntegrationService>(); // Assuming IIoTIntegrationService interface
            if (iotIntegrationService != null)
            {
                // Example: await iotIntegrationService.ProcessDataAsync(payload, cancellationToken);
                _logger.LogDebug("Potentially dispatching to IoTIntegrationService.");
            }

            var blockchainIntegrationService = scope.ServiceProvider.GetService<IBlockchainIntegrationService>(); // Assuming IBlockchainIntegrationService interface
            if (blockchainIntegrationService != null /* && IsCritical(payload) */)
            {
                // Example: await blockchainIntegrationService.ProcessDataAsync(payload, cancellationToken);
                _logger.LogDebug("Potentially dispatching to BlockchainIntegrationService.");
            }

            var digitalTwinIntegrationService = scope.ServiceProvider.GetService<IDigitalTwinIntegrationService>(); // Assuming IDigitalTwinIntegrationService interface
            if (digitalTwinIntegrationService != null)
            {
                // Example: await digitalTwinIntegrationService.ProcessDataAsync(payload, cancellationToken);
                _logger.LogDebug("Potentially dispatching to DigitalTwinIntegrationService.");
            }
            
            // Simulate work
            await Task.Delay(10, cancellationToken); 
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OpcDataConsumer is stopping.");
            _channel?.Close();
            _connection?.Close();
            await base.StopAsync(stoppingToken);
        }
    }

    // Placeholder interfaces for services that OpcDataConsumer would dispatch to.
    // These would be defined in their respective service implementation projects/files.
    public interface IIoTIntegrationService { Task ProcessDataAsync(OpcDataPayload data, CancellationToken token); }
    public interface IBlockchainIntegrationService { Task ProcessDataAsync(OpcDataPayload data, CancellationToken token); }
    public interface IDigitalTwinIntegrationService { Task ProcessDataAsync(OpcDataPayload data, CancellationToken token); }
}