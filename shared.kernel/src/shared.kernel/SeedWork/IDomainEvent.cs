namespace SharedKernel.SeedWork;

/// <summary>
/// Represents a marker interface for a domain event,
/// signifying something that happened in the domain that other parts
/// of the same or different bounded contexts might be interested in.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}