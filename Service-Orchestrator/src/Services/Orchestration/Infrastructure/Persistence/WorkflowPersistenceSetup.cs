using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Persistence.EntityFrameworkCore; // For WorkflowCore persistence setup

namespace OrchestrationService.Infrastructure.Persistence;

/// <summary>
/// Contains extension methods or configuration logic to set up the chosen persistence provider
/// (e.g., EntityFrameworkCore) for WorkflowCore, enabling workflow state to be durably stored.
/// </summary>
public static class WorkflowPersistenceSetup
{
    /// <summary>
    /// Adds and configures workflow persistence using Entity Framework Core.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the WorkflowDb connection string is not configured.</exception>
    public static IServiceCollection AddWorkflowPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WorkflowDb");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("WorkflowDb connection string is not configured in ConnectionStrings section.");
        }

        // Register the OrchestrationDbContext for EF Core
        // This is used by WorkflowCore's EF persistence provider.
        services.AddDbContext<OrchestrationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                // Specify the assembly containing migrations if they are in this project
                sqlServerOptions.MigrationsAssembly(typeof(OrchestrationDbContext).Assembly.FullName);
            })
        );

        // Configure WorkflowCore to use EntityFrameworkCore for persistence.
        // This is typically done within the `services.AddWorkflow(...)` call.
        // So, this method ensures DbContext is registered, and the AddWorkflow call will use it.
        // The actual .UseEntityFrameworkPersistence<OrchestrationDbContext>() is done in ServiceCollectionExtensions.cs

        // Note on DurableTask: The prompt mentioned DurableTask.EntityFrameworkCore in the tech stack.
        // If DurableTask was the primary workflow engine, its persistence setup would be different,
        // e.g., using services.AddDurableTask(...).UseEntityFrameworkCorePersistence(...).
        // Since the file structure and templates point to WorkflowCore, this setup focuses on WorkflowCore.
        // If both engines were used side-by-side (unlikely for orchestration), each would need its own persistence config.

        return services;
    }
}