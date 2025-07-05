using Microsoft.AspNetCore.Identity;

namespace Opc.System.Services.Authentication.Domain.Entities;

/// <summary>
/// The core User entity. It represents an individual who can be authenticated and authorized to access system resources.
/// Extends the base IdentityUser to include application-specific properties.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Flag indicating if the user account is active and can be used for login.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The timestamp when the user account was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The timestamp of the last update to the user account.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
    
    /// <summary>
    /// The unique identifier from an external identity provider, if the user is federated.
    /// </summary>
    public string? ExternalProviderId { get; set; }
}