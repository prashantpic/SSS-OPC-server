using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Configuration;
using IntegrationService.Services;
using System.Collections.Concurrent; // Added for ConcurrentDictionary

namespace IntegrationService.BackgroundServices
{
    /// <summary>
    /// Hosted service for periodically synchronizing data with Digital Twin platforms.
    /// </summary>
    public class DigitalTwinSyncService : BackgroundService
    {
        private readonly ILogger<DigitalTwinSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IntegrationSettings _settings;
        private readonly bool _isEnabled;
        private readonly ConcurrentDictionary<string, Timer> _twinTimers = new ConcurrentDictionary<string, Timer>();
        private readonly ConcurrentDictionary<string, DateTimeOffset> _lastSyncTimes = new ConcurrentDictionary<string, DateTimeOffset>();


        public DigitalTwinSyncService(
            ILogger<DigitalTwinSyncService> logger,
            IServiceProvider serviceProvider,
            IOptions<IntegrationSettings> settingsAccessor,
             IOptions<FeatureFlags> featureFlagsAccessor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _settings = settingsAccessor.Value;
            _isEnabled = featureFlagsAccessor.Value.EnableDigitalTwinSync;

            if (_isEnabled)
            {
                _logger.LogInformation("DigitalTwinSyncService initialized and enabled.");
            } else {
                 _logger.LogInformation("DigitalTwinSyncService is disabled by feature flag.");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_isEnabled)
            {
                _logger.LogInformation("DigitalTwinSyncService is disabled, not starting sync timers.");
                return Task.CompletedTask;
            }

            _logger.LogInformation("DigitalTwinSyncService background service starting.");
            stoppingToken.Register(() => _logger.LogInformation("DigitalTwinSyncService background service is stopping."));

            foreach (var twinConfig in _settings.DigitalTwinSettings.Twins)
            {
                if (twinConfig.SyncFrequencySeconds > 0)
                {
                    var timer = new Timer(
                        async (_) => await TriggerSynchronizationAsync(twinConfig.Id, stoppingToken),
                        null,
                        TimeSpan.Zero, // Start immediately for the first sync
                        TimeSpan.FromSeconds(twinConfig.SyncFrequencySeconds)
                    );
                    _twinTimers[twinConfig.Id] = timer;
                    _logger.LogInformation("Scheduled sync for Digital Twin '{TwinId}' every {Frequency} seconds.", twinConfig.Id, twinConfig.SyncFrequencySeconds);
                }
                else
                {
                    _logger.LogInformation("Sync frequency for Digital Twin '{TwinId}' is not configured or is <= 0. Sync will not be scheduled.", twinConfig.Id);
                }
            }
            return Task.CompletedTask;
        }

        private async Task TriggerSynchronizationAsync(string twinId, CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sync trigger for twin '{TwinId}' cancelled.", twinId);
                return;
            }

            // Check if another sync for this twin is already in progress (simple lock)
            // This is a basic check; a more robust solution might use SemaphoreSlim.
            // For simplicity with Timer, we rely on the service scope.

            _logger.LogInformation("Attempting synchronization for Digital Twin '{TwinId}'.", twinId);

            try
            {
                // Resolve DigitalTwinIntegrationService within a scope for each sync operation
                using (var scope = _serviceProvider.CreateScope())
                {
                    var digitalTwinService = scope.ServiceProvider.GetRequiredService<DigitalTwinIntegrationService>();
                    await digitalTwinService.SynchronizeTwinDataAsync(twinId);
                } // Scope is disposed here
                _lastSyncTimes[twinId] = DateTimeOffset.UtcNow; // Record successful sync time
                _logger.LogInformation("Synchronization completed for Digital Twin '{TwinId}'.", twinId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Digital Twin sync for '{TwinId}' was cancelled during execution.", twinId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during synchronization for Digital Twin '{TwinId}'.", twinId);
                // Optionally, implement backoff for this specific twin's timer if errors persist
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DigitalTwinSyncService background service is stopping.");
            foreach (var timerEntry in _twinTimers)
            {
                timerEntry.Value.Change(Timeout.Infinite, 0); // Stop the timer
                _logger.LogDebug("Stopped timer for Digital Twin '{TwinId}'.", timerEntry.Key);
            }
            await base.StopAsync(stoppingToken);
            _logger.LogInformation("DigitalTwinSyncService background service stopped.");
        }

        public override void Dispose()
        {
            _logger.LogInformation("DigitalTwinSyncService disposing.");
            foreach (var timerEntry in _twinTimers)
            {
                timerEntry.Value.Dispose();
            }
            _twinTimers.Clear();
            base.Dispose();
             GC.SuppressFinalize(this);
        }
    }
}