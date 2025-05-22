using IndustrialAutomation.OpcClient.Application.Interfaces;
// using IndustrialAutomation.OpcClient.Application.Services; // Will be uncommented when services are added
// using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da; // For concrete OPC clients
// using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ua;
// using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa;
// using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda;
// using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.Configuration;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Infrastructure.Logging;
using IndustrialAutomation.OpcClient.Infrastructure.Policies;
// using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
// using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Messaging;
// using IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.ErrorHandling;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Resilience;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.Extensions;

/// <summary>
/// Provides convenient extension methods for IServiceCollection to encapsulate 
/// the registration of various services (application, domain, infrastructure) and their dependencies.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpcClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration Providers
        services.AddSingleton<AppSettingsProvider>();

        // Application Services (Interfaces)
        // TODO: Register concrete implementations for these interfaces once created
        // services.AddSingleton<IOpcCommunicationService, OpcCommunicationService>();
        // services.AddSingleton<IDataTransmissionService, DataTransmissionService>();
        // services.AddSingleton<IConfigurationManagementService, ConfigurationManagementService>();
        // services.AddSingleton<IEdgeIntelligenceService, EdgeIntelligenceService>();

        // Infrastructure - OPC Clients (Interfaces)
        // TODO: Register concrete implementations for these interfaces once created
        // services.AddTransient<IOpcDaClient, OpcDaClient>();
        // services.AddSingleton<IOpcUaClient, OpcUaClient>(); // Singleton if manages long-lived sessions
        // services.AddTransient<IOpcXmlDaClient, OpcXmlDaClient>();
        // services.AddTransient<IOpcHdaClient, OpcHdaClient>();
        // services.AddTransient<IOpcAcClient, OpcAcClient>();
        // services.AddSingleton<UaSubscriptionManager>(); // Manages UA subscriptions

        // Infrastructure - Configuration
        // TODO: Register concrete implementations
        // services.AddTransient<ITagConfigurationImporter, FileTagConfigurationImporter>();

        // Infrastructure - Data Handling
        // TODO: Register concrete implementations
        // services.AddSingleton<IDataBufferer, InMemoryDataBufferer>();
        // services.AddTransient<IClientSideDataValidator, ClientSideDataValidator>();

        // Infrastructure - Logging
        // TODO: Register concrete implementations
        // services.AddSingleton<ICriticalWriteLogger, SerilogCriticalWriteLogger>();

        // Infrastructure - Policies
        // TODO: Register concrete implementations
        // services.AddSingleton<IWriteOperationLimiter, ConfigurableWriteOperationLimiter>();

        // Infrastructure - Server Connectivity
        // TODO: Register concrete implementations for IServerAppGrpcClient and IServerAppMessageProducer
        // services.AddSingleton<IServerAppGrpcClient, ServerAppGrpcClient>();
        // services.AddSingleton<MessageProducerFactory>();
        // services.AddSingleton<IServerAppMessageProducer>(sp => sp.GetRequiredService<MessageProducerFactory>().CreateMessageProducer());


        // Infrastructure - Edge Intelligence
        // TODO: Register concrete implementations
        // services.AddSingleton<IEdgeAiExecutor, OnnxEdgeAiExecutor>();
        // services.AddSingleton<IModelRepository, ModelRepository>();

        // Cross-Cutting Concerns
        services.AddSingleton<GlobalExceptionHandler>();
        services.AddSingleton<RetryPolicyProvider>();
        services.AddSingleton<SecureChannelConfigurator>();

        // Register Polly policies (example)
        var retryPolicyProvider = new RetryPolicyProvider(configuration);
        services.AddSingleton<Resilience.RetryPolicyProvider>(retryPolicyProvider); // Use the fully qualified name

        // Options binding (examples - these would bind to sections in appsettings.json)
        // services.Configure<List<ServerConnectionConfigDto>>(configuration.GetSection("OpcClient:ServerConnections"));
        // services.Configure<TagImportConfigDto>(configuration.GetSection("OpcClient:TagImport"));
        // services.Configure<List<ValidationRule>>(configuration.GetSection("DataHandling:ValidationRules"));
        // services.Configure<List<WriteLimitPolicy>>(configuration.GetSection("WriteLimits"));


        // HttpClient for XML-DA, gRPC (if not using Grpc.Net.ClientFactory)
        services.AddHttpClient();

        return services;
    }
}