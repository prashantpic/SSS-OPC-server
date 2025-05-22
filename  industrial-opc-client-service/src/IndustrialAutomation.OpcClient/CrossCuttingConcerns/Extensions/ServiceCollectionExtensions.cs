using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.Services; // Assuming concrete services will be in this namespace
using IndustrialAutomation.OpcClient.Domain.Models;
using IndustrialAutomation.OpcClient.Infrastructure.Configuration;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence;
using IndustrialAutomation.OpcClient.Infrastructure.Logging;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ua;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa;
using IndustrialAutomation.OpcClient.Infrastructure.Policies;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Messaging;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.ErrorHandling;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Resilience;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly; // For RetryPolicyProvider
using System.Linq; // For TryAddEnumerable
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration; // For Options binding

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.Extensions
{
    /// <summary>
    /// Provides convenient extension methods for IServiceCollection 
    /// to encapsulate the registration of various services (application, domain, infrastructure) 
    /// and their dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpcClientServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration Providers
            services.AddSingleton<AppSettingsProvider>();

            // Application Services (Interfaces with their future concrete implementations)
            // For now, if concrete implementations are not generated, these would fail unless mocked.
            // We will register interfaces for now. Concrete types to be added in other iterations.
            services.AddSingleton<IOpcCommunicationService, OpcCommunicationService>(); // Placeholder if OpcCommunicationService is not yet implemented
            services.AddSingleton<IDataTransmissionService, DataTransmissionService>(); // Placeholder
            services.AddSingleton<IConfigurationManagementService, ConfigurationManagementService>(); // Placeholder
            services.AddSingleton<IEdgeIntelligenceService, EdgeIntelligenceService>(); // Placeholder

            // Infrastructure - OPC Clients (Register interfaces, concrete implementations to follow)
            services.AddTransient<IOpcDaClient, OpcDaClient>(); // Placeholder
            services.AddSingleton<IOpcUaClient, OpcUaClient>(); // Placeholder (often singleton due to connection management)
            services.AddTransient<IOpcXmlDaClient, OpcXmlDaClient>(); // Placeholder
            services.AddTransient<IOpcHdaClient, OpcHdaClient>(); // Placeholder
            services.AddTransient<IOpcAcClient, OpcAcClient>();   // Placeholder
            services.AddSingleton<UaSubscriptionManager>();      // Placeholder
            services.AddSingleton<OpcValueConverter>();

            // Infrastructure - Configuration
            services.AddSingleton<ITagConfigurationImporter, FileTagConfigurationImporter>(); // Placeholder

            // Infrastructure - Data Handling
            services.AddSingleton<IClientSideDataValidator, ClientSideDataValidator>(); // Placeholder
            services.AddSingleton<IDataBufferer, InMemoryDataBufferer>();             // Placeholder

            // Infrastructure - Logging
            services.AddSingleton<ICriticalWriteLogger, SerilogCriticalWriteLogger>(); // Placeholder

            // Infrastructure - Policies
            services.AddSingleton<IWriteOperationLimiter, ConfigurableWriteOperationLimiter>(); // Placeholder

            // Infrastructure - Server Connectivity
            services.AddSingleton<IServerAppGrpcClient, ServerAppGrpcClient>();         // Placeholder
            services.AddSingleton<MessageProducerFactory>();                          // Placeholder
            services.AddSingleton<IServerAppMessageProducer>(sp => sp.GetRequiredService<MessageProducerFactory>().CreateMessageProducer());

            // Infrastructure - Edge AI
            services.AddSingleton<IEdgeAiExecutor, OnnxEdgeAiExecutor>();   // Placeholder
            services.AddSingleton<IModelRepository, ModelRepository>();     // Placeholder

            // Cross-Cutting Concerns
            services.AddSingleton<GlobalExceptionHandler>();
            services.AddSingleton<RetryPolicyProvider>();
            services.AddSingleton<SecureChannelConfigurator>();
            
            // Options Pattern for configurations
            services.Configure<OpcClientSettings>(configuration.GetSection("OpcClient"));
            services.Configure<ServerAppSettings>(configuration.GetSection("ServerApp"));
            services.Configure<EdgeAiSettings>(configuration.GetSection("EdgeAI"));
            services.Configure<DataHandlingSettings>(configuration.GetSection("DataHandling"));
            
            // Example for list configurations that might be directly bound to IOptions<List<T>>
            // This requires the configuration sections to be arrays.
            services.Configure<List<ServerConnectionConfigDto>>(configuration.GetSection("OpcClient:ServerConnections"));
            services.Configure<List<UaSubscriptionConfigDto>>(configuration.GetSection("OpcClient:UaSubscriptions"));
            services.Configure<List<ValidationRule>>(configuration.GetSection("DataHandling:ValidationRules"));
            services.Configure<List<WriteLimitPolicy>>(configuration.GetSection("DataHandling:WriteLimits"));
            services.Configure<TagImportConfigDto>(configuration.GetSection("OpcClient:TagImport"));


            // HttpClient for XML-DA or other HTTP based services if needed
            services.AddHttpClient();

            return services;
        }
    }
}