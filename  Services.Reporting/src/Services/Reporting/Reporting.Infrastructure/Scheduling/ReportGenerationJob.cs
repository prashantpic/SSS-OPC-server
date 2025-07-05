using MediatR;
using Microsoft.Extensions.Logging;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;
using Quartz;

namespace Opc.System.Services.Reporting.Infrastructure.Scheduling;

/// <summary>
/// Implements a schedulable job (e.g., for Quartz.NET) that, when executed, sends a command
/// to the application layer to start generating a specific report.
/// To decouple the scheduling mechanism from the application logic. This job acts as the entry point for a scheduled task.
/// </summary>
[DisallowConcurrentExecution]
public class ReportGenerationJob : IJob
{
    private readonly ISender _sender;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(ISender sender, ILogger<ReportGenerationJob> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
    /// fires that is associated with the <see cref="IJob" />.
    /// </summary>
    /// <param name="context">The execution context of the job.</param>
    /// <returns>A task that represents the asynchronous execution of the job.</returns>
    public async Task Execute(IJobExecutionContext context)
    {
        var reportTemplateIdStr = context.JobDetail.JobDataMap.GetString("ReportTemplateId");
        if (!Guid.TryParse(reportTemplateIdStr, out var reportTemplateId))
        {
            _logger.LogError("Could not parse ReportTemplateId '{Id}' from job data.", reportTemplateIdStr);
            return;
        }

        _logger.LogInformation("Quartz job executing for ReportTemplateId: {ReportTemplateId}", reportTemplateId);

        try
        {
            // For now, we assume scheduled reports are always PDF. This could be stored in the schedule entity.
            var command = new GenerateReportCommand(reportTemplateId, ReportFormat.Pdf);
            await _sender.Send(command, context.CancellationToken);
            _logger.LogInformation("Successfully completed Quartz job for ReportTemplateId: {ReportTemplateId}", reportTemplateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the scheduled report generation for template {ReportTemplateId}", reportTemplateId);
            // The exception is logged, but not re-thrown, to prevent Quartz from marking the job as failed and potentially re-firing it immediately.
            // The command handler is responsible for marking the report instance as 'Failed' in the database.
        }
    }
}