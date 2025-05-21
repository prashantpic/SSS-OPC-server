using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchestrationService.Workflows.ReportGeneration;
using OrchestrationService.Workflows.ReportGeneration.Models;
using OrchestrationService.Workflows.BlockchainSync;
using OrchestrationService.Workflows.BlockchainSync.Models;
// Activities are typically registered with DI and resolved automatically by WorkflowCore.
// Explicit registration with host.RegisterActivity is usually for older versions or specific scenarios.
// WorkflowCore 7.x resolves steps from DI if they are registered as services.
// The SDS mentions host.RegisterActivity, which translates to host.RegisterType<TStep>() in newer versions if still needed.
// However, standard practice is to register activities as transient services and WorkflowCore picks them up.
// Let's follow SDS for `RegisterWorkflow` and assume activities are DI-registered.
// If host.RegisterActivity is strictly required, it's host.RegisterType<TStep>().

using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;


namespace OrchestrationService.Infrastructure.WorkflowEngine
{
    /// <summary>
    /// Provides extension methods for configuring and managing the <see cref="IWorkflowHost"/>.
    /// </summary>
    public static class WorkflowHostExtensions
    {
        /// <summary>
        /// Configures the workflow host by registering defined workflows and their activities.
        /// Activities are typically registered as services in DI (e.g., AddTransient TActivity ).
        /// This method focuses on registering the workflow definitions with the host.
        /// </summary>
        /// <param name="host">The workflow host to configure.</param>
        /// <param name="services">The service provider for resolving dependencies if needed (e.g., logging).</param>
        public static void ConfigureWorkflowDefinitions(this IWorkflowHost host, IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(WorkflowHostExtensions).FullName);
            logger.LogInformation("Registering workflow definitions with the Workflow Host...");

            // Register Workflows (Sagas)
            // WorkflowCore will resolve their steps (activities) from the DI container.
            host.RegisterWorkflow<ReportGenerationSaga, ReportGenerationSagaData>();
            logger.LogInformation("Registered ReportGenerationSaga.");

            host.RegisterWorkflow<BlockchainSyncSaga, BlockchainSyncSagaData>();
            logger.LogInformation("Registered BlockchainSyncSaga.");
            
            // As per SDS Section 8.3:
            // host.RegisterActivity<InitiateAiAnalysisActivity>(); ... and all other activities
            // In WorkflowCore 7, this is typically done by registering the activity type if it's not automatically
            // resolved from DI when used in a workflow. `host.RegisterType<TStep>()` is the method.
            // However, if activities are defined as `WorkflowStep<TActivity>` in `IWorkflow.Build`
            // and `TActivity` is registered in DI, this explicit registration might not be needed.
            // For safety/compliance with SDS, let's add them using RegisterType if that's the intent.

            // Example of explicit type registration if activities are not auto-resolved (usually they are via DI):
            // host.RegisterType(typeof(OrchestrationService.Workflows.ReportGeneration.Activities.InitiateAiAnalysisActivity));
            // host.RegisterType(typeof(OrchestrationService.Workflows.ReportGeneration.Activities.RetrieveHistoricalDataActivity));
            // ... and so on for all activities.
            // This is generally redundant if activities are registered in DI and used as steps.
            // The primary registration is services.AddWorkflowStep<MyActivity>() or services.AddTransient<MyActivity>()
            // in ServiceCollectionExtensions, then WorkflowCore resolves them.
            // The SDS might be based on an older WorkflowCore version's API for `RegisterActivity`.
            // For WorkflowCore 7, direct registration of activity types with the host is less common than DI.
            // We will rely on DI registration of activities in ServiceCollectionExtensions.

            logger.LogInformation("Workflow definitions registration complete.");
        }

        /// <summary>
        /// Starts the workflow host and its background processing tasks.
        /// </summary>
        /// <param name="host">The workflow host to start.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public static async Task StartWorkflowHostAsync(this IWorkflowHost host, CancellationToken cancellationToken = default)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            
            var loggerFactory = host.ServiceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(WorkflowHostExtensions).FullName) ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            logger.LogInformation("Starting Workflow Host...");
            await host.StartAsync(cancellationToken);
            logger.LogInformation("Workflow Host started successfully.");
        }

        /// <summary>
        /// Stops the workflow host and its background processing tasks gracefully.
        /// </summary>
        /// <param name="host">The workflow host to stop.</param>
        /// <param name="cancellationToken">A token to observe for cancellation requests during shutdown.</param>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public static async Task StopWorkflowHostAsync(this IWorkflowHost host, CancellationToken cancellationToken = default)
        {
             if (host == null) throw new ArgumentNullException(nameof(host));

            var loggerFactory = host.ServiceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(WorkflowHostExtensions).FullName) ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            
            logger.LogInformation("Stopping Workflow Host...");
            await host.StopAsync(cancellationToken); // WorkflowCore 7 uses StopAsync(CancellationToken)
            logger.LogInformation("Workflow Host stopped successfully.");
        }
    }
}