namespace AIService.Configuration
{
    public class NlpProviderOptions
    {
        public const string SectionName = "NlpProviderOptions";

        public string ActiveProvider { get; set; } = "SpaCy"; // e.g., "SpaCy", "AzureCognitiveServices"
        public string SpaCyModelName { get; set; } = "en_core_web_sm";
        public string? AzureCognitiveServicesApiKey { get; set; }
        public string? AzureCognitiveServicesEndpoint { get; set; }
        public string? AzureLanguageResourceName { get; set; } // For Azure Language Service (Conversational Language Understanding)
        public string? AzureDeploymentName { get; set; } // For Azure Language Service (Conversational Language Understanding)
    }
}