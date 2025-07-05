using Reporting.Domain.Entities.Base;
using Reporting.Domain.Enums;

namespace Reporting.Domain.Aggregates;

/// <summary>
/// Represents a generated report instance. This is an aggregate root.
/// </summary>
public class GeneratedReport : Entity<Guid>
{
    /// <summary>
    /// The status of the report generation job.
    /// </summary>
    public enum ReportStatus
    {
        Processing,
        Completed,
        Failed
    }

    public Guid ReportTemplateId { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public ReportStatus Status { get; private set; }
    public ReportFormat Format { get; private set; }
    public string? FilePath { get; private set; }
    public string? FailureReason { get; private set; }

    // For EF Core
    private GeneratedReport() { }

    private GeneratedReport(Guid id, Guid reportTemplateId, ReportFormat format) : base(id)
    {
        ReportTemplateId = reportTemplateId;
        Format = format;
        Status = ReportStatus.Processing;
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new record for a report generation job.
    /// </summary>
    /// <param name="id">The unique identifier for this report instance.</param>
    /// <param name="reportTemplateId">The ID of the template being used.</param>
    /// <param name="format">The output format for this report.</param>
    /// <returns>A new GeneratedReport instance.</returns>
    public static GeneratedReport Create(Guid id, Guid reportTemplateId, ReportFormat format)
    {
        return new GeneratedReport(id, reportTemplateId, format);
    }

    /// <summary>
    /// Marks the report as successfully completed.
    /// </summary>
    /// <param name="filePath">The path or URL to the generated report file.</param>
    public void MarkAsCompleted(string filePath)
    {
        if (Status != ReportStatus.Processing)
        {
            throw new InvalidOperationException("Cannot complete a report that is not in the 'Processing' state.");
        }

        Status = ReportStatus.Completed;
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        CompletedAt = DateTime.UtcNow;
        FailureReason = null;
    }

    /// <summary>
    /// Marks the report as failed.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    public void MarkAsFailed(string reason)
    {
        if (Status != ReportStatus.Processing)
        {
            throw new InvalidOperationException("Cannot fail a report that is not in the 'Processing' state.");
        }
        
        Status = ReportStatus.Failed;
        FailureReason = reason ?? "An unknown error occurred.";
        CompletedAt = DateTime.UtcNow;
        FilePath = null;
    }
}