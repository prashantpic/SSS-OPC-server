using System.Text.RegularExpressions;

namespace Reporting.Domain.ValueObjects;

/// <summary>
/// Represents the schedule for a report using a CRON expression.
/// </summary>
public record Schedule
{
    /// <summary>
    /// Gets the CRON expression defining the schedule.
    /// </summary>
    public string CronExpression { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Schedule"/> class.
    /// </summary>
    /// <param name="cronExpression">The CRON expression.</param>
    /// <exception cref="ArgumentException">Thrown if the CRON expression is null, empty, or invalid.</exception>
    public Schedule(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException("CRON expression cannot be null or whitespace.", nameof(cronExpression));
        }

        // This is a very basic validation. A production system should use a robust CRON parsing library.
        var parts = cronExpression.Split(' ');
        if (parts.Length < 5 || parts.Length > 6)
        {
            throw new ArgumentException("Invalid CRON expression format.", nameof(cronExpression));
        }

        CronExpression = cronExpression;
    }
}