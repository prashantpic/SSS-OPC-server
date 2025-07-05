using AuthService.Application.Features.Authentication.Commands.Login;

namespace AuthService.Application.External.Idp;

/// <summary>
/// An abstraction for a service that handles user provisioning and token issuance
/// following a successful external login.
/// </summary>
public interface IIdpIntegrationService
{
    /// <summary>
    /// Processes the external login information obtained from an OIDC provider.
    /// It finds or creates a local user linked to the external identity and then
    /// issues the application's own JWT for that user.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// application's internal token response.
    /// </returns>
    Task<TokenResponse> ProcessExternalLoginAsync();
}