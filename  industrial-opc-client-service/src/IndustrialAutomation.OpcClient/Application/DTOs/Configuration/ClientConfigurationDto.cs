using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using IndustrialAutomation.OpcClient.Domain.Models; // For ValidationRule, WriteLimitPolicy
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Configuration
{
    public record ClientConfigurationDto
    {
        public string ClientId { get; init; } = Guid.NewGuid().ToString();
        public List<ServerConnectionConfigDto> ServerConnections { get; init; } = new();
        public List<TagDefinitionDto> TagDefinitions { get; init; } = new();
        public List<UaSubscriptionConfigDto> UaSubscriptions { get; init; } = new();
        public List<ValidationRule> ValidationRules { get; init; } = new();
        public List<WriteLimitPolicy> WriteLimitPolicies { get; init; } = new();
        public EdgeModelMetadataDto? ActiveEdgeModel { get; init; }
        public Dictionary<string, string> GeneralSettings { get; init; } = new(); // For other generic settings

        // Default constructor for deserialization and initialization
        public ClientConfigurationDto() { }

        // Constructor for creating with initial values if needed
        public ClientConfigurationDto(
            string clientId,
            List<ServerConnectionConfigDto>? serverConnections = null,
            List<TagDefinitionDto>? tagDefinitions = null,
            List<UaSubscriptionConfigDto>? uaSubscriptions = null,
            List<ValidationRule>? validationRules = null,
            List<WriteLimitPolicy>? writeLimitPolicies = null,
            EdgeModelMetadataDto? activeEdgeModel = null,
            Dictionary<string, string>? generalSettings = null)
        {
            ClientId = clientId;
            ServerConnections = serverConnections ?? new List<ServerConnectionConfigDto>();
            TagDefinitions = tagDefinitions ?? new List<TagDefinitionDto>();
            UaSubscriptions = uaSubscriptions ?? new List<UaSubscriptionConfigDto>();
            ValidationRules = validationRules ?? new List<ValidationRule>();
            WriteLimitPolicies = writeLimitPolicies ?? new List<WriteLimitPolicy>();
            ActiveEdgeModel = activeEdgeModel;
            GeneralSettings = generalSettings ?? new Dictionary<string, string>();
        }
    }
}