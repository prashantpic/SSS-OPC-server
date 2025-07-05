using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// Extends the base IdentityUser with a Guid as the primary key type.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// Defaults to true.
    /// </summary>
    public bool IsActive { get; set; } = true;
}