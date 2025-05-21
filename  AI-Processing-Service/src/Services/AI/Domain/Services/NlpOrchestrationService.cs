namespace AIService.Domain.Services
{
    using AIService.Domain.Interfaces;
    using AIService.Domain.Models;
    using AIService.Configuration; // Assuming NlpProviderOptions is here
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Orchestrates NLQ processing. It selects the appropriate INlpProvider,
    /// processes the query, applies user-defined aliases, and handles fallbacks.
    /// (REQ-7-014, REQ-7-015, REQ-7-016)
    /// </summary>
    public class NlpOrchestrationService
    {
        private readonly IEnumerable<INlpProvider> _nlpProviders;
        private readonly NlpProviderOptions _nlpOptions;
        private readonly ILogger<NlpOrchestrationService> _logger;
        // private readonly IModelRepository _modelRepository; // Potentially for fetching aliases/mappings dynamically

        public NlpOrchestrationService(
            IEnumerable<INlpProvider> nlpProviders,
            IOptions<NlpProviderOptions> nlpOptions,
            ILogger<NlpOrchestrationService> logger)
            // IModelRepository modelRepository) // If aliases are stored via ModelRepository
        {
            _nlpProviders = nlpProviders ?? throw new ArgumentNullException(nameof(nlpProviders));
            _nlpOptions = nlpOptions?.Value ?? throw new ArgumentNullException(nameof(nlpOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _modelRepository = modelRepository;
        }

        /// <summary>
        /// Processes a natural language query using the configured NLP provider and strategies.
        /// </summary>
        /// <param name="query">The raw natural language query string.</param>
        /// <param name="initialContext">Optional pre-existing NlqContext (e.g., with user-specific aliases).</param>
        /// <returns>An NlqContext populated with the processing results.</returns>
        public async Task<NlqContext> ProcessAsync(string query, NlqContext initialContext = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("NLP processing attempted with an empty query.");
                return new NlqContext(query) { IdentifiedIntent = "Error", AdditionalData = new Dictionary<string, object>{{"ErrorMessage", "Query cannot be empty."}} };
            }

            NlqContext context = initialContext ?? new NlqContext(query);
            context.OriginalQuery = query; // Ensure original query is set

            // 1. Apply pre-defined aliases (REQ-7-015) - could be from config or context
            // This is a simplified placeholder. Real alias application might be more complex.
            // context.ProcessedQuery = ApplyConfiguredAliases(context.OriginalQuery, _nlpOptions.UserDefinedAliases);
            // if (initialContext?.AppliedAliases != null) { /* merge or use initial context's aliases */ }


            INlpProvider selectedProvider = _nlpProviders.FirstOrDefault(p => 
                string.Equals(p.ProviderName, _nlpOptions.ActiveProvider, StringComparison.OrdinalIgnoreCase));

            if (selectedProvider == null)
            {
                _logger.LogError("Active NLP provider '{ActiveProvider}' configured in NlpProviderOptions not found or not registered.", _nlpOptions.ActiveProvider);
                context.IdentifiedIntent = "Error_Configuration";
                context.AdditionalData["ErrorMessage"] = $"NLP Provider '{_nlpOptions.ActiveProvider}' not available.";
                return context;
            }
            
            _logger.LogInformation("Using NLP provider: {ProviderName} for query: \"{Query}\"", selectedProvider.ProviderName, query);
            context.ProviderUsed = selectedProvider.ProviderName;

            try
            {
                context = await selectedProvider.ProcessAsync(context.ProcessedQuery, context);
                _logger.LogInformation("NLP processing completed. Intent: {Intent}, Entities: {EntityCount}", context.IdentifiedIntent, context.ExtractedEntities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during NLP processing with provider {ProviderName} for query: \"{Query}\"", selectedProvider.ProviderName, query);
                
                // Fallback strategy (REQ-7-016)
                if (_nlpOptions.EnableFallback && !string.IsNullOrWhiteSpace(_nlpOptions.FallbackProviderName) && 
                    !string.Equals(selectedProvider.ProviderName, _nlpOptions.FallbackProviderName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Attempting fallback NLP provider: {FallbackProviderName}", _nlpOptions.FallbackProviderName);
                    INlpProvider fallbackProvider = _nlpProviders.FirstOrDefault(p => 
                        string.Equals(p.ProviderName, _nlpOptions.FallbackProviderName, StringComparison.OrdinalIgnoreCase));
                    
                    if (fallbackProvider != null)
                    {
                        context.FallbackApplied = true;
                        context.ProviderUsed = fallbackProvider.ProviderName; // Update provider used
                        try
                        {
                            context = await fallbackProvider.ProcessAsync(context.ProcessedQuery, context);
                             _logger.LogInformation("Fallback NLP processing completed. Intent: {Intent}, Entities: {EntityCount}", context.IdentifiedIntent, context.ExtractedEntities.Count);
                        }
                        catch (Exception fallbackEx)
                        {
                            _logger.LogError(fallbackEx, "Error during fallback NLP processing with provider {FallbackProviderName}", fallbackProvider.ProviderName);
                            context.IdentifiedIntent = "Error_Processing_Fallback";
                            context.AdditionalData["ErrorMessage"] = $"Fallback NLP failed: {fallbackEx.Message}";
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Fallback NLP provider '{FallbackProviderName}' not found.", _nlpOptions.FallbackProviderName);
                        context.IdentifiedIntent = "Error_Processing_Primary";
                        context.AdditionalData["ErrorMessage"] = $"Primary NLP failed: {ex.Message}. Fallback provider not found.";
                    }
                }
                else
                {
                    context.IdentifiedIntent = "Error_Processing";
                    context.AdditionalData["ErrorMessage"] = $"NLP processing failed: {ex.Message}";
                }
            }
            
            // Post-processing, e.g., further alias application based on results, confidence scoring adjustments
            // ApplyDynamicAliases(context); // Placeholder

            return context;
        }

        // Placeholder for alias application logic (REQ-7-015)
        // This could be more sophisticated, perhaps involving regex or structured alias definitions.
        // private string ApplyConfiguredAliases(string query, Dictionary<string, string> aliases)
        // {
        //     if (aliases == null || !aliases.Any()) return query;
        //     string processedQuery = query;
        //     foreach (var alias in aliases)
        //     {
        //         processedQuery = processedQuery.Replace(alias.Key, alias.Value, StringComparison.OrdinalIgnoreCase);
        //     }
        //     return processedQuery;
        // }

        // Placeholder for more complex post-processing or dynamic alias application
        // private void ApplyDynamicAliases(NlqContext context)
        // {
        //     // Example: if an entity "device_xyz" is found, map it to a canonical ID
        //     // This might involve looking up aliases from a database/configuration
        // }
    }
}