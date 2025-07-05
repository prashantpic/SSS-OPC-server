using services.opc.client.Domain.Abstractions;
using services.opc.client.Domain.Models;
using System.Collections.Concurrent;
using Polly;
using Polly.Retry;
using services.opc.client.Infrastructure.OpcProtocolClients.Ua;
using services.opc.client.Infrastructure.OpcProtocolClients.Hda;
using services.opc.client.Infrastructure.OpcProtocolClients.Ac;

namespace services.core_opc_client.Application.Orchestration;

/// <summary>
/// A core service responsible for managing the lifecycle of all configured OPC server connections.
/// It reads connection settings, creates the appropriate protocol client, and handles connection/disconnection.
/// </summary>
public class ConnectionManager
{
    private readonly ILogger<ConnectionManager> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBusPublisher _messageBusPublisher;
    private readonly ConcurrentDictionary<string, IOpcProtocolClient> _activeClients = new();
    private readonly AsyncRetryPolicy _connectionRetryPolicy;
    
    public ConnectionManager(
        ILogger<ConnectionManager> logger, 
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IMessageBusPublisher messageBusPublisher) // In a future step, this would be injected into a DataFlowOrchestrator instead.
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _messageBusPublisher = messageBusPublisher;

        _connectionRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Connection attempt {RetryCount} failed. Retrying in {TimeSpan}...", retryCount, timeSpan);
                });
    }

    /// <summary>
    /// Initializes all OPC connections based on the application configuration.
    /// </summary>
    public async Task InitializeConnectionsAsync()
    {
        var connectionSettings = _configuration.GetSection("OpcConnections").Get<List<OpcConnectionSettings>>();

        if (connectionSettings == null || !connectionSettings.Any())
        {
            _logger.LogWarning("No OPC connections found in configuration.");
            return;
        }

        var initializationTasks = connectionSettings.Select(ConnectWithRetryAsync);
        await Task.WhenAll(initializationTasks);
    }

    /// <summary>
    /// Disconnects all active OPC clients gracefully.
    /// </summary>
    public async Task DisconnectAllAsync()
    {
        _logger.LogInformation("Disconnecting all active OPC clients...");
        var disconnectionTasks = _activeClients.Values.Select(client => client.DisconnectAsync(CancellationToken.None));
        await Task.WhenAll(disconnectionTasks);
        _activeClients.Clear();
        _logger.LogInformation("All clients have been disconnected.");
    }

    private async Task ConnectWithRetryAsync(OpcConnectionSettings settings)
    {
        try
        {
            await _connectionRetryPolicy.ExecuteAsync(async () =>
            {
                var client = CreateClient(settings.Protocol);
                
                // Subscribe to events. In the future, a DataFlowOrchestrator would be the subscriber.
                client.OnDataReceived += HandleDataReceived;
                client.OnAlarmReceived += HandleAlarmReceived;

                await client.ConnectAsync(settings, CancellationToken.None);

                _activeClients.TryAdd(settings.ServerId, client);
                _logger.LogInformation("Successfully connected to OPC Server: {ServerId}", settings.ServerId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to connect to OPC Server {ServerId} after multiple retries.", settings.ServerId);
        }
    }

    /// <summary>
    /// Factory method to create an OPC client based on the protocol string.
    /// This abstracts the selection of the concrete client implementation.
    /// </summary>
    /// <param name="protocol">The protocol identifier (e.g., "UA", "HDA", "AC").</param>
    /// <returns>An instance of <see cref="IOpcProtocolClient"/>.</returns>
    private IOpcProtocolClient CreateClient(string protocol)
    {
        return protocol.ToUpperInvariant() switch
        {
            "UA" => _serviceProvider.GetRequiredService<OpcUaProtocolClient>(),
            "HDA" => _serviceProvider.GetRequiredService<OpcHdaProtocolClient>(),
            "AC" => _serviceProvider.GetRequiredService<OpcAcProtocolClient>(),
            _ => throw new NotSupportedException($"The OPC protocol '{protocol}' is not supported.")
        };
    }

    // NOTE: The following handlers would ideally be in a separate DataFlowOrchestrator class.
    // They are placed here for simplicity in this iteration.
    private void HandleDataReceived(DataPoint dataPoint)
    {
        _logger.LogDebug("Data received: {DataPoint}", dataPoint);
        
        // TODO: Add Edge AI processing logic here if enabled.

        _ = _messageBusPublisher.PublishDataPointAsync(dataPoint);
    }

    private void HandleAlarmReceived(AlarmEvent alarmEvent)
    {
        _logger.LogInformation("Alarm received: {AlarmEvent}", alarmEvent);
        _ = _messageBusPublisher.PublishAlarmAsync(alarmEvent);
    }
}