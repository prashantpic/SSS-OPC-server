using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;

/// <summary>
/// A domain entity representing the settings for a data retention policy.
/// This POCO class maps to the DataRetentionPolicy table in the database and is used
/// to transport policy information between the data access and application layers.
/// </summary>
public class DataRetentionPolicy
{
    /// <summary>
    /// The unique identifier for the policy.
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// The type of data this policy applies to (e.g., Historical, Alarm).
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// The number of days data should be retained in the primary datastore before being actioned.
    /// </summary>
    public int RetentionPeriodDays { get; set; }

    /// <summary>
    /// The location for long-term archival (e.g., an S3 bucket or Azure Blob container name).
    /// If null or empty, data will be purged without archiving.
    /// </summary>
    public string? ArchiveLocation { get; set; }

    /// <summary>
    /// Indicates whether the policy is currently active and should be enforced.
    /// </summary>
    public bool IsActive { get; set; }
}