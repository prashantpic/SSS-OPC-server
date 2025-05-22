using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Domain.Models; // For ValidationRule, WriteLimitPolicy
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace IndustrialAutomation.OpcClient.Infrastructure.Configuration
{
    // Placeholder for detailed config structures that would be in appsettings.json
    // These would typically be more nested, e.g., OpcClient: { ClientId: "...", ServerConnections: [...] }
    // For simplicity, binding directly to expected DTO/Model lists for now.

    public class OpcClientSettings
    {
        public string ClientId { get; set; } = "DefaultOpcClientId";
        public List<ServerConnectionConfigDto>? ServerConnections { get; set; }
        public TagImportConfigDto? TagImport { get; set; }
        public List<UaSubscriptionConfigDto>? UaSubscriptions { get; set; } // Changed from "Subscriptions" in prompt for clarity
    }

    public class ServerAppSettings
    {
        public string? CommunicationMethod { get; set; } // e.g., "Grpc", "RabbitMq", "Kafka"
        public string? GrpcEndpoint { get; set; }
        public MessageQueueSettings? MessageQueue { get; set; }
    }

    public class MessageQueueSettings // Generic for RabbitMQ/Kafka
    {
        public string? Type { get; set; } // "RabbitMq" or "Kafka"
        public string? ConnectionString { get; set; } // RabbitMQ
        public string? BrokerList { get; set; } // Kafka
        public Dictionary<string, string>? Topics { get; set; } // e.g. "RealtimeData": "topic_realtime"
        // Add security settings if needed
    }

    public class EdgeAiSettings
    {
        public string? ModelPath { get; set; }
        public string? ActiveModelName { get; set; }
        public string? ActiveModelVersion { get; set; }
        public int PredictionIntervalMs { get; set; } = 5000;
    }

    public class DataHandlingSettings
    {
        public int BufferCapacity { get; set; } = 10000;
        public int TransmissionBatchSize { get; set; } = 100;
        public List<ValidationRule>? ValidationRules { get; set; }
        public List<WriteLimitPolicy>? WriteLimits { get; set; }
    }

    /// <summary>
    /// Offers a strongly-typed way to access application configuration values 
    /// defined in appsettings.json and environment variables.
    /// </summary>
    public class AppSettingsProvider
    {
        private readonly IConfiguration _configuration;

        public AppSettingsProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public OpcClientSettings GetOpcClientSettings() =>
            _configuration.GetSection("OpcClient").Get<OpcClientSettings>() ?? new OpcClientSettings();

        public ServerAppSettings GetServerAppSettings() =>
            _configuration.GetSection("ServerApp").Get<ServerAppSettings>() ?? new ServerAppSettings();

        public EdgeAiSettings GetEdgeAiSettings() =>
            _configuration.GetSection("EdgeAI").Get<EdgeAiSettings>() ?? new EdgeAiSettings();
        
        public DataHandlingSettings GetDataHandlingSettings() =>
            _configuration.GetSection("DataHandling").Get<DataHandlingSettings>() ?? new DataHandlingSettings();


        public List<ServerConnectionConfigDto> GetServerConnectionConfigs() =>
            GetOpcClientSettings().ServerConnections ?? new List<ServerConnectionConfigDto>();

        public TagImportConfigDto? GetTagImportConfig() =>
            GetOpcClientSettings().TagImport;
            
        public List<UaSubscriptionConfigDto> GetUaSubscriptionConfigs() =>
            GetOpcClientSettings().UaSubscriptions ?? new List<UaSubscriptionConfigDto>();

        public List<ValidationRule> GetValidationRules() =>
            GetDataHandlingSettings().ValidationRules ?? new List<ValidationRule>();

        public List<WriteLimitPolicy> GetWriteLimitPolicies() =>
            GetDataHandlingSettings().WriteLimits ?? new List<WriteLimitPolicy>();
    }
}