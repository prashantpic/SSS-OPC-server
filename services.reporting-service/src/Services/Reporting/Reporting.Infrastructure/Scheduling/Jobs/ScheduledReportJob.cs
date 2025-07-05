using MediatR;
using Microsoft.Extensions.Logging;
using Reporting.Application.Generation.Commands.GenerateReportOnDemand;
using Reporting.Domain.Enums;

namespace Reporting.Infrastructure.Scheduling.Jobs;

/// <summary>
/// A background job, designed to be executed by a scheduler like Hangfire,
/// that triggers the generation of a scheduled report.
/// </summary>
public class ScheduledReportJob
{
    private readonly ISender _sender;
    private readonly ILogger<ScheduledReportJob> _logger;

    public ScheduledReportJob(ISender sender, ILogger<ScheduledReportJob> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the report generation job for a given template.
    /// </summary>
    /// <param name="templateId">The ID of the template to be generated.</param>
    /// <param name="defaultFormat">The default output format for the report.</param>
    public async Task Execute(Guid templateId, ReportFormat defaultFormat)
    {
        _logger.LogInformation("Executing scheduled report job for TemplateId: {TemplateId}", templateId);

        try
        {
            var command = new GenerateReportOnDemandCommand(templateId, defaultFormat.ToString());
            var reportId = await _sender.Send(command);
            _logger.LogInformation("Successfully triggered report generation for TemplateId: {TemplateId}. GeneratedReportId: {ReportId}", templateId, reportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scheduled report job for TemplateId: {TemplateId}", templateId);
            // The exception is logged, but not re-thrown, to prevent Hangfire from retrying indefinitely by default
            // for business logic failures. Configure retry policies in Hangfire for infrastructure issues.
        }
    }
}