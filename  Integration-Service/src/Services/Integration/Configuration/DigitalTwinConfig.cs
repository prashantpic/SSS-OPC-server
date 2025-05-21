using IntegrationService.Configuration.Models;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Configuration for a single Digital Twin platform.
    /// </summary>
    public class DigitalTwinConfig
    {
        /// <summary>
        /// Unique identifier for this Digital Twin configuration.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of Digital Twin platform (e.g., AzureDigitalTwins).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The API endpoint for the Digital Twin platform.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Authentication details or references (e.g., to ICredentialManager).
        /// </summary>
        public AuthenticationSettings Authentication { get; set; } = new AuthenticationSettings();

        /// <summary>
        /// Identifier or path for data mapping rules specific to this twin.
        /// </summary>
        public string DataMappingRuleId { get; set; } = string.Empty;

        /// <summary>
        /// Frequency in seconds for periodic data synchronization.
        /// </summary>
        public int SyncFrequencySeconds { get; set; }

        /// <summary>
        /// Identifier of the target Digital Twin Definition Language (DTDL) or model version.
        /// </summary>
        public string TargetModelId { get; set; } = string.Empty;

         /// <summary>
        /// Indicates if bi-directional communication (commands, setpoints) is enabled.
        /// </summary>
        public bool EnableBiDirectional { get; set; }
    }
}