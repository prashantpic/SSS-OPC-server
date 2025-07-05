using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Serilog.Formatting.Json;

namespace Opc.System.ApiGateway;

/// <summary>
/// Main entry point for the API Gateway application.
/// </summary>
public class Program
{
    /// <summary>
    /// The main method that configures and runs the web application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Logging Configuration ---
        // Clear default providers and configure Serilog for structured logging.
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(new JsonFormatter()));
        
        // --- Service Registration (Dependency Injection) ---

        // Add services for observability (distributed tracing).
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddConsoleExporter();
            });

        // Add and configure YARP reverse proxy.
        // The configuration for routes and clusters is loaded from the "ReverseProxy" section
        // of appsettings.json, enabling dynamic routing.
        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        // Add JWT Bearer authentication services.
        // This configures the gateway to validate JWTs from the configured identity provider.
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // The authority is the URL of the identity provider (e.g., Keycloak, Azure AD).
                options.Authority = builder.Configuration["JwtSettings:Authority"];
                // The audience is the identifier of this API resource.
                options.Audience = builder.Configuration["JwtSettings:Audience"];
                // In production, this should be true. For development with local identity providers
                // that might use self-signed certificates, it can be set to false.
                options.RequireHttpsMetadata = false; 
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidateIssuer = true;
            });

        // Add authorization services and define policies.
        // Routes in YARP configuration can then refer to these policies.
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("DefaultAuthPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        var app = builder.Build();
        
        // --- Middleware Pipeline Configuration ---

        // In a production environment, enable HTTP Strict Transport Security (HSTS).
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // Redirect all HTTP requests to HTTPS.
        app.UseHttpsRedirection();
        
        // Add Serilog middleware for rich, structured request logging.
        app.UseSerilogRequestLogging();
        
        // Enable authentication middleware. This must come before authorization.
        app.UseAuthentication();
        
        // Enable authorization middleware.
        app.UseAuthorization();
        
        // Map the reverse proxy endpoints. This is the core of YARP's functionality,
        // it inspects the incoming request and forwards it based on the configured routes.
        app.MapReverseProxy();
        
        app.Run();
    }
}