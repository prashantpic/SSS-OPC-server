using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Controllers;

#region DTOs
/// <summary>
/// Represents the data required to create a new role.
/// </summary>
public record CreateRoleRequest([Required] string RoleName, string? Description);

/// <summary>
/// Represents a role's public data.
/// </summary>
public record RoleDto(Guid Id, string Name, string? Description);

/// <summary>
/// Represents the data required to assign a permission (claim) to a role.
/// </summary>
public record AssignPermissionRequest([Required] string ClaimType, [Required] string ClaimValue);

#endregion

/// <summary>
/// Defines RESTful endpoints for creating roles and managing the permissions granted to each role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesController"/> class.
    /// </summary>
    /// <param name="roleManager">The ASP.NET Core Identity role manager.</param>
    public RolesController(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="request">The request containing the new role's details.</param>
    /// <returns>The created role's data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var role = new ApplicationRole
        {
            Name = request.RoleName,
            Description = request.Description
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        var roleDto = new RoleDto(role.Id, role.Name, role.Description);
        return CreatedAtAction(nameof(GetRoleByName), new { roleName = role.Name }, roleDto);
    }

    /// <summary>
    /// Lists all available roles.
    /// </summary>
    /// <returns>A list of all roles.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleDto(r.Id, r.Name!, r.Description))
            .ToListAsync();
            
        return Ok(roles);
    }
    
    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    /// <param name="roleName">The name of the role to retrieve.</param>
    /// <returns>The role's data.</returns>
    [HttpGet("{roleName}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByName(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return NotFound();
        }
        return Ok(new RoleDto(role.Id, role.Name!, role.Description));
    }


    /// <summary>
    /// Assigns a permission (as a claim) to a role.
    /// </summary>
    /// <param name="roleName">The name of the role to modify.</param>
    /// <param name="request">The request containing the claim details.</param>
    /// <returns>An empty response on success.</returns>
    [HttpPost("{roleName}/claims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignPermissionToRole(string roleName, [FromBody] AssignPermissionRequest request)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return NotFound("Role not found.");
        }

        var claim = new Claim(request.ClaimType, request.ClaimValue);
        var result = await _roleManager.AddClaimAsync(role, claim);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}