using Opc.System.BackgroundWorkers.DataLifecycle.Jobs;
using Quartz;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Scheduling;

/// <summary>
/// Configures the Quartz.NET scheduler, defining jobs and their triggers.
/// Encapsulates the logic for adding the DataLifecycleJob and setting its schedule
/// based on the CRON expression from appsettings.json.
/// </summary>
public static class QuartzStartup
{
    /// <summary>
    /// Centralizes the setup of all scheduled tasks within the application.
    /// </summary>
    /// <param name="q">The Quartz configurator.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void Configure(IServiceCollectionQuartzConfigurator q, IConfiguration configuration)
    {
        var jobKey = new JobKey(nameof(DataLifecycleJob));

        // Read the CRON expression from configuration, with a fallback default.
        var cronExpression = configuration["Scheduler:DataLifecycleJobCronExpression"];
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            // Default: Run at 2:00 AM every day
            cronExpression = "0 0 2 * * ?";
        }

        // Add the job to the scheduler, making it durable so it can be triggered on demand
        // and marking it as non-concurrent.
        q.AddJob<DataLifecycleJob>(opts => opts
            .WithIdentity(jobKey)
            .StoreDurably()
            .DisallowConcurrentExecution());

        // Create a trigger for the job using the configured CRON schedule.
        q.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{nameof(DataLifecycleJob)}-trigger")
            .WithCronSchedule(cronExpression));
    }
}