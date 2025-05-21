namespace AIService.Configuration
{
    public class NlpProviderOptions
    {
        public const string SectionName = "NlpProviderOptions";

        public string? ActiveProvider { get; set; } // e.g., "SpaCy", "Azure"
        public string? SpaCyModelName { get; set; } // e.g., "en_core_web_sm"

        // Azure Cognitive Services for Language (Conversational Language Understanding / LUIS)
        public string? AzureCognitiveServicesApiKey { get; set; }
        public string? AzureCognitiveServicesEndpoint { get; set; }
        public string? AzureLanguageResourceName { get; set; } // Specific to some Azure SDKs
        public string? AzureDeploymentName { get; set; } // CLU project name / LUIS App ID
    }
}