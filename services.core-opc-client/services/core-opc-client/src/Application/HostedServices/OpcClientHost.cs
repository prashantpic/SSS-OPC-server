using services.core_opc_client.Application.Orchestration;

namespace services.core_opc_client.Application.HostedServices;

/// <summary>
/// The main long-running background service that controls the application's lifecycle.
/// It integrates with the .NET IHostedService framework.
/// </summary>
public class OpcClientHost : IHostedService
{
    private readonly ILogger<OpcClientHost> _logger;
    private readonly ConnectionManager _connectionManager;

    public OpcClientHost(ILogger<OpcClientHost> logger, ConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// This method initiates the connection process for all configured OPC servers.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Core OPC Client Host is starting.");
        
        // The application lifetime's started event ensures the host is fully configured
        // before we start our long-running tasks.
        await _connectionManager.InitializeConnectionsAsync();
        
        _logger.LogInformation("Core OPC Client Host has started and connections are being established.");
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// This method ensures a clean disconnection from all OPC servers.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Core OPC Client Host is stopping.");

        await _connectionManager.DisconnectAllAsync();

        _logger.LogInformation("Core OPC Client Host has stopped gracefully.");
    }
}