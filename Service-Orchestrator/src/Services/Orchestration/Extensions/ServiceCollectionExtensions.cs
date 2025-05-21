using OrchestrationService.Configuration;
using OrchestrationService.Infrastructure.Persistence;
using WorkflowCore.Interface;
using OrchestrationService.Workflows.ReportGeneration;
using OrchestrationService.Workflows.BlockchainSync;
using OrchestrationService.Workflows.ReportGeneration.Activities;
using OrchestrationService.Workflows.BlockchainSync.Activities;
using OrchestrationService.Infrastructure.HttpClients.AiService;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Infrastructure.HttpClients.ManagementService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OrchestrationService.Application.EventHandlers;
using System; // For Uri

namespace OrchestrationService.Extensions;

/// <summary>
/// Provides extension methods for `IServiceCollection` to simplify the registration of services in `Program.cs`.
/// Encapsulates registration logic for workflow engine, sagas, activities, persistence,
/// HTTP clients, configuration objects, and event handlers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures services related to the WorkflowCore engine, including sagas, activities, and persistence.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Workflow Engine Settings from appsettings.json
        services.Configure<WorkflowEngineSettings>(configuration.GetSection("WorkflowEngineSettings"));
        var workflowSettings = configuration.GetSection("WorkflowEngineSettings").Get<WorkflowEngineSettings>() ?? new WorkflowEngineSettings();

        // Add WorkflowCore services
        services.AddWorkflow(cfg =>
        {
            // Configure persistence - this calls the extension method in WorkflowPersistenceSetup.cs
            // which registers OrchestrationDbContext and configures WorkflowCore to use it.
            services.AddWorkflowPersistence(configuration);
            // Example: cfg.UseEntityFrameworkPersistence<OrchestrationDbContext>(); // This is effectively what AddWorkflowPersistence helps set up.

            // Configure polling interval, error retry interval, etc.
            cfg.UsePollInterval(TimeSpan.FromSeconds(workflowSettings.PollingIntervalInSeconds));
            // Note: Default error retry interval can be set here, but individual steps often define their own.
            // cfg.UseErrorRetryInterval(TimeSpan.FromSeconds(workflowSettings.ErrorRetryIntervalSeconds));


            // Register Workflows (Sagas)
            cfg.AddWorkflow<ReportGenerationSaga, ReportGenerationSagaData>();
            cfg.AddWorkflow<BlockchainSyncSaga, BlockchainSyncSagaData>();

            // Activities are typically registered as transient services below,
            // and WorkflowCore's DI integration will resolve them.
            // If you needed to register steps directly with WorkflowCore's config:
            // cfg.AddStep<InitiateAiAnalysisActivity>();
        });

        // Register Activities (Step Bodies) as transient services for DI
        services.AddTransient<InitiateAiAnalysisActivity>();
        services.AddTransient<RetrieveHistoricalDataActivity>();
        services.AddTransient<GenerateReportDocumentActivity>();
        services.AddTransient<DistributeReportActivity>();
        services.AddTransient<ArchiveReportActivity>();
        services.AddTransient<ReportValidationStepActivity>();
        services.AddTransient<CompensateReportGenerationActivity>();

        services.AddTransient<PrepareBlockchainDataActivity>();
        services.AddTransient<StoreOffChainDataActivity>();
        services.AddTransient<CommitToBlockchainActivity>();
        services.AddTransient<CompensateBlockchainSyncActivity>();

        return services;
    }

    /// <summary>
    /// Adds and configures HTTP clients for communicating with external microservices.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Service Endpoints from appsettings.json
        services.Configure<ServiceEndpoints>(configuration.GetSection("ServiceEndpoints"));
        var serviceEndpoints = configuration.GetSection("ServiceEndpoints").Get<ServiceEndpoints>();

        if (serviceEndpoints == null)
        {
            var logger = services.BuildServiceProvider().GetService<ILogger<ServiceCollectionExtensions>>();
            logger?.LogCritical("ServiceEndpoints configuration section is missing. HTTP clients will not be configured with base addresses.");
            // Application might fail later if these are essential.
        }

        // Register HTTP Clients using IHttpClientFactory for resilience and proper lifetime management
        services.AddHttpClient<IAiServiceClient, AiServiceClient>(client =>
        {
            if (!string.IsNullOrEmpty(serviceEndpoints?.AiServiceUrl))
                client.BaseAddress = new Uri(serviceEndpoints.AiServiceUrl);
        });

        services.AddHttpClient<IDataServiceClient, DataServiceClient>(client =>
        {
            if (!string.IsNullOrEmpty(serviceEndpoints?.DataServiceUrl))
                client.BaseAddress = new Uri(serviceEndpoints.DataServiceUrl);
        });

        services.AddHttpClient<IIntegrationServiceClient, IntegrationServiceClient>(client =>
        {
            if (!string.IsNullOrEmpty(serviceEndpoints?.IntegrationServiceUrl))
                client.BaseAddress = new Uri(serviceEndpoints.IntegrationServiceUrl);
        });

        services.AddHttpClient<IManagementServiceClient, ManagementServiceClient>(client =>
        {
            if (!string.IsNullOrEmpty(serviceEndpoints?.ManagementServiceUrl))
                client.BaseAddress = new Uri(serviceEndpoints.ManagementServiceUrl);
        });

        return services;
    }

    /// <summary>
    /// Adds event handlers, typically for consuming messages from a message bus and triggering workflows.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Register WorkflowTriggerHandler as a hosted service.
        // This assumes WorkflowTriggerHandler implements IHostedService and contains
        // logic to connect to a message bus and consume relevant events.
        // The actual message bus integration (e.g., RabbitMQ, Kafka client setup)
        // would be part of WorkflowTriggerHandler's implementation or a separate shared library.
        services.AddHostedService<WorkflowTriggerHandler>();

        // If using a specific message bus library like MassTransit or Rebus,
        // their specific DI extensions would be used here. For example:
        // services.AddMassTransit(x => {
        //     x.AddConsumer<MyEventConsumer>(); // MyEventConsumer would contain logic similar to WorkflowTriggerHandler
        //     x.UsingRabbitMq((context, cfg) => { /* RabbitMQ config */ });
        // });

        return services;
    }
}