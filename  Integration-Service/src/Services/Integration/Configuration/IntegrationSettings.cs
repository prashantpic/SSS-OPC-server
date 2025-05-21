namespace IntegrationService.Configuration
{
    // Placeholder for IoTPlatformSettings - to be fully defined in IoTPlatformSettings.cs
    public class IoTPlatformSettings
    {
        public List<IoTPlatformConfigInteg> Platforms { get; set; } = new List<IoTPlatformConfigInteg>();
    }

    // Placeholder for IoTPlatformConfig - to be fully defined in IoTPlatformConfig.cs
    public class IoTPlatformConfigInteg // Renamed to avoid conflict if IoTPlatformConfig is defined elsewhere
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "MQTT", "AMQP", "HTTP"
        public string Endpoint { get; set; } = string.Empty;
        public AuthenticationConfig? Authentication { get; set; }
        public string DataFormat { get; set; } = "JSON";
        public string MappingRuleId { get; set; } = string.Empty;
    }

    public class AuthenticationConfig
    {
        public string Type { get; set; } = string.Empty; // "SasKey", "Certificate", "ApiKey", "OAuth"
        public string CredentialIdentifier { get; set; } = string.Empty;
    }

    // Placeholder for BlockchainSettings - to be fully defined in BlockchainSettings.cs
    public class BlockchainSettings
    {
        public string RpcUrl { get; set; } = string.Empty;
        public long? ChainId { get; set; }
        public string SmartContractAddress { get; set; } = string.Empty;
        public string SmartContractAbiPath { get; set; } = string.Empty;
        public string CredentialIdentifier { get; set; } = string.Empty;
        public string GasPriceStrategy { get; set; } = "Medium"; // "Low", "Medium", "High", or specific value
        public List<CriticalDataCriteriaConfig> CriticalDataCriteria { get; set; } = new List<CriticalDataCriteriaConfig>();
    }

    public class CriticalDataCriteriaConfig
    {
        public string SourceProperty { get; set; } = string.Empty;
        public string Operator { get; set; } = "Equals"; // "Equals", "GreaterThan", "Contains"
        public string Value { get; set; } = string.Empty;
    }

    // Placeholder for DigitalTwinSettings - to be fully defined in DigitalTwinSettings.cs
    public class DigitalTwinSettings
    {
        public List<DigitalTwinConfigInteg> Twins { get; set; } = new List<DigitalTwinConfigInteg>();
    }

    // Placeholder for DigitalTwinConfig - to be fully defined in DigitalTwinConfig.cs
    public class DigitalTwinConfigInteg // Renamed to avoid conflict
    {
        public string Id { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string CredentialIdentifier { get; set; } = string.Empty;
        public int SyncFrequencySeconds { get; set; } = 300;
        public string DigitalTwinModelId { get; set; } = string.Empty;
        public string MappingRuleId { get; set; } = string.Empty;
    }

    public class ResiliencySettings
    {
        public RetryPolicyConfig? DefaultRetryPolicy { get; set; }
        public CircuitBreakerPolicyConfig? DefaultCircuitBreakerPolicy { get; set; }
    }

    public class RetryPolicyConfig
    {
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelayMilliseconds { get; set; } = 1000;
    }

    public class CircuitBreakerPolicyConfig
    {
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 5;
        public int DurationOfBreakMilliseconds { get; set; } = 30000;
    }

    public class SecuritySettings
    {
        public string CredentialManagerType { get; set; } = "Environment"; // "Environment", "AzureKeyVault"
        public string? VaultUri { get; set; }
    }

    public class IntegrationServiceMiscSettings
    {
        public string OpcDataInputQueueName { get; set; } = "opc-data-input";
        public string BlockchainProcessingQueueName { get; set; } = "blockchain-processing";
        public string DataMappingPath { get; set; } = "Mappings";
        public Dictionary<string, bool> FeatureFlags { get; set; } = new Dictionary<string, bool>();
    }

    public class IntegrationSettings
    {
        public static readonly string SectionName = "IntegrationSettings";

        public IoTPlatformSettings IoTPlatforms { get; set; } = new();
        public BlockchainSettings Blockchain { get; set; } = new();
        public DigitalTwinSettings DigitalTwins { get; set; } = new();
        public ResiliencySettings Resiliency { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
        public IntegrationServiceMiscSettings Miscellaneous { get; set; } = new();
    }
}