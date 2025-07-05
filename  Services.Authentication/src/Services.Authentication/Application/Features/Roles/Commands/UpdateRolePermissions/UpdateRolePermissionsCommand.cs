using MediatR;
using Microsoft.AspNetCore.Identity;
using Opc.System.Services.Authentication.Application.Interfaces;
using Opc.System.Services.Authentication.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Opc.System.Services.Authentication.Application.Features.Roles.Commands.UpdateRolePermissions;

/// <summary>
/// Represents the intent to change the permissions for a specific role. The handler orchestrates the update.
/// Permissions are represented as claims.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to update.</param>
/// <param name="Permissions">The complete new list of permission strings for the role.</param>
public record UpdateRolePermissionsCommand(
    [Required] Guid RoleId,
    [Required] List<string> Permissions) : IRequest;

/// <summary>
/// The handler for updating a role's permissions.
/// </summary>
public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditService _auditService;
    public const string PermissionClaimType = "permission";

    public UpdateRolePermissionsCommandHandler(RoleManager<ApplicationRole> roleManager, IAuditService auditService)
    {
        _roleManager = roleManager;
        _auditService = auditService;
    }

    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null)
        {
            throw new ApplicationException($"Role with ID '{request.RoleId}' not found.");
        }
        if (role.IsSystemRole)
        {
            throw new ApplicationException($"Cannot modify permissions for system role '{role.Name}'.");
        }

        var currentClaims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = currentClaims.Where(c => c.Type == PermissionClaimType).ToList();

        var originalPermissions = permissionClaims.Select(c => c.Value).ToList();

        // Remove old permissions
        foreach (var claim in permissionClaims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // Add new permissions
        foreach (var permission in request.Permissions.Distinct())
        {
            await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, permission));
        }

        await _auditService.LogEventAsync("RolePermissionsUpdated", "Success", 
            new { RoleName = role.Name, OldPermissions = originalPermissions, NewPermissions = request.Permissions },
            subjectId: role.Id.ToString());
    }
}