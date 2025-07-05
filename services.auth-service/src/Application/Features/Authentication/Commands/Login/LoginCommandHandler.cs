using System.ComponentModel.DataAnnotations;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthService.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Contains the core logic for handling a user login request.
/// It validates credentials and orchestrates token generation.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginCommandHandler"/> class.
    /// </summary>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    /// <param name="tokenService">The token generation service.</param>
    /// <param name="jwtSettingsOptions">The JWT settings from configuration.</param>
    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettingsOptions.Value;
    }

    /// <summary>
    /// Handles the login command.
    /// </summary>
    /// <param name="request">The login command containing user credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response containing the access token and its expiration time.</returns>
    /// <exception cref="ValidationException">Thrown when user is not found or inactive.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid.</exception>
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null || !user.IsActive)
        {
            throw new ValidationException("User not found or account is inactive.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var accessToken = await _tokenService.GenerateAccessToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        return new TokenResponse(accessToken, expiresAt);
    }
}