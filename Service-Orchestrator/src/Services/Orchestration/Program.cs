using OrchestrationService.Extensions;
using OrchestrationService.Infrastructure.Persistence;
using WorkflowCore.Interface;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
// Read Serilog configuration from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console() // Default console sink, can be customized further in appsettings
    .CreateBootstrapLogger(); // Use CreateBootstrapLogger for early logging, then CreateLogger later if needed

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()); // Configure Serilog fully once host is built

Log.Information("Starting Orchestration Service...");

try
{
    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Service-Orchestrator API", Version = "v1" });
    });

    // Add custom services from extension methods
    builder.Services.AddWorkflowServices(builder.Configuration);
    builder.Services.AddHttpClients(builder.Configuration);
    // The actual message bus integration for event handlers would be configured here.
    // For now, AddEventHandlers might just register the IHostedService.
    builder.Services.AddEventHandlers();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service-Orchestrator API V1");
        });

        // Apply EF Core migrations or ensure DB is created in development
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<OrchestrationDbContext>();
                // In a real app, use Migrations: dbContext.Database.Migrate();
                // For simplicity here, EnsureCreated is used. This is not suitable for production.
                dbContext.Database.EnsureCreated();
                Log.Information("Database checked/created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating or creating the database.");
            }
        }
    }

    // app.UseHttpsRedirection(); // Typically handled by a reverse proxy in production

    app.UseRouting();

    app.UseAuthorization(); // Ensure this is after UseRouting and before MapControllers

    app.MapControllers();

    // Get and start the workflow host
    var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();
    // The WorkflowHostExtensions.ConfigureWorkflowHost(workflowHost, app.Services) could be called here
    // if it contained specific host configurations not handled by DI.
    // For now, assuming all registration happens via DI.

    await workflowHost.StartAsync(app.Lifetime.ApplicationStopping); // Pass CancellationToken

    Log.Information("Orchestration Service host started.");

    // Run the application
    await app.RunAsync();

    // Ensure workflow host stops on shutdown
    Log.Information("Orchestration Service stopping. Attempting to stop workflow host...");
    await workflowHost.StopAsync(CancellationToken.None); // Or use a proper CancellationToken for graceful shutdown
    Log.Information("Workflow host stopped.");

}
catch (Exception ex)
{
    Log.Fatal(ex, "Orchestration Service host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}