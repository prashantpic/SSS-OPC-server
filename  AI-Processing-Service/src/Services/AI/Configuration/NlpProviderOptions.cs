namespace AIService.Configuration
{
    /// <summary>
    /// Defines strongly-typed configuration properties for various NLP providers,
    /// such as API keys, endpoint URLs, and default language settings.
    /// REQ-7-014
    /// </summary>
    public class NlpProviderOptions
    {
        public const string SectionName = "NlpProviderOptions";

        /// <summary>
        /// Specifies which INlpProvider implementation to use (e.g., "SpaCy", "AzureCognitiveServices").
        /// </summary>
        public string ActiveProvider { get; set; } = "SpaCy"; // Default to SpaCy

        /// <summary>
        /// Configuration for SpaCy NLP Provider.
        /// </summary>
        public SpaCyOptions? SpaCy { get; set; }

        /// <summary>
        /// Configuration for Azure Cognitive Services NLP Provider.
        /// </summary>
        public AzureCognitiveServicesOptions? AzureCognitiveServices { get; set; }

        /// <summary>
        /// Default language for NLP processing (e.g., "en-US", "de-DE").
        /// </summary>
        public string DefaultLanguage { get; set; } = "en-US";
    }

    public class SpaCyOptions
    {
        /// <summary>
        /// Name of the spaCy language model to load (e.g., "en_core_web_sm").
        /// This model needs to be installed where the service runs.
        /// </summary>
        public string ModelName { get; set; } = "en_core_web_sm";

        // Potentially other spaCy specific settings, like Python environment path if using Python interop.
    }

    public class AzureCognitiveServicesOptions
    {
        /// <summary>
        /// API Key for Azure Cognitive Services (Language Service).
        /// Should be stored securely (e.g., Key Vault).
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Endpoint URL for Azure Cognitive Services (Language Service).
        /// Example: "https://<your-resource-name>.cognitiveservices.azure.com/"
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// The name of the Language resource.
        /// </summary>
        public string? LanguageResourceName { get; set; }

        /// <summary>
        /// The name of the deployment for Conversational Language Understanding (CLU).
        /// </summary>
        public string? CluDeploymentName { get; set; }

        /// <summary>
        /// The project name for Conversational Language Understanding (CLU).
        /// </summary>
        public string? CluProjectName { get; set; }

    }
}