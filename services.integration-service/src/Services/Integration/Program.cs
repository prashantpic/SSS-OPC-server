using Microsoft.AspNetCore.Diagnostics;
using MQTTnet;
using Serilog;
using Serilog.Formatting.Json;
using Services.Integration.Api.Hubs;
using Services.Integration.Application.Interfaces;
using Services.Integration.Application.UseCases.Iot;
using Services.Integration.Infrastructure.Adapters.Blockchain;
using Services.Integration.Infrastructure.Adapters.DigitalTwin;
using Services.Integration.Infrastructure.Connectors.Iot;
using Services.Integration.Infrastructure.Streaming;

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new JsonFormatter())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Integration Service host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new JsonFormatter()));

    // --- Add services to the DI container. ---

    // Load configuration from appsettings.json
    builder.Services.Configure<AzureIotSettings>(builder.Configuration.GetSection("AzureIotSettings"));
    builder.Services.Configure<EthereumSettings>(builder.Configuration.GetSection("EthereumSettings"));

    // Register Infrastructure services
    builder.Services.AddHttpClient(); // For IHttpClientFactory
    builder.Services.AddSingleton<IMqttFactory, MqttFactory>();

    // Register Adapters and Connectors (Strategy Pattern for IoT)
    builder.Services.AddSingleton<IIotPlatformConnector, AzureIotConnector>();
    // TODO: Register AwsIotConnector when implemented
    // builder.Services.AddSingleton<IIotPlatformConnector, AwsIotConnector>(); 
    
    builder.Services.AddSingleton<IBlockchainAdapter, EthereumAdapter>();
    builder.Services.AddSingleton<IArDataStreamer, ArDataStreamer>();
    builder.Services.AddSingleton<IDigitalTwinAdapter, AasDigitalTwinAdapter>();
    
    // Register Application Use Case Handlers
    // Placeholder registration for repository
    builder.Services.AddSingleton<IIntegrationEndpointRepository, // Replace with a real implementation (e.g., in-memory or EF Core)
        Services.Integration.Infrastructure.Persistence.InMemoryIntegrationEndpointRepository>(); 
    builder.Services.AddScoped<SendDataToIotPlatformHandler>();

    // Add SignalR and Health Checks
    builder.Services.AddSignalR();
    builder.Services.AddHealthChecks();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // --- Configure the HTTP request pipeline. ---

    // Global exception handler
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception caught by global handler");
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseRouting();

    // Authentication/Authorization would go here if APIs were secured
    // app.UseAuthentication();
    // app.UseAuthorization();
    
    app.MapHub<ArDataStreamerHub>("/hubs/arstreamer");
    app.MapHealthChecks("/health");

    app.MapGet("/", () => "Integration Service is healthy.")
       .WithTags("Status")
       .WithName("GetServiceStatus");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Integration Service host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// In a separate file normally, but included here for compilation of Program.cs
namespace Services.Integration.Infrastructure.Persistence
{
    using Domain.Aggregates;
    using Domain.ValueObjects;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    // Dummy in-memory repository for demonstration purposes
    public class InMemoryIntegrationEndpointRepository : IIntegrationEndpointRepository
    {
        private readonly ConcurrentDictionary<Guid, IntegrationEndpoint> _endpoints = new();

        public InMemoryIntegrationEndpointRepository()
        {
            // Seed with some data for testing
            var endpoint = IntegrationEndpoint.Create(
                "TestAzureIotDevice",
                EndpointType.AzureIot,
                new EndpointAddress("iothub.azure-devices.net", new Dictionary<string, string>
                {
                    { "HostName", "YourIoTHubName.azure-devices.net" },
                    { "DeviceId", "TestDevice1" },
                    { "SharedAccessKey", "YourSharedAccessKey" } // In real life, use Key Vault
                })
            );
            _endpoints.TryAdd(endpoint.Id, endpoint);
        }

        public Task<IntegrationEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _endpoints.TryGetValue(id, out var endpoint);
            return Task.FromResult(endpoint);
        }
    }
}

// Placeholder for Azure IoT settings, matching appsettings.json structure.
public class AzureIotSettings
{
    public string? DefaultConnectionString { get; set; }
}