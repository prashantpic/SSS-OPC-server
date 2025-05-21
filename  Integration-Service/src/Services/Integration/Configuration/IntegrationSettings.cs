using IntegrationService.Configuration;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Root configuration class for all integration settings, mapped from appsettings.json.
    /// Acts as a container for more specific configuration objects like IoTPlatformSettings, BlockchainSettings, and DigitalTwinSettings.
    /// </summary>
    public class IntegrationSettings
    {
        public const string SectionName = "IntegrationSettings";

        public IoTPlatformSettings IoTPlatformSettings { get; set; } = new IoTPlatformSettings();
        public BlockchainSettings BlockchainSettings { get; set; } = new BlockchainSettings();
        public DigitalTwinSettings DigitalTwinSettings { get; set; } = new DigitalTwinSettings();

        /// <summary>
        /// The name of the message queue used for asynchronous blockchain logging requests.
        /// </summary>
        public string AsyncQueueNameForBlockchainProcessing { get; set; } = string.Empty;

        /// <summary>
        /// The name of the message queue from which OPC data is consumed.
        /// </summary>
        public string OpcDataInputQueueName { get; set; } = string.Empty;

        /// <summary>
        /// The base path where data mapping rule files are stored.
        /// </summary>
        public string MappingRulePathBase { get; set; } = "Mappings";

        // FeatureFlags and ResiliencyConfigs are often separate top-level sections or nested.
        // For this model, they are directly referenced from the root configuration in Program.cs
        // but could be nested here if preferred.
        // Example:
        // public FeatureFlags FeatureFlags { get; set; } = new FeatureFlags();
        // public ResiliencyConfigs ResiliencyConfigs { get; set; } = new ResiliencyConfigs();
    }
}