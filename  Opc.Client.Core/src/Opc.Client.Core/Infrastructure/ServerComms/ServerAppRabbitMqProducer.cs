using Opc.Client.Core.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Opc.Client.Core.Infrastructure.ServerComms;

/// <summary>
/// A concrete implementation of IServerEventPublisher that sends messages to a RabbitMQ message broker.
/// </summary>
/// <remarks>
/// This class handles the connection to RabbitMQ, serialization of event DTOs to JSON,
/// and publishing messages to the appropriate exchanges and routing keys.
/// </remarks>
public class ServerAppRabbitMqProducer : IServerEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _dataExchange = "opc.data.exchange";
    private readonly string _systemExchange = "opc.system.exchange";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerAppRabbitMqProducer"/> class.
    /// </summary>
    /// <param name="connectionFactory">The RabbitMQ connection factory configured with the broker details.</param>
    public ServerAppRabbitMqProducer(ConnectionFactory connectionFactory)
    {
        // In a real app, use Polly for connection resilience.
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchanges to ensure they exist
        _channel.ExchangeDeclare(exchange: _dataExchange, type: ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare(exchange: _systemExchange, type: ExchangeType.Topic, durable: true);
    }

    /// <inheritdoc/>
    public Task PublishDataChangeAsync(DataChangeNotification notification)
    {
        var routingKey = $"data.change.{notification.ServerId}";
        return PublishAsync(_dataExchange, routingKey, notification);
    }

    /// <inheritdoc/>
    public Task PublishAlarmAsync(AlarmEventNotification notification)
    {
        var routingKey = $"alarm.{notification.ServerId}.severity.{notification.Severity}";
        return PublishAsync(_dataExchange, routingKey, notification);
    }

    /// <inheritdoc/>
    public Task PublishHealthStatusAsync(ClientHealthStatus status)
    {
        var routingKey = $"health.{status.ClientId}";
        return PublishAsync(_systemExchange, routingKey, status);
    }

    /// <inheritdoc/>
    public Task PublishCriticalWriteLogAsync(CriticalWriteLog log)
    {
        var routingKey = $"audit.write.critical.{log.ServerId}";
        return PublishAsync(_systemExchange, routingKey, log);
    }
    
    private Task PublishAsync<T>(string exchange, string routingKey, T message) where T : class
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);
                
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Failed to publish message: {ex.Message}");
            return Task.FromException(ex);
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}