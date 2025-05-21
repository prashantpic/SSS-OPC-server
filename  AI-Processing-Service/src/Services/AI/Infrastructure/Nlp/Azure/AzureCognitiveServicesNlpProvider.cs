using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Domain.Models.NlpModels; // Assuming NlqContext is here
using AIService.Configuration; // For NlpProviderOptions
using Azure; // For Response<T>
using Azure.AI.Language.Conversations; // For Conversational Language Understanding (CLU)
// Or using Azure.AI.TextAnalytics; // For older Text Analytics features / LUIS
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Infrastructure.Nlp.Azure
{
    public class AzureCognitiveServicesNlpProvider : INlpProvider
    {
        private readonly ILogger<AzureCognitiveServicesNlpProvider> _logger;
        private readonly NlpProviderOptions.AzureNlpOptions _azureOptions;
        private readonly ConversationAnalysisClient _convAnalysisClient;
        // private readonly TextAnalyticsClient _textAnalyticsClient; // If using Text Analytics specific features

        public string ProviderName => "AzureCognitiveServices";

        public AzureCognitiveServicesNlpProvider(ILogger<AzureCognitiveServicesNlpProvider> logger, IOptions<NlpProviderOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureOptions = options?.Value?.Azure ?? throw new ArgumentNullException(nameof(options), "Azure NLP options are not configured.");

            if (string.IsNullOrWhiteSpace(_azureOptions.LanguageEndpoint) ||
                string.IsNullOrWhiteSpace(_azureOptions.ApiKey) ||
                string.IsNullOrWhiteSpace(_azureOptions.CluProjectName) ||
                string.IsNullOrWhiteSpace(_azureOptions.CluDeploymentName))
            {
                _logger.LogError("AzureCognitiveServicesNlpProvider is configured as active, but one or more required settings (Endpoint, ApiKey, CluProjectName, CluDeploymentName) are missing.");
                throw new InvalidOperationException("Azure Cognitive Services NLP provider is missing required configuration.");
            }

            try
            {
                var credential = new AzureKeyCredential(_azureOptions.ApiKey);
                _convAnalysisClient = new ConversationAnalysisClient(new Uri(_azureOptions.LanguageEndpoint), credential);
                _logger.LogInformation("Azure ConversationAnalysisClient initialized for endpoint: {Endpoint}, Project: {Project}", _azureOptions.LanguageEndpoint, _azureOptions.CluProjectName);

                // If using TextAnalyticsClient for other purposes:
                // _textAnalyticsClient = new TextAnalyticsClient(new Uri(_azureOptions.LanguageEndpoint), credential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure ConversationAnalysisClient.");
                throw new InvalidOperationException("Failed to initialize Azure Cognitive Services client. See inner exception for details.", ex);
            }
        }

        public bool CanHandle(string providerName)
        {
            return ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<NlqContext> ProcessAsync(string query, NlqContext context = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            _logger.LogDebug("Processing NLQ using Azure Cognitive Services (CLU): '{Query}'", query);
            context ??= new NlqContext(query);
            context.ProviderUsed = ProviderName;

            try
            {
                var conversationalTask = new ConversationalTask(
                    query,
                    new TextConversationAnalysisOptions // Use ConversationAnalysisOptions or TextConversationAnalysisOptions based on SDK version
                    {
                        ProjectName = _azureOptions.CluProjectName,
                        DeploymentName = _azureOptions.CluDeploymentName,
                        // Language = context.Language ?? "en-US", // Or from options
                        Verbose = true, // To get more details if needed
                        IsLoggingEnabled = false
                    });

                Response<AnalyzeConversationResult> response = await _convAnalysisClient.AnalyzeConversationAsync(conversationalTask);
                
                if (response.Value.Result is ConversationPrediction conversationPrediction)
                {
                    context.IdentifiedIntent = conversationPrediction.TopIntent;
                    context.ConfidenceScore = conversationPrediction.Intents.FirstOrDefault(i => i.Category == conversationPrediction.TopIntent)?.Confidence ?? 0.0;

                    _logger.LogInformation("Azure CLU: Top Intent: {Intent}, Confidence: {Confidence}", context.IdentifiedIntent, context.ConfidenceScore);

                    context.ExtractedEntities = new List<NlpEntity>();
                    foreach (var entity in conversationPrediction.Entities)
                    {
                        context.ExtractedEntities.Add(new NlpEntity
                        {
                            Text = entity.Text,
                            Type = entity.Category,
                            SubType = entity.Subcategory, // If applicable
                            StartChar = entity.Offset,
                            EndChar = entity.Offset + entity.Length -1,
                            Confidence = entity.Confidence
                        });
                        _logger.LogTrace("Azure CLU: Found entity '{EntityText}' of type '{EntityType}', Confidence: {EntityConfidence}",
                            entity.Text, entity.Category, entity.Confidence);
                    }
                }
                else
                {
                    _logger.LogWarning("Azure CLU response did not contain a ConversationPrediction result for query: {Query}", query);
                    context.IdentifiedIntent = "Unknown"; // Or handle as error
                    context.ProcessingError = "Azure CLU did not return a valid conversation prediction.";
                }
                
                _logger.LogDebug("Azure CLU NLQ processing completed for: '{Query}'. Intent: {Intent}, Entities: {EntityCount}",
                                   query, context.IdentifiedIntent, context.ExtractedEntities?.Count ?? 0);
                return context;
            }
            catch (RequestFailedException rfEx)
            {
                _logger.LogError(rfEx, "Azure Cognitive Services API request failed for query: {Query}. Status: {Status}, ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                    query, rfEx.Status, rfEx.ErrorCode, rfEx.Message);
                context.ProcessingError = $"Azure API Error: {rfEx.ErrorCode} - {rfEx.Message}";
                throw; // Or handle as per NlpOrchestrationService's fallback strategy needs
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NLQ with Azure Cognitive Services for query: {Query}", query);
                context.ProcessingError = ex.Message;
                throw;
            }
        }

        public void Dispose()
        {
            // Clients are typically designed to be long-lived and thread-safe.
            // No explicit Dispose needed for Azure SDK clients usually.
            GC.SuppressFinalize(this);
        }
    }
}