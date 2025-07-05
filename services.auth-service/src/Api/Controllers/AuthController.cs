using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.Application.Features.Authentication.Commands.Login;

namespace AuthService.Api.Controllers;

/// <summary>
/// A request to log in with internal credentials.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="Password">The user's password.</param>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Defines RESTful endpoints for user authentication and token lifecycle management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender to dispatch commands and queries.</param>
    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates an internal user and returns a JWT.
    /// </summary>
    /// <param name="request">The login request containing user credentials.</param>
    /// <returns>An action result containing the token response on success.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}