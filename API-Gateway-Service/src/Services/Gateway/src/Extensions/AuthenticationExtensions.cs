using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using GatewayService.Services; // Assuming ITokenValidationService might be used in events

namespace GatewayService.Extensions
{
    /// <summary>
    /// Extension methods for configuring JWT and other authentication schemes.
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Configures JWT Bearer authentication for the API Gateway.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var securityConfigs = configuration.GetSection("SecurityConfigs");
            var jwtAuthority = securityConfigs["JwtAuthority"];
            var jwtAudience = securityConfigs["JwtAudience"];
            // Example: If using symmetric key (less common for IdP scenarios, more for self-issued)
            // var jwtKey = securityConfigs["JwtKey"];

            if (string.IsNullOrEmpty(jwtAuthority) || string.IsNullOrEmpty(jwtAudience))
            {
                // Log this warning or throw an exception if JWT auth is critical
                Console.WriteLine("Warning: JWT Authority or Audience is not configured. JWT Authentication will not function correctly.");
                // Or throw new ApplicationException("JWT Authority or Audience must be configured.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = jwtAuthority; // URL of the Identity Provider
                options.Audience = jwtAudience;   // Audience claim to validate
                options.RequireHttpsMetadata = configuration.GetValue<bool>("SecurityConfigs:RequireHttpsMetadata", true); // Should be true in production

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtAuthority), // Validate the server that generates the token
                    ValidateAudience = !string.IsNullOrEmpty(jwtAudience), // Validate the recipient of the token is authorized to receive it
                    ValidateLifetime = true, // Check if the token is not expired and that the signing key is valid
                    ValidateIssuerSigningKey = true, // Validate the signing key (important if Authority is not set or if using symmetric key)
                    //ClockSkew = TimeSpan.Zero // Optional: remove default 5-minute clock skew
                    // If using symmetric key:
                    // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "")),
                };

                // Example of custom event handling, could integrate with ITokenValidationService
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // var tokenValidationService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidationService>();
                        // Custom validation logic after standard validation
                        // e.g., check against a revocation list, custom claims
                        // If validation fails: context.Fail("Custom validation failed.");
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError(context.Exception, "Authentication failed.");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        // You might want to return a standardized error model here
                        // var errorResponse = new ErrorViewModel { Message = "Authentication failed.", StatusCode = 401 };
                        // await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        // E.g., to extract token from query string for WebSockets
                        // var accessToken = context.Request.Query["access_token"];
                        // var path = context.HttpContext.Request.Path;
                        // if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/graphql"))) // Or other WebSocket paths
                        // {
                        //    context.Token = accessToken;
                        // }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                // Example: Define a policy that requires a specific scope
                // options.AddPolicy("ApiScope", policy =>
                // {
                //     policy.RequireAuthenticatedUser();
                //     policy.RequireClaim("scope", "api.read", "api.write"); // Example scopes
                // });
            });


            return services;
        }
    }
}