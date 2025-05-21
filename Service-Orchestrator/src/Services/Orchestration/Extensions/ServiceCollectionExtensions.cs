using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchestrationService.Application.EventHandlers;
using OrchestrationService.Configuration;
using OrchestrationService.Infrastructure.HttpClients.AiService;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Infrastructure.HttpClients.ManagementService;
using OrchestrationService.Infrastructure.Persistence;
using OrchestrationService.Workflows.BlockchainSync;
using OrchestrationService.Workflows.BlockchainSync.Activities;
using OrchestrationService.Workflows.ReportGeneration;
using OrchestrationService.Workflows.ReportGeneration.Activities;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using WorkflowCore.Interface;

namespace OrchestrationService.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to simplify service registration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds orchestrator-specific services to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddOrchestratorServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Options
            services.AddOptions<WorkflowEngineSettings>()
                .Bind(configuration.GetSection("WorkflowEngine"))
                .ValidateDataAnnotations();

            services.AddOptions<ServiceEndpoints>()
                .Bind(configuration.GetSection("ServiceEndpoints"))
                .ValidateDataAnnotations();

            // Configure WorkflowCore
            // The IWorkflowHost is registered by AddWorkflow() itself.
            // services.AddSingleton<IWorkflowHost, WorkflowHost>(); // No longer needed if AddWorkflow used

            // Configure Persistence
            WorkflowPersistenceSetup.ConfigurePersistence(services, configuration);

            // Register Workflow Definitions (Sagas)
            services.AddWorkflowDSL(); // Required for JSON/YAML workflows, but also good practice
            services.AddTransient<ReportGenerationSaga>();
            services.AddTransient<BlockchainSyncSaga>();

            // Register Workflow Activities
            // Report Generation Activities
            services.AddTransient<InitiateAiAnalysisActivity>();
            services.AddTransient<RetrieveHistoricalDataActivity>();
            services.AddTransient<GenerateReportDocumentActivity>();
            services.AddTransient<DistributeReportActivity>();
            services.AddTransient<ArchiveReportActivity>();
            services.AddTransient<ReportValidationStepActivity>();
            services.AddTransient<CompensateReportGenerationActivity>();

            // Blockchain Sync Activities
            services.AddTransient<PrepareBlockchainDataActivity>();
            services.AddTransient<StoreOffChainDataActivity>();
            services.AddTransient<CommitToBlockchainActivity>();
            services.AddTransient<CompensateBlockchainSyncActivity>();
            
            // Register HTTP Clients
            services.AddHttpClients(configuration);

            // Register Event Handlers (as HostedServices or similar depending on message bus integration)
            // For simplicity, registering as a singleton that might be manually triggered or part of a hosted service.
            services.AddSingleton<WorkflowTriggerHandler>();
            // If using a message bus like RabbitMQ/Kafka, these would be consumers registered as HostedServices.
            // e.g., services.AddHostedService<RabbitMqWorkflowTriggerConsumer>();

            return services;
        }

        /// <summary>
        /// Configures and registers HTTP clients for communicating with other microservices.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceEndpoints = configuration.GetSection("ServiceEndpoints").Get<ServiceEndpoints>() 
                ?? throw new InvalidOperationException("ServiceEndpoints configuration section is missing.");

            Action<HttpClient> configureClientDefault = client =>
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                // Add other default headers if needed, e.g., User-Agent
            };
            
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException, 5XX and 408
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Log retry attempts using ILogger if available via context or DI
                        Console.WriteLine($"Retrying HTTP request... attempt {retryAttempt}, outcome: {outcome.Result?.StatusCode}, delay: {timespan.TotalSeconds}s");
                    });

            services.AddHttpClient<IAiServiceClient, AiServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceEndpoints.AiServiceUrl ?? throw new ArgumentNullException(nameof(serviceEndpoints.AiServiceUrl)));
                configureClientDefault(client);
            }).AddPolicyHandler(retryPolicy);

            services.AddHttpClient<IDataServiceClient, DataServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceEndpoints.DataServiceUrl ?? throw new ArgumentNullException(nameof(serviceEndpoints.DataServiceUrl)));
                configureClientDefault(client);
            }).AddPolicyHandler(retryPolicy);

            services.AddHttpClient<IIntegrationServiceClient, IntegrationServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceEndpoints.IntegrationServiceUrl ?? throw new ArgumentNullException(nameof(serviceEndpoints.IntegrationServiceUrl)));
                configureClientDefault(client);
            }).AddPolicyHandler(retryPolicy);

            services.AddHttpClient<IManagementServiceClient, ManagementServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceEndpoints.ManagementServiceUrl ?? throw new ArgumentNullException(nameof(serviceEndpoints.ManagementServiceUrl)));
                configureClientDefault(client);
            }).AddPolicyHandler(retryPolicy);

            return services;
        }
    }
}