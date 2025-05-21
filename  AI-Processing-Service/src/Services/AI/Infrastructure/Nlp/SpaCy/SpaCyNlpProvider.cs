using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Domain.Models.NlpModels; // Assuming NlqContext is here
using AIService.Configuration; // For NlpProviderOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpacyDotNet; // Assuming this is the correct namespace for spaCy.NET
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Infrastructure.Nlp.SpaCy
{
    public class SpaCyNlpProvider : INlpProvider
    {
        private readonly ILogger<SpaCyNlpProvider> _logger;
        private readonly NlpProviderOptions _options;
        private readonly Spacy _spacy; // spaCy.NET main object
        private Language _nlp; // spaCy language model (e.g., en_core_web_sm)

        public string ProviderName => "SpaCy";

        public SpaCyNlpProvider(ILogger<SpaCyNlpProvider> logger, IOptions<NlpProviderOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (_options.SpaCy == null || string.IsNullOrWhiteSpace(_options.SpaCy.ModelName))
            {
                _logger.LogError("SpaCyNlpProvider is configured as active, but SpaCy model name is not specified in NlpProviderOptions.");
                throw new InvalidOperationException("SpaCy model name not configured.");
            }

            try
            {
                _spacy = Spacy.Instance;
                _logger.LogInformation("Loading spaCy model: {SpaCyModelName}", _options.SpaCy.ModelName);
                // Ensure the model is downloaded/available. spaCy.NET might have a way to check/download.
                // e.g., python -m spacy download en_core_web_sm
                _nlp = _spacy.Load(_options.SpaCy.ModelName);
                _logger.LogInformation("SpaCy model {SpaCyModelName} loaded successfully.", _options.SpaCy.ModelName);
            }
            catch (Exception ex)
            {
                // Specific exceptions from spaCy.NET for model not found would be helpful here.
                _logger.LogError(ex, "Failed to load spaCy model: {SpaCyModelName}. Ensure the model is downloaded and accessible.", _options.SpaCy.ModelName);
                throw new InvalidOperationException($"Failed to initialize spaCy model '{_options.SpaCy.ModelName}'. See inner exception for details.", ex);
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

            if (_nlp == null)
            {
                _logger.LogError("SpaCy language model is not loaded. Cannot process NLQ.");
                throw new InvalidOperationException("SpaCy model not loaded.");
            }

            _logger.LogDebug("Processing NLQ using SpaCy: '{Query}'", query);
            context ??= new NlqContext(query);
            context.ProviderUsed = ProviderName;

            try
            {
                var doc = await Task.Run(() => _nlp.Process(query)); // spaCy processing can be CPU-bound

                // Extract entities
                context.ExtractedEntities = new List<NlpEntity>();
                foreach (var ent in doc.Ents)
                {
                    context.ExtractedEntities.Add(new NlpEntity
                    {
                        Text = ent.Text,
                        Type = ent.Label,
                        StartChar = ent.StartChar,
                        EndChar = ent.EndChar,
                        Confidence = null // spaCy typically doesn't provide confidence for NER out-of-the-box per entity
                    });
                    _logger.LogTrace("SpaCy NER: Found entity '{EntityText}' of type '{EntityType}'", ent.Text, ent.Label);
                }

                // Intent recognition with spaCy typically requires a custom component (e.g., TextCategorizer)
                // or rule-based matching on tokens/dependencies.
                // For simplicity, this example does not implement complex intent recognition.
                // It might be based on keywords or sentence structure if a full classifier isn't trained.
                // context.IdentifiedIntent = RecognizeIntent(doc);
                // context.ConfidenceScore = ...

                // Placeholder for intent if not specifically implemented
                if (string.IsNullOrWhiteSpace(context.IdentifiedIntent) && context.ExtractedEntities.Any())
                {
                    context.IdentifiedIntent = "InformationExtraction"; // Generic intent
                }
                else if (string.IsNullOrWhiteSpace(context.IdentifiedIntent))
                {
                    context.IdentifiedIntent = "Unknown";
                }
                
                // Example: Tokenization and POS tagging (can be added to context if needed)
                // context.Tokens = doc.Tokens.Select(t => new NlpToken { Text = t.Text, Lemma = t.Lemma, Pos = t.Pos, Tag = t.Tag }).ToList();

                _logger.LogDebug("SpaCy NLQ processing completed for: '{Query}'. Intent: {Intent}, Entities: {EntityCount}",
                    query, context.IdentifiedIntent, context.ExtractedEntities.Count);

                return context;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NLQ with SpaCy for query: {Query}", query);
                context.ProcessingError = ex.Message; // Store error in context
                // Potentially rethrow or handle as per NlpOrchestrationService's fallback strategy needs
                throw;
            }
        }

        // Example helper for a very basic intent recognizer (would need significant improvement)
        private string RecognizeIntent(Doc doc)
        {
            // This is highly simplistic. Real intent recognition needs a trained model or more complex rules.
            var text = doc.Text.ToLowerInvariant();
            if (text.Contains("predict") && text.Contains("failure")) return "PredictFailure";
            if (text.Contains("what is") || text.Contains("get") || text.Contains("show me")) return "GetData";
            if (text.Contains("temperature of")) return "GetTemperature";
            // ... more rules or model-based classification
            return "Unknown";
        }

        public void Dispose()
        {
            _nlp?.Dispose();
            // _spacy might have a dispose or shutdown if it holds unmanaged resources globally.
            // Check spaCy.NET documentation. For now, assuming Language object (_nlp) is the primary disposable.
            GC.SuppressFinalize(this);
        }
    }
}