using Opc.System.Services.Reporting.Domain.Common;

namespace Opc.System.Services.Reporting.Domain.Entities;

/// <summary>
/// Defines a schedule, based on a CRON expression, for the automated generation of a specific report template.
/// Models the configuration for a recurring report generation task.
/// </summary>
public class ReportSchedule : Entity<Guid>
{
    /// <summary>
    /// Foreign key to the ReportTemplate.
    /// </summary>
    public Guid ReportTemplateId { get; private set; }

    /// <summary>
    /// The schedule in CRON format.
    /// </summary>
    public string CronExpression { get; private set; }

    /// <summary>
    /// Flag to activate or deactivate the schedule.
    /// </summary>
    public bool IsEnabled { get; private set; }

    private ReportSchedule(Guid id, Guid reportTemplateId, string cronExpression) : base(id)
    {
        // Add validation for CronExpression format here
        ReportTemplateId = reportTemplateId;
        CronExpression = cronExpression;
        IsEnabled = true;
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // Required for EF Core
    private ReportSchedule() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618


    /// <summary>
    /// Creates a new ReportSchedule instance.
    /// </summary>
    /// <param name="reportTemplateId">The ID of the template to schedule.</param>
    /// <param name="cronExpression">The CRON expression for the schedule.</param>
    /// <returns>A new ReportSchedule.</returns>
    public static ReportSchedule Create(Guid reportTemplateId, string cronExpression)
    {
        return new ReportSchedule(Guid.NewGuid(), reportTemplateId, cronExpression);
    }

    /// <summary>
    /// Toggles the IsEnabled flag to false.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Toggles the IsEnabled flag to true.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
    }
    
    /// <summary>
    /// Updates the CRON expression for the schedule.
    /// </summary>
    /// <param name="newCronExpression">The new CRON expression.</param>
    public void UpdateCronExpression(string newCronExpression)
    {
        // Add validation for the new expression
        CronExpression = newCronExpression;
    }
}