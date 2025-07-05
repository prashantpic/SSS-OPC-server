using MediatR;
using System.Text.Json;
using DataService.Application.Features.HistoricalData.Commands;

namespace DataService.Api.Messaging;

// Placeholder for a real message bus client abstraction
public interface IMessageBusConsumer
{
    Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived, CancellationToken cancellationToken);
}


/// <summary>
/// A background service that consumes data from a message queue for asynchronous ingestion.
/// Listens for historical data and alarms to decouple the ingestion process.
/// Fulfills requirement REQ-DLP-001.
/// </summary>
public class DataIngestionConsumer : BackgroundService
{
    private readonly ILogger<DataIngestionConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    // In a real implementation, this would be a specific client (e.g., for RabbitMQ or Kafka)
    private readonly IMessageBusConsumer _messageBusConsumer;

    public DataIngestionConsumer(
        ILogger<DataIngestionConsumer> logger,
        IServiceProvider serviceProvider,
        IMessageBusConsumer messageBusConsumer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _messageBusConsumer = messageBusConsumer;
    }

    /// <summary>
    /// This method is called when the IHostedService starts.
    /// It subscribes to relevant message bus topics.
    /// </summary>
    /// <param name="stoppingToken">Triggered when the service is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Ingestion Consumer is starting.");

        stoppingToken.Register(() => _logger.LogInformation("Data Ingestion Consumer is stopping."));

        await _messageBusConsumer.SubscribeAsync(
            topic: "historical-data-topic",
            onMessageReceived: HandleHistoricalDataMessage,
            cancellationToken: stoppingToken);
            
        // Can add more subscriptions here for other topics like alarms or audits
    }

    private async Task HandleHistoricalDataMessage(string messagePayload)
    {
        _logger.LogDebug("Received historical data message. Payload: {Payload}", messagePayload);

        try
        {
            var command = JsonSerializer.Deserialize<StoreHistoricalDataBatchCommand>(messagePayload);

            if (command == null || !command.DataPoints.Any())
            {
                _logger.LogWarning("Deserialized command is null or has no data points. Ignoring message.");
                return; // Acknowledge message as processed
            }

            // Create a new scope to resolve scoped services like MediatR
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(command);
            }

            _logger.LogInformation("Successfully processed and stored a batch of {Count} historical data points.", command.DataPoints.Count());
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize historical data message. Payload: {Payload}", messagePayload);
            // In a real system, this message would be sent to a dead-letter queue.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while processing historical data message.");
            // In a real system, we might nack (not acknowledge) the message to retry.
            throw; // Re-throwing might cause the consumer to crash and restart, depending on bus implementation
        }
    }
}