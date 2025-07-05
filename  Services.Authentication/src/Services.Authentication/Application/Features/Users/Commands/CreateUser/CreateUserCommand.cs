using MediatR;
using Microsoft.AspNetCore.Identity;
using Opc.System.Services.Authentication.Application.Interfaces;
using Opc.System.Services.Authentication.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Opc.System.Services.Authentication.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Represents the intent to create a new user. This is a CQRS command.
/// </summary>
/// <param name="Username">The desired username for the new account.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password. Must meet complexity requirements.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Roles">A list of role names to assign to the user.</param>
public record CreateUserCommand(
    [Required] string Username,
    [Required][EmailAddress] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] List<string> Roles) : IRequest<Guid>;

/// <summary>
/// The handler contains the orchestration logic for the user creation process.
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
        {
            throw new ApplicationException($"Username '{request.Username}' is already taken.");
        }

        existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ApplicationException($"Email '{request.Email}' is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            EmailConfirmed = true // Assuming email confirmation is handled elsewhere or not required for this flow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            await _auditService.LogEventAsync("UserCreationFailure", "Failure", new { request.Username, Errors = errors });
            throw new ApplicationException($"Failed to create user: {errors}");
        }

        if (request.Roles.Any())
        {
            var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
            if (!roleResult.Succeeded)
            {
                // Note: User is created, but role assignment failed. This might require a compensating action.
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                await _auditService.LogEventAsync("RoleAssignmentFailure", "Failure", new { user.Id, request.Roles, Errors = errors }, actingUserId: user.Id, subjectId: user.Id.ToString());
                // Decide on error handling: throw, log, or ignore. For now, we log and proceed.
            }
        }
        
        await _auditService.LogEventAsync("UserCreated", "Success", new { user.Id, user.UserName }, subjectId: user.Id.ToString());

        return user.Id;
    }
}