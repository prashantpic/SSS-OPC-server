using System;

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Strongly-typed representation of an OPC tag's value, quality, and timestamp, 
/// ensuring data integrity within the domain.
/// </summary>
public class OpcValue
{
    public object? Value { get; }
    public DateTime Timestamp { get; }
    public string QualityStatus { get; }

    public OpcValue(object? value, DateTime timestamp, string qualityStatus)
    {
        Value = value;
        Timestamp = timestamp;
        QualityStatus = qualityStatus ?? throw new ArgumentNullException(nameof(qualityStatus));
    }
}