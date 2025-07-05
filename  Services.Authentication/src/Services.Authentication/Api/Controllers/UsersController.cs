using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.Authentication.Application.Features.DataPrivacy.Commands.AnonymizeUser;
using Opc.System.Services.Authentication.Application.Features.Users.Commands.CreateUser;

namespace Opc.System.Services.Authentication.Api.Controllers;

/// <summary>
/// Provides the public RESTful API for all user-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Most user management actions should be admin-only
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new user in the internal user store.
    /// </summary>
    /// <param name="command">The command containing details for the new user.</param>
    /// <returns>A 201 Created response with the location of the new resource.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            // In a real application, you would have a GetUserById endpoint to point to.
            return CreatedAtAction(null, new { id = userId }, new { id = userId });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Anonymizes a user's data to fulfill 'right to be forgotten' requests.
    /// </summary>
    /// <param name="id">The unique identifier of the user to anonymize.</param>
    /// <returns>A 204 No Content response on success.</returns>
    [HttpPost("{id:guid}/anonymize")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AnonymizeUser(Guid id)
    {
        var command = new AnonymizeUserCommand(id);
        var success = await _mediator.Send(command);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}