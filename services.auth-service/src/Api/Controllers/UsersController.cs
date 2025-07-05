using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Controllers;

#region DTOs
/// <summary>
/// Represents the data required to create a new user.
/// </summary>
public record CreateUserRequest([Required] string Username, [Required][EmailAddress] string Email, [Required] string Password, string? FullName);

/// <summary>
/// Represents the data required to assign a role to a user.
/// </summary>
public record AssignRoleRequest([Required] string RoleName);

/// <summary>
/// Represents a user's public data.
/// </summary>
public record UserDto(Guid Id, string Username, string Email, string? FullName, bool IsActive, IList<string> Roles);
#endregion

/// <summary>
/// Defines RESTful endpoints for performing CRUD operations on user accounts and their role assignments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The request containing the new user's details.</param>
    /// <returns>The created user's data.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, user.IsActive, roles);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
    }

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's ID.</param>
    /// <returns>The user's data.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, user.IsActive, roles);
        
        return Ok(userDto);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="id">The user's ID.</param>
    /// <param name="request">The request containing the role name.</param>
    /// <returns>An empty response on success.</returns>
    [HttpPost("{id:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRoleToUser(Guid id, [FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
    
    /// <summary>
    /// Gets a list of all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto(user.Id, user.UserName!, user.Email!, user.FullName, user.IsActive, roles));
        }

        return Ok(userDtos);
    }
}