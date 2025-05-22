namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Defines policies for rate limiting or threshold checks on OPC write operations 
/// to prevent server overload or erroneous writes.
/// </summary>
public record WriteLimitPolicy
{
    // Applies to a specific TagId, or "*" for a default policy (if supported by limiter logic)
    public required string TagId { get; init; } 

    // Rate Limiting
    public bool RateLimitingEnabled { get; init; } = false;
    public int MaxWritesPerInterval { get; init; } = 10; // e.g., 10 writes
    public int IntervalSeconds { get; init; } = 60; // e.g., per 60 seconds (1 minute)

    // Value Change Threshold
    public bool ValueChangeThresholdEnabled { get; init; } = false;
    // Minimum percentage the value must change by to be written without confirmation (if RequireConfirmation is true)
    // Or, minimum absolute change if MinAbsoluteValueChange is set and preferred.
    public double MinValueChangePercentage { get; init; } = 0; 
    public double? MinAbsoluteValueChange { get; init; } // Alternative to percentage

    // Confirmation Logic
    // If true and the change is below threshold, the write might be blocked or require special handling
    public bool RequireConfirmationBelowThreshold { get; init; } = false; 

    public bool Enabled { get; init; } = true;
}