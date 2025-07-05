using services.opc.client.Domain.Abstractions;
using services.opc.client.Domain.Models;

namespace services.opc.client.Infrastructure.OpcProtocolClients.Hda;

/// <summary>
/// Concrete implementation for OPC Historical Data Access (HDA).
/// This class is responsible for connecting to OPC HDA servers and executing historical data queries.
/// NOTE: This is a stub implementation.
/// </summary>
public class OpcHdaProtocolClient : IOpcProtocolClient
{
    private readonly ILogger<OpcHdaProtocolClient> _logger;

    public event Action<DataPoint>? OnDataReceived;
    public event Action<AlarmEvent>? OnAlarmReceived;

    public OpcHdaProtocolClient(ILogger<OpcHdaProtocolClient> logger)
    {
        _logger = logger;
        _logger.LogWarning("OPC HDA Client is a stub and not fully implemented.");
    }
    
    /// <inheritdoc/>
    public Task ConnectAsync(OpcConnectionSettings settings, CancellationToken cancellationToken)
    {
        _logger.LogWarning("ConnectAsync for OPC HDA is not implemented. // TODO: Implement using a third-party OPC Classic HDA library.");
        // In a real implementation, this would connect to the DCOM server.
        // For now, we'll consider it "connected" for stub purposes.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("DisconnectAsync for OPC HDA is not implemented.");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<DataPoint>> ReadAsync(IEnumerable<string> tagIds, CancellationToken cancellationToken)
    {
        _logger.LogError("ReadAsync is not supported for OPC HDA. Use a specific historical query method instead.");
        throw new NotImplementedException("ReadAsync is not supported for OPC HDA protocol. Use a specific historical query method.");
    }

    /// <inheritdoc/>
    public Task WriteAsync(string tagId, object value, CancellationToken cancellationToken)
    {
        _logger.LogError("WriteAsync is not supported for OPC HDA protocol.");
        throw new NotImplementedException("WriteAsync is not supported for OPC HDA protocol.");
    }

    /// <summary>
    /// Queries historical data from the server. This is a specialized method for HDA.
    /// </summary>
    /// <param name="request">The historical data request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of historical data points.</returns>
    public Task<IEnumerable<DataPoint>> QueryHistoricalDataAsync(object request, CancellationToken cancellationToken)
    {
        _logger.LogWarning("QueryHistoricalDataAsync for OPC HDA is not implemented. // TODO: Implement using a third-party OPC Classic HDA library.");
        throw new NotImplementedException("This method requires a third-party library for OPC Classic HDA communication.");
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}