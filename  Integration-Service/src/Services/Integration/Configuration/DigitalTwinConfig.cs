namespace IntegrationService.Configuration
{
    public class DigitalTwinConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = "HTTP"; // Primarily HTTP/REST based for now
        public string Endpoint { get; set; } = string.Empty; // Base URL for the Digital Twin Platform API
        public AuthenticationConfig Authentication { get; set; } = new AuthenticationConfig();
        public int SyncFrequencySeconds { get; set; } = 300; // 5 minutes default
        public string? DigitalTwinModelId { get; set; } // Identifier for the model definition (e.g., DTDL model ID)
        public string? MappingRuleId { get; set; }
        public string? MappingRulePath { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string ApiVersion { get; set; } = "2020-10-31"; // Example for Azure Digital Twins
    }
}