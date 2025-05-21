using GatewayService.Extensions;
using GatewayService.Middleware;
using GatewayService.Services;
using GatewayService.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using GatewayService.GraphQL.Schemas; // For AppSchema
using Microsoft.AspNetCore.Diagnostics.HealthChecks; // For HealthCheckOptions
using HealthChecks.UI.Client; // For UIResponseWriter
using Serilog; // Assuming Serilog might be used, add placeholder or actual integration

namespace GatewayService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. Configuration ---
            // Load appsettings.json, environment-specific appsettings, ocelot.json, yarp.json
            // ASP.NET Core loads appsettings.json and appsettings.{Environment}.json by default.
            // Ocelot configuration
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                                 .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            // YARP configuration (ensure yarp.json is structured with a "Yarp" root key or adjust YarpConfigurationExtensions)
            builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true)
                                 .AddJsonFile($"yarp.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddCommandLine(args);


            // --- 2. Logging ---
            // Clear default providers
            // builder.Logging.ClearProviders();
            // Add Console, Debug, etc.
            // builder.Logging.AddConsole();
            // builder.Logging.AddDebug();
            // Placeholder for Serilog or other advanced logging
            // Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(builder.Configuration)
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .CreateLogger();
            // builder.Host.UseSerilog();
            // For now, use default ASP.NET Core logging
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                // Add OpenTelemetry logging here if desired
            });


            // --- 3. Services Configuration ---
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API Gateway Service", Version = "v1" });
                // Add JWT Authentication to Swagger UI
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            // JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Polly Resilience Policies
            builder.Services.AddPollyPolicies(builder.Configuration);

            // Distributed Caching (Redis example)
            var cacheSettings = builder.Configuration.GetSection("CacheSettings");
            if (cacheSettings.GetValue<bool>("Enabled"))
            {
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = cacheSettings["RedisConnectionString"];
                    options.InstanceName = cacheSettings["RedisInstanceName"];
                });
                builder.Services.AddSingleton<IDistributedCacheService, DistributedCacheService>();
            }
            else // Fallback to in-memory cache if Redis is not enabled (primarily for dev/testing)
            {
                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddSingleton<IDistributedCacheService, DistributedCacheService>(); // Still wraps IDistributedCache
            }


            // Custom Services
            builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();


            // Ocelot Gateway Configuration (includes registering Ocelot Delegating Handlers)
            builder.Services.AddOcelotGateway(builder.Configuration);

            // YARP Reverse Proxy Configuration
            builder.Services.AddYarpReverseProxy(builder.Configuration);

            // GraphQL Configuration
            builder.Services.AddGraphQLServices(builder.Configuration, builder.Environment);

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddCheck<DownstreamServiceHealthCheck>("ManagementServiceHealthCheck",
                    tags: new[] { "downstream", "management" },
                    args: new object[] { builder.Configuration["ServiceEndpoints:ManagementServiceHttp"] ?? "http://localhost:5001", "/health" }) // Example
                .AddCheck<DownstreamServiceHealthCheck>("AiServiceHealthCheck",
                    tags: new[] { "downstream", "ai" },
                    args: new object[] { builder.Configuration["ServiceEndpoints:AiServiceHttp"] ?? "http://localhost:5002", "/health" }) // Example
                .AddUrlGroup(new Uri(builder.Configuration.GetSection("SecurityConfigs")["JwtAuthority"] + "/.well-known/openid-configuration"), name: "IdentityProviderHealth", tags: new[] {"security", "idp"});


            // CORS Configuration
            var allowedCorsOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new string[0];
            if (allowedCorsOrigins.Length > 0 && (allowedCorsOrigins.Contains("*") || allowedCorsOrigins.Any(o => !string.IsNullOrWhiteSpace(o))))
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("DefaultCorsPolicy", policy =>
                    {
                        if (allowedCorsOrigins.Contains("*"))
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyMethod()
                                  .AllowAnyHeader();
                        }
                        else
                        {
                            policy.WithOrigins(allowedCorsOrigins)
                                  .AllowAnyMethod()
                                  .AllowAnyHeader()
                                  .AllowCredentials(); // If you need to support credentials
                        }
                    });
                });
            }


            // --- 4. HTTP Request Pipeline Configuration ---
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway Service v1"));
            }
            else
            {
                app.UseExceptionHandler("/error"); // Example: redirect to a generic error handler
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            }

            // app.UseHttpsRedirection(); // Consider if Kestrel is edge-facing or behind a load balancer handling SSL

            if (allowedCorsOrigins.Length > 0)
            {
                app.UseCors("DefaultCorsPolicy");
            }

            app.UseRouting();

            // ASP.NET Core Middleware (if not handled by Ocelot Delegating Handlers for Ocelot routes)
            // Correlation ID handling can be an ASP.NET Core middleware for all requests
            // app.UseMiddleware<CorrelationIdMiddleware>();
            // Logging middleware can be an ASP.NET Core middleware for all requests
            // app.UseMiddleware<UnifiedLoggingMiddleware>();

            app.UseMiddleware<SchemaValidationMiddleware>(); // For validating requests/responses against schemas

            if (app.Configuration.GetValue<bool>("FeatureFlags:EnableMqttBridge"))
            {
                app.UseMiddleware<MqttProtocolHandlerMiddleware>();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets(); // For GraphQL Subscriptions or other WebSocket features

            // Map Endpoints
            app.MapGraphQL<AppSchema>("/graphql"); // GraphQL endpoint registered by GraphQL.NET
            if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableGraphQLPlayground", false))
            {
                 // UseGraphQLPlayground has options for path, etc.
                app.UseGraphQLPlayground(new GraphQL.Server.Ui.Playground.PlaygroundOptions
                {
                    Path = "/ui/playground" // Default path
                });
            }

            app.MapHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true, // Include all health checks
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // Nicer JSON output
            });
            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }); // Liveness
            app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") }); // Readiness

            app.MapControllers(); // For GraphQLController (if used) or other API controllers

            app.MapReverseProxy(); // YARP endpoints

            // Ocelot middleware should be placed carefully.
            // If Ocelot is the primary router, it might come earlier.
            // If it handles routes not covered by above endpoints, this placement is okay.
            // Ocelot's UseOcelot is typically terminal for matched routes.
            app.UseOcelot().Wait();


            // --- 5. Run Application ---
            app.Run();
        }
    }
}