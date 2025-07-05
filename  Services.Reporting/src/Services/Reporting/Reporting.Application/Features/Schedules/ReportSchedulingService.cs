using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.System.Services.Reporting.Domain.Abstractions;
using Quartz;

namespace Opc.System.Services.Reporting.Application.Features.Schedules;

/// <summary>
/// A long-running background service that initializes and manages the job scheduler,
/// ensuring that scheduled reports are triggered at their configured times.
/// To load all active report schedules from the database and configure them with a scheduling library like Quartz.NET.
/// </summary>
public class ReportSchedulingService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IReportScheduleRepository _reportScheduleRepository;
    private readonly ILogger<ReportSchedulingService> _logger;
    private IScheduler? _scheduler;

    public ReportSchedulingService(
        ISchedulerFactory schedulerFactory,
        IReportScheduleRepository reportScheduleRepository,
        ILogger<ReportSchedulingService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _reportScheduleRepository = reportScheduleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Report Scheduling Service is starting.");
        _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        await _scheduler.Start(cancellationToken);

        var schedules = await _reportScheduleRepository.GetAllEnabledAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            try
            {
                var jobKey = new JobKey(schedule.Id.ToString(), "ReportGeneration");
                if (await _scheduler.CheckExists(jobKey, cancellationToken))
                {
                    _logger.LogInformation("Job for schedule {ScheduleId} already exists. Skipping.", schedule.Id);
                    continue;
                }
                
                IJobDetail job = JobBuilder.Create<Infrastructure.Scheduling.ReportGenerationJob>()
                    .WithIdentity(jobKey)
                    .WithDescription($"Job for Report Template {schedule.ReportTemplateId}")
                    .UsingJobData("ReportTemplateId", schedule.ReportTemplateId.ToString())
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(schedule.Id.ToString() + "-trigger", "ReportGeneration")
                    .WithCronSchedule(schedule.CronExpression)
                    .ForJob(job)
                    .Build();

                await _scheduler.ScheduleJob(job, trigger, cancellationToken);
                _logger.LogInformation("Scheduled report job for template {ReportTemplateId} with CRON '{CronExpression}'",
                    schedule.ReportTemplateId, schedule.CronExpression);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule job for Report Template {ReportTemplateId}", schedule.ReportTemplateId);
            }
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Report Scheduling Service is stopping.");
        if (_scheduler != null && !_scheduler.IsShutdown)
        {
            await _scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken);
        }
    }
}