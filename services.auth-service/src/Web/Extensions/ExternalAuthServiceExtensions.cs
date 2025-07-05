using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AuthService.Web.Extensions;

/// <summary>
/// Provides an extension method to register and configure authentication handlers
/// for external OpenID Connect identity providers.
/// </summary>
public static class ExternalAuthServiceExtensions
{
    /// <summary>
    /// To abstract and conditionally enable OIDC authentication providers based on
    /// application configuration.
    /// </summary>
    /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add the OIDC handler to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/> so that additional calls can be chained.</returns>
    public static AuthenticationBuilder AddExternalIdentityProviders(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        // Configure Azure AD
        if (configuration.GetValue<bool>("ExternalIdpSettings:AzureAd:Enabled"))
        {
            builder.AddOpenIdConnect("AzureAd", "Azure AD", options =>
            {
                configuration.GetSection("ExternalIdpSettings:AzureAd").Bind(options);
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
            });
        }

        // Configure Okta (or other providers) similarly
        // if (configuration.GetValue<bool>("ExternalIdpSettings:Okta:Enabled"))
        // {
        //     // ... Okta configuration
        // }

        return builder;
    }
}