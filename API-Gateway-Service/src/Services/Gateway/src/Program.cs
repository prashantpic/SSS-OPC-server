using GatewayService.Extensions;
using GatewayService.GraphQL.Schemas;
using GatewayService.HealthChecks;
using GatewayService.Middleware;
using GatewayService.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration.Json;
using Ocelot.Middleware;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Load configurations
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("yarp.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure Serilog (assuming Shared-Utilities or similar setup)
// For standalone, basic Serilog setup:
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog();

var featureFlags = builder.Configuration.GetSection("FeatureFlags").Get<string[]>() ?? Array.Empty<string>();

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI if added later
builder.Services.AddSwaggerGen(); // For Swagger/OpenAPI if added later

builder.Services.AddHealthChecks()
    .AddCheck<DownstreamServiceHealthCheck>("management-service-health", tags: new[] { "downstream" }, args: new object[] { "ManagementServiceBaseUrl", "Management Service" })
    .AddCheck<DownstreamServiceHealthCheck>("ai-service-health", tags: new[] { "downstream" }, args: new object[] { "AIServiceBaseUrl", "AI Service" })
    .AddCheck<DownstreamServiceHealthCheck>("data-service-health", tags: new[] { "downstream" }, args: new object[] { "DataServiceBaseUrl", "Data Service" })
    .AddCheck<DownstreamServiceHealthCheck>("integration-service-health", tags: new[] { "downstream" }, args: new object[] { "IntegrationServiceBaseUrl", "Integration Service" });
    // Potentially add self health check for Redis, etc.

var cacheSettings = builder.Configuration.GetSection("CacheSettings");
if (cacheSettings.GetValue<string>("Provider")?.Equals("Redis", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheSettings.GetValue<string>("ConnectionString");
        options.InstanceName = cacheSettings.GetValue<string>("InstanceNamePrefix", "Gateway_");
    });
}
else
{
    builder.Services.AddDistributedMemoryCache(); // Fallback or for local dev without Redis
}
builder.Services.AddSingleton<IDistributedCacheService, DistributedCacheService>();

builder.Services.AddJwtBearerAuthentication(builder.Configuration); // REQ-3-010
builder.Services.AddAuthorization(options =>
{
    // Example policy:
    // options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    // Ocelot RouteClaimsRequirement can also be used for route-specific authorization
});

var corsOrigins = builder.Configuration.GetSection("SecurityConfigs:AllowedCorsOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins?.Any() == true)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin() // More permissive, consider for development only
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddResiliencePolicies(builder.Configuration); // Polly policies for HttpClientFactory
builder.Services.AddHttpContextAccessor(); // Required by some Ocelot features and custom handlers

builder.Services.AddOcelotServices(builder.Configuration);
builder.Services.AddYarpServices(builder.Configuration);

if (featureFlags.Contains("enableGraphQLSupport", StringComparer.OrdinalIgnoreCase))
{
    builder.Services.AddGraphQLServices();
}

builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();

// Placeholder for SchemaValidationMiddleware registration if it needs specific services
// builder.Services.AddScoped<ISchemaRegistry, SchemaRegistry>(); // Example

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error"); // Requires an error handling endpoint or middleware
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(); // Apply CORS policy

// Ocelot's built-in authentication middleware will handle JWT validation for routes configured with AuthenticationOptions.
// The JwtValidationDelegatingHandler is for custom/additional validation logic if Ocelot's isn't sufficient.
app.UseAuthentication(); // REQ-3-010
app.UseAuthorization();

// Custom Middleware Order - consider carefully
// Correlation ID should be early
app.UseMiddleware<CorrelationIdMiddleware>(); // Assuming CorrelationIdDelegatingHandler is registered as middleware for global effect
// Unified Logging should also be early to capture as much as possible
app.UseMiddleware<UnifiedLoggingMiddleware>(); // Assuming UnifiedLoggingDelegatingHandler is registered as middleware

if (featureFlags.Contains("enableSchemaValidation", StringComparer.OrdinalIgnoreCase))
{
    app.UseMiddleware<SchemaValidationMiddleware>();
}

if (featureFlags.Contains("enableMqttProtocolHandler", StringComparer.OrdinalIgnoreCase))
{
    // Ensure MqttProtocolHandlerMiddleware does not interfere with YARP/Ocelot if not targeted
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/mqtt-gateway", StringComparison.OrdinalIgnoreCase), // Example path condition
        appBuilder => appBuilder.UseMiddleware<MqttProtocolHandlerMiddleware>()
    );
}


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // For GraphQLController or other API controllers on the gateway itself

    if (featureFlags.Contains("enableGraphQLSupport", StringComparer.OrdinalIgnoreCase))
    {
         // Standard way to map GraphQL endpoint, often uses /graphql
         // Make sure this doesn't conflict with Ocelot/YARP upstream paths
         // Option 1: Map via controller (if GraphQLController is used)
         // endpoints.MapControllers(); is usually enough if GraphQLController has [Route("graphql")]

         // Option 2: Direct mapping if not using a controller
         // endpoints.MapGraphQL<AppSchema>(); // Requires GraphQL.Server.Ui.Playground or similar for UI
    }

    if (featureFlags.Contains("enableYarpIntegration", StringComparer.OrdinalIgnoreCase))
    {
        endpoints.MapReverseProxy(); // YARP handles its configured routes
    }
});

// Health Checks Endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true, // Serves all health checks
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration
                }),
                totalDuration = report.TotalDuration
            },
            new JsonSerializerOptions { WriteIndented = true }
        );
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Name.Contains("self") }); // Liveness
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => !r.Name.Contains("self") }); // Readiness (dependencies)


// Ocelot must be registered after routing and YARP if YARP is to take precedence for some routes.
// Or, Ocelot can be primary, and YARP handles specific cases not covered by Ocelot.
// The current setup maps YARP via UseEndpoints, and Ocelot is terminal.
// If Ocelot is primary, its middleware usually comes before app.UseEndpoints.
// For Ocelot to act as the main router for non-YARP routes:
await app.UseOcelot();

app.Run();