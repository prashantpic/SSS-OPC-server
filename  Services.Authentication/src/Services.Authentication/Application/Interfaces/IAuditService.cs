namespace Opc.System.Services.Authentication.Application.Interfaces;

/// <summary>
/// A contract for the audit logging service, ensuring all security events are captured consistently.
/// This provides a centralized, asynchronous way for the application to record audit trail entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Asynchronously logs a security-relevant event.
    /// </summary>
    /// <param name="eventType">The type of event (e.g., "UserLoginAttempt").</param>
    /// <param name="outcome">The result of the event (e.g., "Success", "Failure").</param>
    /// <param name="details">An object containing event-specific details, which will be serialized to JSON.</param>
    /// <param name="actingUserId">Optional. The ID of the user who performed the action.</param>
    /// <param name="subjectId">Optional. The ID or name of the resource that was affected.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogEventAsync(string eventType, string outcome, object details, Guid? actingUserId = null, string? subjectId = null);
}