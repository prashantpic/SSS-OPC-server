using services.opc.client.Domain.Abstractions;
using services.opc.client.Domain.Models;

namespace services.opc.client.Infrastructure.OpcProtocolClients.Ac;

/// <summary>
/// Concrete implementation for OPC Alarms & Conditions (A&C).
/// This class connects to OPC A&C servers, subscribes to alarm and event notifications.
/// NOTE: This is a stub implementation.
/// </summary>
public class OpcAcProtocolClient : IOpcProtocolClient
{
    private readonly ILogger<OpcAcProtocolClient> _logger;

    public event Action<DataPoint>? OnDataReceived;
    public event Action<AlarmEvent>? OnAlarmReceived;

    public OpcAcProtocolClient(ILogger<OpcAcProtocolClient> logger)
    {
        _logger = logger;
        _logger.LogWarning("OPC A&C Client is a stub and not fully implemented.");
    }
    
    /// <inheritdoc/>
    public Task ConnectAsync(OpcConnectionSettings settings, CancellationToken cancellationToken)
    {
        _logger.LogWarning("ConnectAsync for OPC A&C is not implemented. // TODO: Implement using a third-party OPC Classic A&C library.");
        // In a real implementation, this would connect to the DCOM server and set up event sinks.
        // For demonstration, we can raise a dummy alarm event.
        OnAlarmReceived?.Invoke(new AlarmEvent("Stub.Source", "Stub.Condition", "A&C Client Connected (Stub)", 100, DateTime.UtcNow, false));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("DisconnectAsync for OPC A&C is not implemented.");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<DataPoint>> ReadAsync(IEnumerable<string> tagIds, CancellationToken cancellationToken)
    {
        _logger.LogError("ReadAsync is not supported for OPC A&C protocol.");
        throw new NotSupportedException("The ReadAsync operation is not applicable to an OPC A&C client.");
    }
    
    /// <inheritdoc/>
    public Task WriteAsync(string tagId, object value, CancellationToken cancellationToken)
    {
        _logger.LogError("WriteAsync is not supported for OPC A&C protocol.");
        throw new NotSupportedException("The WriteAsync operation is not applicable to an OPC A&C client.");
    }

    /// <summary>
    /// Acknowledges a specific alarm. This is a specialized method for A&C.
    /// </summary>
    /// <param name="request">The request containing details of the alarm to acknowledge.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AcknowledgeAlarmAsync(object request)
    {
        _logger.LogWarning("AcknowledgeAlarmAsync for OPC A&C is not implemented. // TODO: Implement using a third-party OPC Classic A&C library.");
        throw new NotImplementedException("This method requires a third-party library for OPC Classic A&C communication.");
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}