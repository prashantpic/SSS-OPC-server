using OrchestrationService.Configuration;
using OrchestrationService.Extensions;
using OrchestrationService.Infrastructure.WorkflowEngine;
using Serilog;
using WorkflowCore.Interface;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog for structured logging
        // Assuming REPO-SHARED-UTILS provides a standard Serilog configuration extension,
        // or configure it directly here. For now, a basic console logger.
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger(); // Use bootstrap logger for early DI issues

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        // Add services to the container.
        builder.Services.AddOrchestratorServices(builder.Configuration);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Service Orchestrator API",
                Version = "v1",
                Description = "API for managing and interacting with workflows in the Service Orchestrator."
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service Orchestrator API V1");
            });
        }

        // Placeholder for Shared.Utilities exception handling middleware
        // app.UseCustomExceptionHandler(); 

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        // Start Workflow Host
        var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();
        workflowHost.RegisterWorkflows(app.Services); // Extension method to register workflows and activities
        
        // Ensure graceful shutdown for WorkflowCore
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            Log.Information("Application is stopping. Stopping Workflow Host...");
            workflowHost.Stop();
            Log.Information("Workflow Host stopped.");
        });
        
        try
        {
            Log.Information("Starting web host and Workflow Host.");
            await workflowHost.StartAsync(CancellationToken.None); // Start WorkflowCore Host
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.Information("Shutting down Serilog...");
            await Log.CloseAndFlushAsync();
        }
    }
}