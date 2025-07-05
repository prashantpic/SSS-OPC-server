using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Strategies;

/// <summary>
/// The specific strategy implementation for managing the lifecycle of historical process data.
/// Handles archiving and purging based on the policy.
/// </summary>
public sealed class HistoricalDataLifecycleStrategy : IDataLifecycleStrategy
{
    private readonly IArchiver _archiver;
    private readonly IPurger _purger;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<HistoricalDataLifecycleStrategy> _logger;

    /// <inheritdoc/>
    public DataType DataType => DataType.Historical;

    public HistoricalDataLifecycleStrategy(
        IArchiver archiver,
        IPurger purger,
        IAuditLogger auditLogger,
        ILogger<HistoricalDataLifecycleStrategy> logger)
    {
        _archiver = archiver;
        _purger = purger;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(DataRetentionPolicy policy, CancellationToken cancellationToken)
    {
        var retentionThreshold = DateTimeOffset.UtcNow.AddDays(-policy.RetentionPeriodDays);
        _logger.LogInformation("Executing strategy for '{DataType}' with a retention threshold of {Threshold}.",
            DataType, retentionThreshold);

        // 1. Archival process (if configured)
        if (!string.IsNullOrWhiteSpace(policy.ArchiveLocation))
        {
            await ExecuteArchivalAsync(policy, retentionThreshold, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Archival is not configured for this policy. Skipping archival step.");
        }

        // 2. Purging process
        await ExecutePurgingAsync(retentionThreshold, cancellationToken);
    }

    private async Task ExecuteArchivalAsync(DataRetentionPolicy policy, DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting archival process for data older than {Threshold} to '{Location}'.",
            threshold, policy.ArchiveLocation);

        ArchiveResult result;
        try
        {
            result = await _archiver.ArchiveAsync(DataType, threshold, policy.ArchiveLocation!, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Archival completed successfully. Archived {Count} records.", result.RecordsArchived);
            }
            else
            {
                _logger.LogWarning("Archival process finished with a non-success status: {Details}", result.Details);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during the archival process.");
            result = new ArchiveResult(false, 0, $"Archival failed with exception: {ex.Message}");
        }

        await _auditLogger.LogDataLifecycleEventAsync(
            action: "Archive",
            dataType: DataType,
            success: result.Success,
            recordsAffected: result.RecordsArchived,
            details: result.Details,
            cancellationToken: cancellationToken);
    }

    private async Task ExecutePurgingAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting purge process for data older than {Threshold}.", threshold);

        PurgeResult result;
        try
        {
            result = await _purger.PurgeAsync(DataType, threshold, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Purge completed successfully. Purged {Count} records.", result.RecordsPurged);
            }
            else
            {
                _logger.LogWarning("Purge process finished with a non-success status: {Details}", result.Details);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during the purge process.");
            result = new PurgeResult(false, 0, $"Purge failed with exception: {ex.Message}");
        }

        await _auditLogger.LogDataLifecycleEventAsync(
            action: "Purge",
            dataType: DataType,
            success: result.Success,
            recordsAffected: result.RecordsPurged,
            details: result.Details,
            cancellationToken: cancellationToken);
    }
}