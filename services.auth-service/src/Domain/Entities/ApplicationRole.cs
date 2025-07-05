using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

/// <summary>
/// Represents a role in the system.
/// Extends the base IdentityRole with a Guid as the primary key type.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Gets or sets the description of the role, explaining its purpose.
    /// </summary>
    public string? Description { get; set; }
}