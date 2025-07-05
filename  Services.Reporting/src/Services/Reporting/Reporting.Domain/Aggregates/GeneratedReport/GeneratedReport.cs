using Opc.System.Services.Reporting.Domain.Common;

namespace Opc.System.Services.Reporting.Domain.Aggregates.GeneratedReport;

/// <summary>
/// Represents an instance of a generated report, containing metadata about its creation, status, version, and sign-off state.
/// Models a generated report, tracking its lifecycle, storage location, and approval status.
/// </summary>
public class GeneratedReport : AggregateRoot<Guid>
{
    /// <summary>
    /// Foreign Key to the ReportTemplate.
    /// </summary>
    public Guid ReportTemplateId { get; private set; }

    /// <summary>
    /// When the report generation was initiated.
    /// </summary>
    public DateTimeOffset GenerationTimestamp { get; private set; }

    /// <summary>
    /// The current status of the report generation process.
    /// </summary>
    public ReportStatus Status { get; private set; }

    /// <summary>
    /// The path to the generated file (e.g., blob storage URI or file path).
    /// </summary>
    public string? StoragePath { get; private set; }

    /// <summary>
    /// Version number for reports based on the same template.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// The sign-off status of the report.
    /// </summary>
    public SignOffStatus SignOffStatus { get; private set; }
    
    /// <summary>
    /// User ID of the approver.
    /// </summary>
    public string? SignOffBy { get; private set; }
    
    /// <summary>
    /// Timestamp of the sign-off action.
    /// </summary>
    public DateTimeOffset? SignOffTimestamp { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // For EF Core
    private GeneratedReport() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    private GeneratedReport(Guid id, Guid reportTemplateId, int version) : base(id)
    {
        ReportTemplateId = reportTemplateId;
        GenerationTimestamp = DateTimeOffset.UtcNow;
        Status = ReportStatus.Pending;
        SignOffStatus = SignOffStatus.NotRequired;
        Version = version;
    }

    /// <summary>
    /// Static factory to initialize a report instance in the Pending state.
    /// </summary>
    /// <param name="reportTemplateId">The ID of the template being used.</param>
    /// <param name="version">The version number for this report instance.</param>
    /// <returns>A new GeneratedReport instance.</returns>
    public static GeneratedReport Create(Guid reportTemplateId, int version)
    {
        return new GeneratedReport(Guid.NewGuid(), reportTemplateId, version);
    }
    
    /// <summary>
    /// Moves status to Generating.
    /// </summary>
    public void StartGeneration()
    {
        if (Status == ReportStatus.Pending)
        {
            Status = ReportStatus.Generating;
        }
    }

    /// <summary>
    /// Sets status to Completed and records the storage path.
    /// </summary>
    /// <param name="storagePath">The path where the generated file is stored.</param>
    public void MarkAsCompleted(string storagePath)
    {
        if (Status != ReportStatus.Generating)
        {
            // Or throw a domain exception
            return;
        }
        Status = ReportStatus.Completed;
        StoragePath = storagePath;
    }

    /// <summary>
    /// Sets status to Failed.
    /// </summary>
    /// <param name="reason">The reason for the failure (optional, for logging).</param>
    public void MarkAsFailed(string reason)
    {
        if (Status != ReportStatus.Generating)
        {
            return;
        }
        Status = ReportStatus.Failed;
        // Optionally store the reason in a new property
    }
    
    /// <summary>
    /// Sets sign-off status to Pending.
    /// </summary>
    public void RequestSignOff()
    {
        if (Status == ReportStatus.Completed)
        {
            SignOffStatus = SignOffStatus.Pending;
        }
    }

    /// <summary>
    /// Sets sign-off status to Approved.
    /// </summary>
    /// <param name="userId">The ID of the user approving the report.</param>
    public void ApproveSignOff(string userId)
    {
        if (SignOffStatus != SignOffStatus.Pending)
        {
            // Or throw a domain exception
            return;
        }

        SignOffStatus = SignOffStatus.Approved;
        SignOffBy = userId;
        SignOffTimestamp = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Sets sign-off status to Rejected.
    /// </summary>
    /// <param name="userId">The ID of the user rejecting the report.</param>
    public void RejectSignOff(string userId)
    {
        if (SignOffStatus != SignOffStatus.Pending)
        {
            return;
        }

        SignOffStatus = SignOffStatus.Rejected;
        SignOffBy = userId;
        SignOffTimestamp = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Represents the lifecycle status of a generated report.
/// </summary>
public enum ReportStatus
{
    Pending,
    Generating,
    Completed,
    Failed
}

/// <summary>
/// Represents the approval status for a report that requires sign-off.
/// </summary>
public enum SignOffStatus
{
    NotRequired,
    Pending,
    Approved,
    Rejected
}