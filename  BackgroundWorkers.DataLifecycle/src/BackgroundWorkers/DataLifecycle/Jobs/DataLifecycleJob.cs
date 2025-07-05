using Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories;
using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Quartz;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Jobs;

/// <summary>
/// The core Quartz.NET job that executes the data lifecycle management process.
/// It is triggered by the scheduler and orchestrates the application of retention policies.
/// This job prevents concurrent execution to ensure one lifecycle process runs at a time.
/// </summary>
[DisallowConcurrentExecution]
public sealed class DataLifecycleJob : IJob
{
    private readonly IPolicyProvider _policyProvider;
    private readonly IDataLifecycleStrategyFactory _strategyFactory;
    private readonly ILogger<DataLifecycleJob> _logger;

    public DataLifecycleJob(
        IPolicyProvider policyProvider,
        IDataLifecycleStrategyFactory strategyFactory,
        ILogger<DataLifecycleJob> logger)
    {
        _policyProvider = policyProvider;
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    /// <summary>
    /// Main execution logic for the periodic data lifecycle check.
    /// It iterates through all active policies and delegates the work to the appropriate strategy.
    /// </summary>
    /// <param name="context">The job execution context provided by Quartz.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Data Lifecycle Job starting run at {RunTime}", DateTimeOffset.UtcNow);

        try
        {
            var policies = await _policyProvider.GetActivePoliciesAsync(context.CancellationToken);

            if (!policies.Any())
            {
                _logger.LogInformation("No active data retention policies found. Job exiting.");
                return;
            }
            
            _logger.LogInformation("Found {PolicyCount} active policies to process.", policies.Count());

            foreach (var policy in policies)
            {
                try
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    _logger.LogInformation("Processing policy '{PolicyId}' for data type '{DataType}'.", policy.PolicyId, policy.DataType);

                    var strategy = _strategyFactory.GetStrategy(policy.DataType);
                    await strategy.ExecuteAsync(policy, context.CancellationToken);
                    
                    _logger.LogInformation("Successfully processed policy '{PolicyId}'.", policy.PolicyId);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Data Lifecycle Job was canceled during policy processing.");
                    break;
                }
                catch (NotSupportedException ex)
                {
                    _logger.LogWarning(ex, "Could not process policy '{PolicyId}'. No strategy found for data type '{DataType}'.", policy.PolicyId, policy.DataType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred while processing policy '{PolicyId}'.", policy.PolicyId);
                    // Continue to the next policy
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Data Lifecycle Job run was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "A fatal error occurred during the Data Lifecycle Job orchestration.");
        }
        finally
        {
            _logger.LogInformation("Data Lifecycle Job run finished at {FinishTime}", DateTimeOffset.UtcNow);
        }
    }
}