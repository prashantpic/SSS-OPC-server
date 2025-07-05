using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Web.Extensions;

/// <summary>
/// Contains settings for JSON Web Token generation and validation.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// The secret key used to sign the JWT. Must be long and complex.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The issuer of the JWT.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience of the JWT.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The token's expiry duration in minutes.
    /// </summary>
    public int ExpiryInMinutes { get; set; }
}

/// <summary>
/// Provides extension methods to register and configure JWT Bearer authentication services.
/// </summary>
public static class AuthenticationServiceExtensions
{
    /// <summary>
    /// Centralizes JWT authentication setup, ensuring consistent token validation logic.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.Bind("JwtSettings", jwtSettings);
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Set to true in production
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = jwtSettings.Audience,
                ValidIssuer = jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}