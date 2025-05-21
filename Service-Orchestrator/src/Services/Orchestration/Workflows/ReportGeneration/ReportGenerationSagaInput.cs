using System.Collections.Generic;
using System;

namespace OrchestrationService.Workflows.ReportGeneration;

/// <summary>
/// Defines the parameters required to initiate a report generation saga,
/// such as report type, schedule information, triggering event data,
/// target users/roles for distribution, and any specific data filters.
/// </summary>
public class ReportGenerationSagaInput
{
    /// <summary>
    /// Unique identifier for this specific report request. Can be pre-assigned or generated.
    /// </summary>
    public Guid ReportId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of report requested (e.g., "DailySummary", "AnomalyDetails").
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Details about the schedule (e.g., "daily@02:00", "weekly-monday", or a specific one-time execution request).
    /// This can be a CRON expression or a descriptive string.
    /// </summary>
    public string ScheduleDetails { get; set; } = string.Empty;

    /// <summary>
    /// List of target recipients (e.g., email addresses, user IDs, role names).
    /// The actual distribution list might be resolved by the DistributeReportActivity.
    /// </summary>
    public List<string> TargetRecipients { get; set; } = new List<string>();

    /// <summary>
    /// Criteria for filtering the data used in the report.
    /// </summary>
    public DataFilters DataFilters { get; set; } = new DataFilters();

    /// <summary>
    /// Optional: Identifier of the event or external trigger that initiated this report generation.
    /// </summary>
    public string? TriggeringEventId { get; set; }

    /// <summary>
    /// Flag indicating if this report requires a validation step before final distribution/archiving.
    /// (REQ-7-022)
    /// </summary>
    public bool RequiresValidation { get; set; } = false;

    /// <summary>
    /// Optional: User ID of the person or system requesting the report.
    /// </summary>
    public string? RequestedByUserId { get; set; }
}

/// <summary>
/// Defines the criteria for filtering data for the report.
/// </summary>
public class DataFilters
{
    /// <summary>
    /// Start time for the data period.
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// End time for the data period.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// List of specific tag IDs or sensor IDs to include.
    /// </summary>
    public List<string>? TagIds { get; set; }

    /// <summary>
    /// Other custom filter criteria as key-value pairs.
    /// </summary>
    public Dictionary<string, string> CustomFilters { get; set; } = new Dictionary<string, string>();
}