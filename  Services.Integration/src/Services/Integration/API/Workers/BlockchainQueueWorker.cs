using MediatR;
using Opc.System.Services.Integration.Application.Features.BlockchainLogging.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Services.Integration.API.Workers;

/// <summary>
/// A background service that processes messages from a queue to log data to the blockchain.
/// This decouples the time-consuming process of blockchain logging from the main request thread.
/// </summary>
public class BlockchainQueueWorker : BackgroundService
{
    private readonly ILogger<BlockchainQueueWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BlockchainQueueWorker(ILogger<BlockchainQueueWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Blockchain Queue Worker starting.");

        stoppingToken.Register(() => _logger.LogInformation("Blockchain Queue Worker is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Blockchain Queue Worker waiting for new message...");
            
            // In a real-world application, this section would use a message bus client (e.g., RabbitMQ, Azure Service Bus)
            // to dequeue a message. For this example, we will simulate receiving a message.
            // var message = await messageBusClient.ReceiveAsync(stoppingToken);
            // if (message != null) { ... }
            
            // --- Start Simulation ---
            await Task.Delay(15000, stoppingToken); // Simulate waiting for a message
            var simulatedCommand = new LogCriticalDataCommand(
                ConnectionId: Guid.Parse("08d9b1c2-1e9d-4e4f-8e1a-9a8b7c6d5e4f"), // Example connection ID
                DataPayload: $"{{\"tag\":\"Sensor.Temperature\",\"value\":98.6,\"timestamp\":\"{DateTime.UtcNow:O}\"}}",
                Metadata: "Simulated critical event from worker."
            );
            // --- End Simulation ---
            
            _logger.LogInformation("Received critical event message. Processing payload for Connection ID: {ConnectionId}", simulatedCommand.ConnectionId);

            try
            {
                // Create a new DI scope to resolve scoped services like MediatR and its handlers
                // This is crucial because a BackgroundService is a singleton.
                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var transactionId = await mediator.Send(simulatedCommand, stoppingToken);
                    _logger.LogInformation("Successfully processed blockchain log command. Transaction ID: {TransactionId}", transactionId);
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected when the service is stopping.
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing a message in the Blockchain Queue Worker.");
                // In a real implementation, you would decide whether to requeue the message or move it to a dead-letter queue.
            }
        }
        
        _logger.LogInformation("Blockchain Queue Worker has stopped.");
    }
}