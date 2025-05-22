using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using IndustrialAutomation.OpcClient.Domain.Models; // For ValidationRule, WriteLimitPolicy
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Configuration
{
    public record ClientConfigurationDto
    {
        public string ClientId { get; set; } = string.Empty; // Modifiable by setter for initial load
        public List<ServerConnectionConfigDto> ServerConnections { get; set; } = new List<ServerConnectionConfigDto>();
        public List<TagDefinitionDto> TagDefinitions { get; set; } = new List<TagDefinitionDto>();
        public List<UaSubscriptionConfigDto> UaSubscriptions { get; set; } = new List<UaSubscriptionConfigDto>();
        public List<ValidationRule> ValidationRules { get; set; } = new List<ValidationRule>();
        public List<WriteLimitPolicy> WriteLimitPolicies { get; set; } = new List<WriteLimitPolicy>();
        public EdgeModelMetadataDto? ActiveEdgeModel { get; set; }

        // For appsettings.json structure primarily, less for server push
        public List<TagImportConfigDto>? TagImportConfigs { get; set; } // Loaded locally
        public EdgeAiSettingsDto? EdgeAISettings { get; set; } // Loaded locally
        public DataHandlingSettingsDto? DataHandlingSettings { get; set; } // Loaded locally
        public ServerAppSettingsDto? ServerAppSettings { get; set; } // Loaded locally

    }

    // Supporting DTOs for local configuration structure
    public record EdgeAiSettingsDto
    {
        public string? ModelPath { get; init; }
        public string? ActiveModel { get; init; }
        public string? ActiveModelVersion { get; init; }
        public int PredictionIntervalMs { get; init; } = 5000;
    }

    public record DataHandlingSettingsDto
    {
        public int BufferCapacity { get; init; } = 10000;
        public int TransmissionBatchSize { get; init; } = 100;
    }

    public record ServerAppSettingsDto
    {
        public string? CommunicationMethod { get; init; }
        public string? GrpcEndpoint { get; init; }
        public MessageQueueSettingsDto? MessageQueue { get; init; }
    }

    public record MessageQueueSettingsDto
    {
        public string? BrokerType { get; init; } // "RabbitMQ", "Kafka"
        public string? ConnectionString { get; init; } // For RabbitMQ
        public string? BrokerList { get; init; } // For Kafka
        public Dictionary<string, string>? Topics { get; init; }
        // Add security settings if needed (TLS paths, SASL etc.)
    }
}