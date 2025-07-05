namespace Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

/// <summary>
/// Defines the categories of data that can have retention policies applied to them.
/// This provides a strongly-typed identifier for different data categories subject to retention policies.
/// </summary>
public enum DataType
{
    /// <summary>
    /// Historical process data.
    /// </summary>
    Historical,

    /// <summary>
    /// Alarm and event data.
    /// </summary>
    Alarm,

    /// <summary>
    /// System audit trail data.
    /// </summary>
    Audit,

    /// <summary>
    /// Data related to Artificial Intelligence models or results.
    /// </summary>
    AI
}