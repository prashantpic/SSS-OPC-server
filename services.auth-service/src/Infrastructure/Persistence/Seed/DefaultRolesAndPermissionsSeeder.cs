using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence.Seed;

/// <summary>
/// A utility class to seed the database with predefined default roles.
/// </summary>
public static class DefaultRolesAndPermissionsSeeder
{
    /// <summary>
    /// Ensures that essential, predefined roles are available in the system upon initial setup.
    /// </summary>
    /// <param name="roleManager">The RoleManager service.</param>
    /// <param name="logger">The logger service.</param>
    public static async Task SeedDefaultRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var defaultRoles = new[]
        {
            new ApplicationRole { Name = "Administrator", Description = "Has full access to all system features." },
            new ApplicationRole { Name = "Engineer", Description = "Manages system configurations and technical settings." },
            new ApplicationRole { Name = "Operator", Description = "Monitors and operates the system on a day-to-day basis." },
            new ApplicationRole { Name = "Viewer", Description = "Has read-only access to system data." }
        };

        foreach (var role in defaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created default role: {RoleName}", role.Name);
                }
                else
                {
                    logger.LogError("Failed to create default role {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}