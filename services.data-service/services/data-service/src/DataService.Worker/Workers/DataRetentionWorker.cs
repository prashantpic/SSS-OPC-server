using DataService.Domain.Entities;
using DataService.Domain.Repositories;
using DataService.Infrastructure.Persistence.TimeSeries; // For InfluxDbSettings
using Microsoft.Extensions.Options;

namespace DataService.Worker.Workers;

// Placeholder for IUnitOfWork, which would be defined in the Application or Domain layer
public interface IUnitOfWork : IDisposable
{
    IDataRetentionPolicyRepository Policies { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// A background worker that periodically applies data retention policies.
/// It queries for active policies and performs the configured actions (Purge or Archive).
/// Fulfills requirement REQ-DLP-017.
/// </summary>
public class DataRetentionWorker : BackgroundService
{
    private readonly ILogger<DataRetentionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run once a day

    public DataRetentionWorker(IServiceProvider serviceProvider, ILogger<DataRetentionWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Retention Worker starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Data Retention Worker triggered. Next run in {Interval}.", _checkInterval);

            try
            {
                await ApplyRetentionPolicies(stoppingToken);
            }
            catch (Exception ex)
            {
                // Catching exceptions here prevents the worker from crashing on a failed run.
                _logger.LogError(ex, "An unhandled exception occurred during the data retention cycle.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Data Retention Worker stopping.");
    }

    private async Task ApplyRetentionPolicies(CancellationToken stoppingToken)
    {
        // Create a new dependency injection scope for this run to resolve scoped services.
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var timeSeriesRepository = scope.ServiceProvider.GetRequiredService<ITimeSeriesRepository>();
        var influxSettings = scope.ServiceProvider.GetRequiredService<IOptions<InfluxDbSettings>>().Value;
        
        var policies = await unitOfWork.Policies.GetAllAsync(stoppingToken);
        var activePolicies = policies.Where(p => p.IsActive).ToList();

        _logger.LogInformation("Found {Count} active data retention policies to process.", activePolicies.Count);

        foreach (var policy in activePolicies)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            try
            {
                var cutoff = DateTimeOffset.UtcNow.AddDays(-policy.RetentionPeriodDays);
                string? targetBucket = GetBucketForDataType(policy.DataType, influxSettings);

                if (string.IsNullOrEmpty(targetBucket))
                {
                    _logger.LogWarning("No target bucket configured for DataType {DataType}. Skipping policy '{PolicyId}'.", policy.DataType, policy.Id);
                    continue;
                }

                _logger.LogInformation("Applying policy '{PolicyId}' for DataType '{DataType}'. Action: {Action}. Cutoff: {Cutoff}",
                    policy.Id, policy.DataType, policy.Action, cutoff);

                if (policy.Action == RetentionAction.Purge)
                {
                    await timeSeriesRepository.DeleteDataBeforeAsync(targetBucket, cutoff, stoppingToken);
                    _logger.LogInformation("Successfully purged data from bucket '{Bucket}' for policy '{PolicyId}'.", targetBucket, policy.Id);
                }
                else if (policy.Action == RetentionAction.Archive)
                {
                    // Archiving logic would be implemented here.
                    _logger.LogWarning("Archiving is not yet implemented. Policy Action 'Archive' for policy '{PolicyId}' was skipped.", policy.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply retention policy with ID {PolicyId}.", policy.Id);
                // Continue to the next policy
            }
        }
    }

    private string? GetBucketForDataType(DataType dataType, InfluxDbSettings settings)
    {
        return dataType switch
        {
            DataType.Historical => settings.BucketHistorical,
            DataType.Alarm => settings.BucketAlarms,
            // Add other data types as needed
            _ => null
        };
    }
}