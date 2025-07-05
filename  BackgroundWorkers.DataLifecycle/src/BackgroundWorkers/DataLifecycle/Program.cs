using Opc.System.BackgroundWorkers.DataLifecycle.Application.Factories;
using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Application.Strategies;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Archiving;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Logging;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Persistence;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Purging;
using Opc.System.BackgroundWorkers.DataLifecycle.Scheduling;
using Quartz;
using Serilog;

// This is a placeholder for the actual repository from DataService.Core
// It allows the project to compile and run for demonstration purposes.
namespace Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Persistence
{
    public interface ISourceDataRepository<T> { }
    public class HistoricalDataRepository : ISourceDataRepository<object> { }
    public class AlarmEventRepository : ISourceDataRepository<object> { }
    public class DataRetentionPolicyRepository : IDataRetentionPolicyRepository
    {
        public Task<IEnumerable<DataRetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken)
        {
            // In a real scenario, this would query a database.
            // For this worker, we return a hardcoded list for testing.
            var policies = new List<DataRetentionPolicy>
            {
                new() {
                    PolicyId = Guid.NewGuid(),
                    DataType = Domain.Enums.DataType.Historical,
                    RetentionPeriodDays = 30,
                    ArchiveLocation = "some-archive-container",
                    IsActive = true
                },
                new() {
                    PolicyId = Guid.NewGuid(),
                    DataType = Domain.Enums.DataType.Alarm,
                    RetentionPeriodDays = 90,
                    ArchiveLocation = null, // Purge only
                    IsActive = true
                },
                 new() {
                    PolicyId = Guid.NewGuid(),
                    DataType = Domain.Enums.DataType.Audit,
                    RetentionPeriodDays = 365,
                    ArchiveLocation = "audit-archive-container",
                    IsActive = true
                },
                new() {
                    PolicyId = Guid.NewGuid(),
                    DataType = Domain.Enums.DataType.AI,
                    RetentionPeriodDays = 180,
                    IsActive = false // This one should be ignored
                }
            };
            return Task.FromResult<IEnumerable<DataRetentionPolicy>>(policies);
        }
    }
}


var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Hosting & Scheduling
        services.AddQuartz(q => QuartzStartup.Configure(q, hostContext.Configuration));
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Application Services & Factories
        services.AddSingleton<IDataLifecycleStrategyFactory, DataLifecycleStrategyFactory>();
        services.AddSingleton<IPolicyProvider, PolicyProvider>();
        
        // Placeholder implementations for core data operations
        // In a real system, these would have dependencies on cloud SDKs and data repositories
        services.AddSingleton<IArchiver, BlobStorageArchiver>(); 
        services.AddSingleton<IPurger, DatabasePurger>();
        services.AddSingleton<IAuditLogger, AuditLogger>();

        // Strategies (registered as a collection for the factory)
        services.AddSingleton<IDataLifecycleStrategy, HistoricalDataLifecycleStrategy>();
        // Future strategies would be added here
        // services.AddSingleton<IDataLifecycleStrategy, AlarmDataLifecycleStrategy>();
        // services.AddSingleton<IDataLifecycleStrategy, AuditDataLifecycleStrategy>();
        // services.AddSingleton<IDataLifecycleStrategy, AiDataLifecycleStrategy>(); 

        // Infrastructure / Data Access
        // These are assumed to come from a shared DataService project.
        // Using placeholder implementations for this standalone service.
        services.AddScoped<IDataRetentionPolicyRepository, DataRetentionPolicyRepository>();
        services.AddScoped<ISourceDataRepository<object>, HistoricalDataRepository>(); // Placeholder for generic type
        // services.AddScoped<ISourceDataRepository<AlarmEvent>, AlarmEventRepository>();
        
        // Cloud clients would be registered here based on configuration
        // e.g., if (config["ArchiveStorage:Type"] == "AzureBlob") { ... }
    })
    .Build();

host.Run();