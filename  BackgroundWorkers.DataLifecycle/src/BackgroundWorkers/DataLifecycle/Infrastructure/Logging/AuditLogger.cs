using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Logging;

/// <summary>
/// Placeholder implementation of IAuditLogger. In a production system, this class
/// would be responsible for sending a structured audit event to a central system,
/// likely via a message queue (e.g., RabbitMQ, Kafka) or a dedicated HTTP endpoint.
/// For this implementation, it logs the audit event to the standard logger.
/// </summary>
public sealed class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(ILogger<AuditLogger> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task LogDataLifecycleEventAsync(
        string action,
        DataType dataType,
        bool success,
        long recordsAffected,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        // This simulates sending an audit event. The structured logging format
        // makes it easy for log collectors (like Fluentd, Logstash) to parse and
        // forward these specific events to a dedicated audit sink.
        _logger.LogInformation(
            "AUDIT EVENT: Action='{Action}', DataType='{DataType}', Success='{Success}', RecordsAffected='{RecordsAffected}', Details='{Details}'",
            action,
            dataType,
            success,
            recordsAffected,
            details ?? "N/A");

        return Task.CompletedTask;
    }
}