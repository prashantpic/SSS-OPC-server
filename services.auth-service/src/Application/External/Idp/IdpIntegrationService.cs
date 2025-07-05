using System.Security.Claims;
using AuthService.Application.Features.Authentication.Commands.Login;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthService.Application.External.Idp;

/// <summary>
/// Contains the logic to find or create a local user corresponding to an external identity
/// and then generate internal application tokens.
/// </summary>
public class IdpIntegrationService : IIdpIntegrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<IdpIntegrationService> _logger;
    private readonly JwtSettings _jwtSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdpIntegrationService"/> class.
    /// </summary>
    public IdpIntegrationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettingsOptions,
        ILogger<IdpIntegrationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
        _jwtSettings = jwtSettingsOptions.Value;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> ProcessExternalLoginAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogError("Error loading external login information.");
            throw new InvalidOperationException("Error loading external login information.");
        }

        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user == null)
        {
            user = await CreateUserFromExternalLogin(info);
        }

        var accessToken = await _tokenService.GenerateAccessToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        return new TokenResponse(accessToken, expiresAt);
    }

    private async Task<ApplicationUser> CreateUserFromExternalLogin(ExternalLoginInfo info)
    {
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            throw new InvalidOperationException("Email claim not received from external provider.");
        }
        
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
             // If user exists with the email, link the external login
            var linkResult = await _userManager.AddLoginAsync(user, info);
            if (!linkResult.Succeeded)
            {
                _logger.LogError("Failed to link external login for existing user {Email}", email);
                throw new InvalidOperationException($"Could not link external login for user {email}.");
            }
            return user;
        }

        var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            _logger.LogError("Failed to create new user from external login: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
            throw new InvalidOperationException("Could not create user from external login.");
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            _logger.LogError("Failed to add external login to new user: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
            // Consider a compensating action, e.g., deleting the user created in the previous step
            await _userManager.DeleteAsync(user);
            throw new InvalidOperationException("Could not associate external login with the new user.");
        }
        
        _logger.LogInformation("User created from external provider: {Email}", email);
        return user;
    }
}