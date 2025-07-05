using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.Authentication.Application.Features.Roles.Commands.UpdateRolePermissions;
using System.ComponentModel.DataAnnotations;

namespace Opc.System.Services.Authentication.Api.Controllers;

/// <summary>
/// Provides the public RESTful API for all role and permission-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly ISender _mediator;

    public RolesController(ISender mediator)
    {
        _mediator = mediator;
    }

    // In a full implementation, you would have endpoints for creating, listing, and deleting roles.
    // Example: [HttpPost] public async Task<IActionResult> CreateRole(...) { ... }

    /// <summary>
    /// Updates the set of permissions associated with a specific role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="commandPayload">The payload containing the new list of permissions.</param>
    /// <returns>A 204 No Content response on success.</returns>
    [HttpPut("{roleId:guid}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePermissionsForRole(Guid roleId, [FromBody] UpdatePermissionsPayload commandPayload)
    {
        var command = new UpdateRolePermissionsCommand(roleId, commandPayload.Permissions);
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            // Can distinguish between 404 and 400 based on exception type if needed
            return BadRequest(new { message = ex.Message });
        }
    }
    
    /// <summary>
    /// Payload for the UpdatePermissionsForRole endpoint.
    /// </summary>
    public record UpdatePermissionsPayload([Required] List<string> Permissions);
}