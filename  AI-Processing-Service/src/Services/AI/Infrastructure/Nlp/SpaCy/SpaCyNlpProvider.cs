```csharp
using AIService.Domain.Enums;
using AIService.Domain.Interfaces;
using AIService.Domain.Models; // For NlqContext, NlpProviderType
using AIService.Infrastructure.Configuration; // For NlpProviderOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spacy.Net; // Assuming this is the main namespace for spaCy.NET
using Spacy.Net.Containers; // For Doc
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Infrastructure.Nlp.SpaCy
{
    public class SpaCyNlpProvider : INlpProvider
    {
        private readonly ILogger<SpaCyNlpProvider> _logger;
        private readonly NlpProviderOptions _options;
        private static Spacy.Net.Spacy _spacyInstance; // Static to load models only once
        private static Language _nlp; // spaCy language model instance
        private static bool _isInitialized = false;
        private static readonly object _initLock = new object();

        public SpaCyNlpProvider(ILogger<SpaCyNlpProvider> logger, IOptions<NlpProviderOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            InitializeSpaCy();
        }

        private void InitializeSpaCy()
        {
            if (_isInitialized) return;

            lock (_initLock)
            {
                if (_isInitialized) return;

                try
                {
                    _logger.LogInformation("Initializing spaCy.NET...");
                    _spacyInstance = Spacy.Net.Spacy.Instance;
                    
                    string modelName = _options.SpaCyModelName;
                    if (string.IsNullOrWhiteSpace(modelName))
                    {
                        _logger.LogError("SpaCy model name is not configured in NlpProviderOptions.SpaCyModelName.");
                        throw new InvalidOperationException("SpaCy model name not configured.");
                    }

                    _logger.LogInformation("Loading spaCy model: {SpaCyModelName}", modelName);
                    // Ensure the model is downloaded. spaCy.NET might have utilities for this,
                    // or it's a manual prerequisite. python -m spacy download en_core_web_sm
                    _nlp = _spacyInstance.Load(modelName);
                    _logger.LogInformation("SpaCy model {SpaCyModelName} loaded successfully.", modelName);
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize spaCy.NET or load model.");
                    // Not throwing here to allow the application to start, but ProcessAsync will fail.
                    // Or, rethrow if spaCy is critical: throw new InvalidOperationException("SpaCy initialization failed.", ex);
                }
            }
        }
        
        public NlpProviderType ProviderType => NlpProviderType.SpaCy;

        public bool CanHandle(NlpProviderType type) => type == ProviderType;

        public Task<NlqContext> ProcessAsync(string query, NlqContext context)
        {
            if (!_isInitialized || _nlp == null)
            {
                _logger.LogError("SpaCy NLP provider is not initialized or model failed to load. Cannot process query.");
                throw new InvalidOperationException("SpaCy NLP provider not initialized.");
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Input query is null or empty.");
                // Update context or return it as is, perhaps with an error.
                context.ErrorMessage = "Input query is null or empty.";
                return Task.FromResult(context);
            }

            _logger.LogDebug("Processing query with spaCy: \"{Query}\"", query);
            context.OriginalQuery = query;
            context.ProviderUsed = ProviderType.ToString();

            try
            {
                Doc doc = _nlp.GetDocument(query);

                // Named Entity Recognition (NER)
                var extractedEntities = new List<Domain.Models.NlqEntity>(); // Assuming NlqEntity is defined in Domain.Models
                foreach (var ent in doc.Ents)
                {
                    extractedEntities.Add(new Domain.Models.NlqEntity
                    {
                        Type = ent.Label,
                        Value = ent.Text,
                        StartIndex = ent.StartChar,
                        EndIndex = ent.EndChar
                        // ConfidenceScore might not be directly available for spaCy entities unless a custom component provides it.
                    });
                }
                context.ExtractedEntities = extractedEntities;
                _logger.LogDebug("Extracted {EntityCount} entities.", extractedEntities.Count);

                // Intent Recognition (if a custom pipeline component for intent classification is part of the spaCy model)
                // SpaCy core models usually don't do intent classification out-of-the-box.
                // This would require a custom text classifier in the pipeline.
                // Example: if (doc.HasExtension("intent")) { context.IdentifiedIntent = doc.GetExtension<string>("intent"); }
                // For now, we'll leave intent blank or handle it via rules/keywords if no classifier.
                if (doc.Cats != null && doc.Cats.Any()) // TextCategorizer results
                {
                    var bestCat = doc.Cats.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                    if (bestCat.Value > (_options.SpaCyIntentThreshold ?? 0.5)) // Use a configurable threshold
                    {
                        context.IdentifiedIntent = bestCat.Key;
                        context.ConfidenceScore = bestCat.Value; // Assuming this is confidence for intent
                         _logger.LogDebug("Identified intent: {Intent} with confidence {Confidence}", context.IdentifiedIntent, context.ConfidenceScore);
                    }
                    else
                    {
                        _logger.LogDebug("No intent identified above threshold from doc.Cats.");
                    }
                }
                else
                {
                     _logger.LogDebug("No intent classification (doc.Cats) available in the spaCy model output for query: \"{Query}\"", query);
                }
                
                context.ProcessedQuery = query; // Could be normalized text if spaCy pipeline does that
                context.ErrorMessage = null; // Clear previous errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query \"{Query}\" with spaCy.", query);
                context.ErrorMessage = $"SpaCy processing failed: {ex.Message}";
            }

            return Task.FromResult(context);
        }
    }
}