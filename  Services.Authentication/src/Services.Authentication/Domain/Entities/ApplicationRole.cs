using Microsoft.AspNetCore.Identity;

namespace Opc.System.Services.Authentication.Domain.Entities;

/// <summary>
/// The core Role entity. It groups permissions together, forming the basis of Role-Based Access Control (RBAC).
/// Extends the base IdentityRole.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// A description of the role's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the role is a system-defined role that cannot be deleted.
    /// </summary>
    public bool IsSystemRole { get; set; }
}