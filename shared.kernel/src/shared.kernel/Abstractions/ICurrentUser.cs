namespace SharedKernel.Abstractions;

/// <summary>
/// Provides an abstraction to access the current user's information
/// from the execution context, decoupling services from HttpContext.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// Returns null if the user is not authenticated.
    /// </summary>
    Guid? UserId { get; }
    
    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// Returns an empty list if the user has no roles or is not authenticated.
    /// </summary>
    IReadOnlyList<string> Roles { get; }
}