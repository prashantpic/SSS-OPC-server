using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Gateways.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring authentication and authorization services.
/// </summary>
public static class AuthConfigurationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication and default authorization policies for the API Gateway.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's <see cref="IConfiguration"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var authority = jwtSettings["Authority"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrEmpty(authority) || string.IsNullOrEmpty(audience))
        {
            throw new InvalidOperationException("JWT Authority and Audience must be configured in appsettings.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // NameClaimType and RoleClaimType can be configured if claims have different names
                    NameClaimType = ClaimTypes.NameIdentifier, 
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            // The default policy used by YARP when a route has "AuthorizationPolicy": "default"
            options.AddPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            
            // Example of a role-based policy that could be used on specific routes
            // options.AddPolicy("AdministratorOnly", policy =>
            // {
            //     policy.RequireRole("Administrator");
            // });
        });

        return services;
    }
}