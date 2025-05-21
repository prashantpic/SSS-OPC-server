```csharp
using AIService.Domain.Enums;
using AIService.Domain.Interfaces;
using AIService.Domain.Models; // For NlqContext, NlpProviderType
using AIService.Infrastructure.Configuration; // For NlpProviderOptions
using Azure; // For Response<T>
using Azure.AI.Language.Conversations; // For Conversational Language Understanding (CLU)
// Or use Azure.AI.TextAnalytics for general text analytics (key phrases, entities, sentiment)
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
        private readonly NlpProviderOptions _options;
        private readonly ConversationAnalysisClient _client;

        public AzureCognitiveServicesNlpProvider(
            ILogger<AzureCognitiveServicesNlpProvider> logger,
            IOptions<NlpProviderOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.AzureCognitiveServicesEndpoint) ||
                string.IsNullOrWhiteSpace(_options.AzureCognitiveServicesApiKey) ||
                string.IsNullOrWhiteSpace(_options.AzureLanguageProjectName) || // CLU Project Name
                string.IsNullOrWhiteSpace(_options.AzureLanguageDeploymentName)) // CLU Deployment Name
            {
                _logger.LogError("Azure Cognitive Services NLP provider is not fully configured. " +
                                 "Endpoint, API Key, Project Name, and Deployment Name are required.");
                throw new InvalidOperationException("Azure Cognitive Services NLP provider not fully configured.");
            }

            try
            {
                _client = new ConversationAnalysisClient(
                    new Uri(_options.AzureCognitiveServicesEndpoint),
                    new AzureKeyCredential(_options.AzureCognitiveServicesApiKey)
                );
                _logger.LogInformation("Azure ConversationAnalysisClient initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure ConversationAnalysisClient.");
                throw;
            }
        }

        public NlpProviderType ProviderType => NlpProviderType.AzureCognitiveServices;
        public bool CanHandle(NlpProviderType type) => type == ProviderType;

        public async Task<NlqContext> ProcessAsync(string query, NlqContext context)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Input query is null or empty.");
                context.ErrorMessage = "Input query is null or empty.";
                return context;
            }

            _logger.LogDebug("Processing query with Azure Cognitive Services (CLU): \"{Query}\"", query);
            context.OriginalQuery = query;
            context.ProviderUsed = ProviderType.ToString();

            try
            {
                var data = new
                {
                    analysisInput = new
                    {
                        conversationItem = new
                        {
                            text = query,
                            id = context.ConversationId ?? "1", // A unique ID for the conversation turn
                            participantId = context.UserId ?? "user1" // A unique ID for the user
                        }
                    },
                    parameters = new
                    {
                        projectName = _options.AzureLanguageProjectName,
                        deploymentName = _options.AzureLanguageDeploymentName,
                        // We can add other parameters like stringIndexType, verbose, etc. if needed
                        stringIndexType = "Utf16CodeUnit", 
                    },
                    kind = "Conversation"
                };
                
                Response response = await _client.AnalyzeConversationAsync(RequestContent.Create(data));
                
                // Parse the response. The structure of this response depends on your CLU model.
                // Using System.Text.Json to parse the dynamic response.
                using var jsonDocument = JsonDocument.Parse(response.ContentStream);
                var result = jsonDocument.RootElement.GetProperty("result");
                var prediction = result.GetProperty("prediction");

                // Top Intent
                if (prediction.TryGetProperty("topIntent", out JsonElement topIntentElement))
                {
                    context.IdentifiedIntent = topIntentElement.GetString();
                    if (prediction.TryGetProperty("intents", out JsonElement intentsElement))
                    {
                        var topIntentDetails = intentsElement.EnumerateArray()
                            .FirstOrDefault(i => i.GetProperty("category").GetString() == context.IdentifiedIntent);
                        if(topIntentDetails.TryGetProperty("confidenceScore", out JsonElement confidenceElement))
                        {
                            context.ConfidenceScore = confidenceElement.GetDouble();
                        }
                    }
                     _logger.LogDebug("Identified intent: {Intent} with confidence {Confidence}", context.IdentifiedIntent, context.ConfidenceScore);
                }
                else
                {
                    _logger.LogWarning("Top intent not found in Azure CLU response for query: \"{Query}\"", query);
                }

                // Entities
                if (prediction.TryGetProperty("entities", out JsonElement entitiesElement))
                {
                    var extractedEntities = new List<Domain.Models.NlqEntity>();
                    foreach (var entity in entitiesElement.EnumerateArray())
                    {
                        extractedEntities.Add(new Domain.Models.NlqEntity
                        {
                            Type = entity.GetProperty("category").GetString(),
                            Value = entity.GetProperty("text").GetString(),
                            StartIndex = entity.TryGetProperty("offset", out JsonElement offsetEl) ? offsetEl.GetInt32() : 0,
                            EndIndex = entity.TryGetProperty("offset", out JsonElement lenOffsetEl) && entity.TryGetProperty("length", out JsonElement lengthEl) 
                                       ? lenOffsetEl.GetInt32() + lengthEl.GetInt32() 
                                       : 0,
                            ConfidenceScore = entity.TryGetProperty("confidenceScore", out JsonElement confEl) ? confEl.GetDouble() : (double?)null
                        });
                    }
                    context.ExtractedEntities = extractedEntities;
                    _logger.LogDebug("Extracted {EntityCount} entities.", extractedEntities.Count);
                }
                
                context.ProcessedQuery = query; // Or modified query if CLU provides one
                context.ErrorMessage = null; // Clear previous errors

            }
            catch (RequestFailedException ex)
            {
                 _logger.LogError(ex, "Azure Cognitive Services request failed for query \"{Query}\". Status: {Status}, Content: {Content}", query, ex.Status, ex.Message);
                 context.ErrorMessage = $"Azure NLP request failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query \"{Query}\" with Azure Cognitive Services.", query);
                context.ErrorMessage = $"Azure NLP processing failed: {ex.Message}";
            }

            return context;
        }
    }
}