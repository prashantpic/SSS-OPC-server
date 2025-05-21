using ManagementService.Application.Abstractions.Clients;
using ManagementService.Application.Abstractions.Data;
using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Application.Abstractions.Services;
using ManagementService.Application.Features.ConfigurationMigrations.Services; // For IConfigurationFileParserFactory
using ManagementService.Infrastructure.ConfigurationMigration.Parsers;
using ManagementService.Infrastructure.ConfigurationMigration.Transformers;
using ManagementService.Infrastructure.ConfigurationMigration.Validators;
using ManagementService.Infrastructure.HttpClients;
using ManagementService.Infrastructure.Persistence;
using ManagementService.Infrastructure.Persistence.Repositories;
using ManagementService.Infrastructure.Services; // For IDeploymentOrchestrationClient
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ManagementService.Api.Middleware; // For ErrorHandlerMiddleware

namespace ManagementService.Infrastructure;

/// <summary>
/// Centralizes the registration of all infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext and configure EF Core
        services.AddDbContext<ManagementDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("ManagementDbContext"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    // sqlOptions.EnableRetryOnFailure( // Optional: configure EF Core level retry
                    //    maxRetryCount: 5,
                    //    maxRetryDelay: TimeSpan.FromSeconds(30),
                    //    errorNumbersToAdd: null);
                }));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IClientInstanceRepository, ClientInstanceRepository>();
        services.AddScoped<IClientConfigurationRepository, ClientConfigurationRepository>();
        services.AddScoped<IBulkOperationJobRepository, BulkOperationJobRepository>();
        services.AddScoped<IConfigurationMigrationJobRepository, ConfigurationMigrationJobRepository>();

        // Register API Clients
        // HttpClient configuration is done in Program.cs using IHttpClientFactory
        services.AddScoped<IDataServiceApiClient, DataServiceApiClient>();
        services.AddScoped<IDeploymentOrchestrationClient, DeploymentOrchestrationClient>();

        // Register Configuration Migration Components
        services.AddScoped<IConfigurationFileParserFactory, ConfigurationFileParserFactory>();
        services.AddScoped<CsvConfigurationParser>();
        services.AddScoped<XmlConfigurationParser>();
        services.AddScoped<DefaultConfigurationTransformer>();
        services.AddScoped<MigratedConfigurationValidator>();

        // Register Middleware (can also be added directly in Program.cs)
        services.AddTransient<ErrorHandlerMiddleware>();

        return services;
    }
}