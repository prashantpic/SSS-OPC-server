namespace AiService.Domain.Aggregates.AiModel;

/// <summary>
/// Represents a historical record of a model's performance at a specific point in time.
/// </summary>
public class ModelPerformanceLog
{
    /// <summary>
    /// Gets the unique identifier for the performance log entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the timestamp when the performance was recorded.
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Gets the performance score (e.g., accuracy, F1-score, MAE).
    /// </summary>
    public decimal Score { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelPerformanceLog"/> class.
    /// </summary>
    /// <param name="timestamp">The timestamp of the performance log.</param>
    /// <param name="score">The performance score.</param>
    public ModelPerformanceLog(DateTime timestamp, decimal score)
    {
        Id = Guid.NewGuid();
        Timestamp = timestamp;
        Score = score;
    }

    // Private constructor for ORM
#pragma warning disable CS8618
    private ModelPerformanceLog() { }
#pragma warning restore CS8618
}