using MediatR;
using Microsoft.AspNetCore.Identity;
using Opc.System.Services.Authentication.Application.Interfaces;
using Opc.System.Services.Authentication.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;

namespace Opc.System.Services.Authentication.Application.Features.Auth.Commands.Login;

/// <summary>
/// Represents the intent to log in. The handler validates user credentials and issues a token upon success.
/// </summary>
/// <param name="Username">The user's username.</param>
/// <param name="Password">The user's password.</param>
public record LoginCommand([Required] string Username, [Required] string Password) : IRequest<LoginResult>;

/// <summary>
/// Represents the successful result of a login operation.
/// </summary>
/// <param name="Token">The generated JSON Web Token.</param>
public record LoginResult(string Token);

/// <summary>
/// The handler for the LoginCommand, orchestrating authentication and token generation.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IAuditService _auditService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtGenerator jwtGenerator,
        IAuditService auditService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGenerator = jwtGenerator;
        _auditService = auditService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);

        if (user == null || !user.IsActive)
        {
            await _auditService.LogEventAsync("LoginAttempt", "Failure", new { request.Username, Reason = "UserNotFoundOrInactive" });
            throw new AuthenticationException("Invalid username or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            string reason = result.IsLockedOut ? "AccountLockedOut" : "InvalidCredentials";
            await _auditService.LogEventAsync("LoginAttempt", "Failure", new { request.Username, Reason = reason }, actingUserId: user.Id, subjectId: user.Id.ToString());
            throw new AuthenticationException("Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtGenerator.GenerateToken(user, roles);

        await _auditService.LogEventAsync("UserLogin", "Success", new { request.Username }, actingUserId: user.Id, subjectId: user.Id.ToString());

        return new LoginResult(token);
    }
}