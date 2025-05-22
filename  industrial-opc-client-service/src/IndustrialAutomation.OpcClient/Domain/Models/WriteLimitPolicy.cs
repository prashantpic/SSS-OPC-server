namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Defines policies for rate limiting or threshold checks on OPC write operations 
    /// to prevent server overload or erroneous writes.
    /// </summary>
    public record WriteLimitPolicy
    {
        /// <summary>
        /// Identifier of the tag this policy applies to. Can be a specific tag ID or a wildcard (e.g., "*").
        /// </summary>
        public required string TagIdPattern { get; init; }

        /// <summary>
        /// Enables or disables rate limiting for writes to the specified tag(s).
        /// </summary>
        public bool RateLimitingEnabled { get; init; } = false;

        /// <summary>
        /// Maximum number of allowed writes per minute if rate limiting is enabled.
        /// </summary>
        public int? MaxWritesPerMinute { get; init; }

        /// <summary>
        /// Enables or disables value change threshold checks.
        /// A write might be flagged or require confirmation if the change is too small or too large.
        /// </summary>
        public bool ValueChangeThresholdEnabled { get; init; } = false;

        /// <summary>
        /// Minimum percentage change required for a write to be considered significant.
        /// (e.g., 5.0 for 5%). Only applicable if ValueChangeThresholdEnabled is true.
        /// </summary>
        public double? MinSignificantValueChangePercentage { get; init; }

        /// <summary>
        /// Maximum percentage change allowed for a write without confirmation.
        /// (e.g., 50.0 for 50%). Only applicable if ValueChangeThresholdEnabled is true.
        /// </summary>
        public double? MaxAllowedValueChangePercentage { get; init; }
        
        /// <summary>
        /// If true, writes that fall below the MinSignificantValueChangePercentage or exceed MaxAllowedValueChangePercentage 
        /// (if enabled and set) might require special handling or confirmation (logic handled by service).
        /// </summary>
        public bool RequireConfirmationOnThresholdDeviation { get; init; } = false;

        /// <summary>
        /// Indicates if this policy is currently active.
        /// </summary>
        public bool Enabled { get; init; } = true;
    }
}