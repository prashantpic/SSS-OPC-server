using System;

namespace DataService.Domain.Entities
{
    /// <summary>
    /// Specifies the type of data a retention policy applies to.
    /// </summary>
    public enum DataType
    {
        Historical,
        Alarm,
        Audit,
        AI
    }

    /// <summary>
    /// Specifies the action to take when data reaches its retention period.
    /// </summary>
    public enum RetentionAction
    {
        /// <summary>
        /// Permanently delete the data.
        /// </summary>
        Purge,
        
        /// <summary>
        /// Move the data to a long-term storage location.
        /// </summary>
        Archive
    }

    /// <summary>
    /// Represents a rule for managing the lifecycle of data within the system.
    /// Defines how long data of a certain type should be kept and what action to take upon expiration.
    /// Fulfills requirement REQ-DLP-017.
    /// </summary>
    public class DataRetentionPolicy
    {
        /// <summary>
        /// The unique identifier for the policy.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The type of data this policy applies to (e.g., Historical, Alarm).
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// The number of days to retain the data before taking action.
        /// </summary>
        public int RetentionPeriodDays { get; set; }

        /// <summary>
        /// The action to perform after the retention period (e.g., Purge, Archive).
        /// </summary>
        public RetentionAction Action { get; set; }

        /// <summary>
        /// The location for archiving data, such as an S3 bucket path or a database connection string.
        /// This is only relevant if the Action is 'Archive'.
        /// </summary>
        public string? ArchiveLocation { get; set; }

        /// <summary>
        /// A flag indicating whether this policy is currently active and should be enforced.
        /// </summary>
        public bool IsActive { get; set; }
    }
}