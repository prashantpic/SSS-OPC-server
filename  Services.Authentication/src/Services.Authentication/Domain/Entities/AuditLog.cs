using System.Text.Json;

namespace Opc.System.Services.Authentication.Domain.Entities;

/// <summary>
/// An entity representing an audit log entry, capturing who did what, when, to what, and what the result was.
/// This is an immutable record of a security-relevant system event.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; private set; }

    /// <summary>
    /// The type of event that occurred (e.g., "UserLogin", "RoleUpdated").
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// The unique identifier of the user who performed the action. Can be null for system events.
    /// </summary>
    public Guid? ActingUserId { get; private set; }

    /// <summary>
    /// A string representation of the entity or resource that was the subject of the action (e.g., a user ID, role name).
    /// </summary>
    public string? SubjectId { get; private set; }

    /// <summary>
    /// A JSON string containing event-specific details.
    /// </summary>
    public string Details { get; private set; }

    /// <summary>
    /// The outcome of the event (e.g., "Success", "Failure").
    /// </summary>
    public string Outcome { get; private set; }

    // Private constructor for EF Core
    private AuditLog() { }

    /// <summary>
    /// Initializes a new instance of the AuditLog class.
    /// </summary>
    /// <param name="eventType">The type of event.</param>
    /// <param name="outcome">The outcome of the event.</param>
    /// <param name="details">Event-specific details, will be serialized to JSON.</param>
    /// <param name="actingUserId">The ID of the user performing the action.</param>
    /// <param name="subjectId">The ID of the subject of the action.</param>
    public AuditLog(string eventType, string outcome, object details, Guid? actingUserId, string? subjectId)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTimeOffset.UtcNow;
        EventType = eventType;
        Outcome = outcome;
        Details = JsonSerializer.Serialize(details);
        ActingUserId = actingUserId;
        SubjectId = subjectId;
    }
}