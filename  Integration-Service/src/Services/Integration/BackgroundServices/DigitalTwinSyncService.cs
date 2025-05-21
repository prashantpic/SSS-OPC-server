namespace IntegrationService.BackgroundServices
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationService.Configuration;
    using IntegrationService.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DigitalTwinSyncService : IHostedService, IDisposable
    {
        private readonly ILogger<DigitalTwinSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<DigitalTwinSettings> _digitalTwinSettingsMonitor;
        private readonly ConcurrentDictionary<string, PeriodicTimer> _platformTimers = new();
        private readonly ConcurrentDictionary<string, Task> _platformSyncTasks = new();
        private CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public DigitalTwinSyncService(
            ILogger<DigitalTwinSyncService> logger,
            IServiceProvider serviceProvider,
            IOptionsMonitor<DigitalTwinSettings> digitalTwinSettingsMonitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _digitalTwinSettingsMonitor = digitalTwinSettingsMonitor ?? throw new ArgumentNullException(nameof(digitalTwinSettingsMonitor));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Digital Twin Sync Service is starting.");
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            _digitalTwinSettingsMonitor.OnChange(async (settings, _) => await ConfigureTimersAsync(settings, _stoppingCts.Token));
            
            // Initial configuration
            _ = ConfigureTimersAsync(_digitalTwinSettingsMonitor.CurrentValue, _stoppingCts.Token);

            return Task.CompletedTask;
        }

        private async Task ConfigureTimersAsync(DigitalTwinSettings settings, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reconfiguring Digital Twin Sync Service timers based on new settings.");
            var activePlatformIds = settings.Platforms.Where(p => p.IsEnabled).Select(p => p.Id).ToList();

            // Stop and remove timers for platforms no longer active or removed
            foreach (var existingPlatformId in _platformTimers.Keys.ToList())
            {
                if (!activePlatformIds.Contains(existingPlatformId))
                {
                    if (_platformTimers.TryRemove(existingPlatformId, out var timer))
                    {
                        timer.Dispose();
                        _logger.LogInformation("Stopped sync timer for Digital Twin platform ID: {PlatformId}", existingPlatformId);
                    }
                    // If there's an associated task, it should naturally complete or be cancelled by _stoppingCts
                     _platformSyncTasks.TryRemove(existingPlatformId, out _);
                }
            }

            // Add or update timers for active platforms
            foreach (var platformConfig in settings.Platforms.Where(p => p.IsEnabled))
            {
                if (cancellationToken.IsCancellationRequested) break;

                if (_platformTimers.TryGetValue(platformConfig.Id, out var existingTimer))
                {
                    // If frequency changed, need to dispose and recreate. PeriodicTimer period is immutable.
                    // For simplicity, we'll recreate. A more complex solution could compare frequencies.
                    existingTimer.Dispose();
                     _platformSyncTasks.TryRemove(platformConfig.Id, out _); // Remove old task
                }

                var syncInterval = TimeSpan.FromSeconds(platformConfig.SyncFrequencySeconds > 0 ? platformConfig.SyncFrequencySeconds : 300);
                var newTimer = new PeriodicTimer(syncInterval);
                _platformTimers[platformConfig.Id] = newTimer;

                _logger.LogInformation("Starting sync timer for Digital Twin platform ID: {PlatformId} with interval: {SyncInterval}", platformConfig.Id, syncInterval);
                
                // Start a new task for this timer
                var syncTask = Task.Run(async () => await SyncLoopAsync(platformConfig, newTimer, _stoppingCts.Token), cancellationToken);
                _platformSyncTasks[platformConfig.Id] = syncTask;
            }
        }
        
        private async Task SyncLoopAsync(DigitalTwinConfig platformConfig, PeriodicTimer timer, CancellationToken cancellationToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Timer elapsed for Digital Twin platform: {PlatformId}. Triggering synchronization.", platformConfig.Id);
                    await PerformSyncForPlatformAsync(platformConfig, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Sync loop for Digital Twin platform {PlatformId} was canceled.", platformConfig.Id);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Unhandled exception in sync loop for Digital Twin platform {PlatformId}.", platformConfig.Id);
            }
            finally
            {
                _logger.LogInformation("Sync loop for Digital Twin platform {PlatformId} is stopping.", platformConfig.Id);
            }
        }


        private async Task PerformSyncForPlatformAsync(DigitalTwinConfig platformConfig, CancellationToken cancellationToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var digitalTwinIntegrationService = scope.ServiceProvider.GetRequiredService<DigitalTwinIntegrationService>();
                    
                    // In a real scenario, you'd fetch the relevant twin instances and their data for this platform.
                    // For example, from a configuration or REPO-DATA-SERVICE.
                    // List<string> twinInstanceIdsToSync = GetTwinInstancesForPlatform(platformConfig.Id);
                    // foreach (var twinInstanceId in twinInstanceIdsToSync)
                    // {
                    //    if (cancellationToken.IsCancellationRequested) break;
                    //    object currentDataForTwin = await FetchDataForTwinInstanceAsync(twinInstanceId, cancellationToken);
                    //    await digitalTwinIntegrationService.SyncDataForTwinAsync(platformConfig.Id, twinInstanceId, currentDataForTwin, cancellationToken);
                    // }

                    // Simplified: Assume a placeholder method or if the service handles finding twins.
                    // This example assumes the DigitalTwinIntegrationService itself knows which twins to sync for a platform,
                    // or SyncDataForTwinAsync is called with specific data.
                    // For now, let's assume we need to get a list of specific twins from config or another source.
                    // As a placeholder, let's imagine the platformConfig itself might hint at a specific twin or a "sync all" approach
                    _logger.LogDebug("Performing sync for platform {PlatformId}. Specific twin data fetching logic needs implementation.", platformConfig.Id);
                    
                    // Example: if platformConfig.DigitalTwinModelId refers to a specific twin to sync or a group
                    // For this example, we are not syncing specific data, as it's not provided.
                    // The `DigitalTwinIntegrationService.SyncDataForTwinAsync` would need the data.
                    // This background service is just the trigger.
                    // Perhaps the integration service is changed to `SyncAllDataForPlatformAsync(string platformId)`
                    // await digitalTwinIntegrationService.SyncAllDataForPlatformAsync(platformConfig.Id, cancellationToken);

                    _logger.LogInformation("Synchronization triggered for platform {PlatformId}. DigitalTwinIntegrationService will handle the details.", platformConfig.Id);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled Digital Twin synchronization for platform {PlatformId}.", platformConfig.Id);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Digital Twin Sync Service is stopping.");
            if (_stoppingCts != null && !_stoppingCts.IsCancellationRequested)
            {
                _stoppingCts.Cancel();
            }

            // Dispose all timers
            foreach (var timerEntry in _platformTimers)
            {
                timerEntry.Value.Dispose();
            }
            _platformTimers.Clear();

            // Wait for all sync tasks to complete or be cancelled
            var allTasks = _platformSyncTasks.Values.ToList();
            if (allTasks.Any())
            {
                 await Task.WhenAll(allTasks).ConfigureAwait(false);
            }
            _platformSyncTasks.Clear();

            _logger.LogInformation("Digital Twin Sync Service has stopped.");
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing DigitalTwinSyncService.");
            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();
            _stoppingCts = null!;

            foreach (var timerEntry in _platformTimers)
            {
                timerEntry.Value.Dispose();
            }
            _platformTimers.Clear();
            _platformSyncTasks.Clear(); // Tasks should have completed or cancelled.
        }
    }
}