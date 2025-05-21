using System.Collections.Generic;
using System;
using OrchestrationService.Workflows.ReportGeneration; // For DataFilters

namespace OrchestrationService.Application.Events;

/// <summary>
/// Represents an event (e.g., from a message queue or an internal system trigger)
/// that signals a request to initiate the report generation saga.
/// Contains necessary data for starting the workflow.
/// </summary>
public class ReportGenerationRequestedEvent
{
    /// <summary>
    /// Unique identifier for this specific report request, if available from the source.
    /// </summary>
    public Guid ReportId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of report requested.
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Scheduling details if applicable.
    /// </summary>
    public string? ScheduleDetails { get; set; }

    /// <summary>
    /// Target recipients for the report.
    /// </summary>
    public List<string> TargetRecipients { get; set; } = new List<string>();

    /// <summary>
    /// Data filters for the report.
    /// </summary>
    public DataFilters DataFilters { get; set; } = new DataFilters();

    /// <summary>
    /// Optional identifier of the event that triggered this request.
    /// </summary>
    public string? TriggeringEventId { get; set; }

    /// <summary>
    /// Indicates if the report requires a validation step.
    /// </summary>
    public bool RequiresValidation { get; set; } = false;

    /// <summary>
    /// User ID of the requester, if available.
    /// </summary>
    public string? RequestedByUserId { get; set; }
}