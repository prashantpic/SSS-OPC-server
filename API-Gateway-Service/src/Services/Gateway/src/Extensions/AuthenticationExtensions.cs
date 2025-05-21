using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text; // For SymmetricSecurityKey if used for testing

namespace GatewayService.Extensions
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Configures JWT Bearer authentication for the service. REQ-3-010.
        /// </summary>
        public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var securityConfigs = configuration.GetSection("SecurityConfigs");
            var jwtAuthority = securityConfigs["JwtAuthority"];
            var jwtAudience = securityConfigs["JwtAudience"];
            var jwtValidateIssuer = securityConfigs.GetValue<bool?>("JwtValidateIssuer") ?? true;
            var jwtValidateAudience = securityConfigs.GetValue<bool?>("JwtValidateAudience") ?? true;
            var jwtValidateLifetime = securityConfigs.GetValue<bool?>("JwtValidateLifetime") ?? true;
            var jwtValidateIssuerSigningKey = securityConfigs.GetValue<bool?>("JwtValidateIssuerSigningKey") ?? true;
            
            // For testing with a symmetric key directly from config (NOT FOR PRODUCTION)
            var testSymmetricKey = securityConfigs["TestSymmetricKey"];


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = jwtAuthority; // e.g., "https://identity.example.com"
                options.Audience = jwtAudience;   // e.g., "api_gateway_resource"
                
                options.RequireHttpsMetadata = !string.IsNullOrEmpty(jwtAuthority) && jwtAuthority.StartsWith("https://", StringComparison.OrdinalIgnoreCase); // Typically true for production

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtValidateIssuer,
                    ValidateAudience = jwtValidateAudience,
                    ValidateLifetime = jwtValidateLifetime,
                    ValidateIssuerSigningKey = jwtValidateIssuerSigningKey,
                    // ValidIssuer and ValidAudience can be set here if Authority is not used or for more specific validation
                    // ValidIssuer = jwtAuthority, 
                    // ValidAudience = jwtAudience,

                    // If using symmetric key for testing (NOT FOR PRODUCTION)
                    // IssuerSigningKey = !string.IsNullOrEmpty(testSymmetricKey) 
                    //                      ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testSymmetricKey)) 
                    //                      : null,

                    ClockSkew = TimeSpan.FromMinutes(securityConfigs.GetValue<int?>("JwtClockSkewMinutes") ?? 5) // Default is 5 minutes
                };

                // Custom event handlers if needed
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures
                        // context.Exception contains details
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Perform custom validation after token is validated by the handler
                        // var tokenValidationService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidationService>();
                        // await tokenValidationService.AdditionalValidationAsync(context.Principal);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        // Custom response on challenge (e.g., if token is missing or invalid)
                        // context.HandleResponse(); // Prevents default challenge logic
                        // context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        // await context.Response.WriteAsJsonAsync(new { error = "Unauthorized", message = "Authentication token is missing or invalid." });
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}