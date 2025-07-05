using Microsoft.AspNetCore.Identity;
using Opc.System.Services.Authentication.Domain.Entities;
using System.Security.Claims;

namespace Opc.System.Services.Authentication.Infrastructure.ExternalProviders;

/// <summary>
/// Orchestrates the process of mapping an external identity from an IdP to a local application user.
/// This logic would typically be invoked from an API controller handling the OIDC callback.
/// </summary>
public class ExternalAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ExternalAuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Processes the callback from an external IdP. It gets external login info, finds or creates a local user,
    /// and prepares for local sign-in.
    /// </summary>
    /// <returns>A tuple containing the result of the operation and the resulting ApplicationUser if successful.</returns>
    public async Task<(IdentityResult result, ApplicationUser? user)> HandleExternalLoginCallbackAsync()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "Error loading external login information." }), null);
        }

        // Try to sign in the user with this external login provider.
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            return (IdentityResult.Success, user);
        }
        
        if (signInResult.IsLockedOut)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "User account locked out." }), null);
        }

        // If the user does not have an account, then create one.
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        ApplicationUser? localUser = null;
        if (email != null)
        {
            localUser = await _userManager.FindByEmailAsync(email);
            if (localUser != null)
            {
                // Link the external account to the existing local account
                var linkResult = await _userManager.AddLoginAsync(localUser, info);
                return (linkResult, localUser);
            }
        }
        
        // Create a new user if no local account is found
        var newUser = new ApplicationUser
        {
            UserName = email ?? $"user_{Guid.NewGuid().ToString().Substring(0, 8)}",
            Email = email,
            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "External",
            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ExternalProviderId = info.ProviderKey
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (createResult.Succeeded)
        {
            createResult = await _userManager.AddLoginAsync(newUser, info);
            if (createResult.Succeeded)
            {
                return (IdentityResult.Success, newUser);
            }
        }

        return (createResult, null);
    }
}