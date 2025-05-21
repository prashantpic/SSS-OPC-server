using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Placeholder for NlpProviderOptions, which would be in the Configuration project/namespace
namespace AIService.Configuration
{
    public class NlpProviderOptions
    {
        public string ActiveProvider { get; set; } = "Default"; // e.g., "SpaCy", "AzureCognitiveServices"
        public Dictionary<string, string> GlobalAliases { get; set; } = new Dictionary<string, string>();
        public string? FallbackProvider { get; set; } // Optional: Name of a provider to use as fallback
        public double MinConfidenceForPrimary { get; set; } = 0.7; // Min confidence to accept from primary before trying fallback
    }
}

namespace AIService.Domain.Services
{
    /// <summary>
    /// Orchestrates NLQ processing. It selects the appropriate INlpProvider based on configuration,
    /// processes the query, applies user-defined aliases (REQ-7-015), and handles fallbacks (REQ-7-016).
    /// </summary>
    public class NlpOrchestrationService
    {
        private readonly IEnumerable<INlpProvider> _nlpProviders;
        private readonly NlpProviderOptions _options;
        private readonly ILogger<NlpOrchestrationService> _logger;

        public NlpOrchestrationService(
            IEnumerable<INlpProvider> nlpProviders,
            IOptions<NlpProviderOptions> options,
            ILogger<NlpOrchestrationService> logger)
        {
            _nlpProviders = nlpProviders ?? throw new ArgumentNullException(nameof(nlpProviders));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NlqContext> ProcessAsync(string query, NlqContext initialContext, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                initialContext.ErrorMessage = "Query cannot be empty.";
                return initialContext;
            }

            _logger.LogInformation("Processing NLQ: '{Query}' with active provider hint: {ActiveProvider}", query, _options.ActiveProvider);

            INlpProvider? primaryProvider = _nlpProviders.FirstOrDefault(p =>
                p.ProviderName.Equals(_options.ActiveProvider, StringComparison.OrdinalIgnoreCase));

            if (primaryProvider == null)
            {
                _logger.LogError("Active NLP provider '{ActiveProvider}' not found.", _options.ActiveProvider);
                initialContext.ErrorMessage = $"Active NLP provider '{_options.ActiveProvider}' not found.";
                return initialContext;
            }
            
            initialContext.ProviderUsed = primaryProvider.ProviderName;

            // Apply global aliases before sending to provider
            string processedQueryForProvider = ApplyAliases(query, _options.GlobalAliases);
            initialContext.ProcessedQuery = processedQueryForProvider; // Store pre-provider alias application

            try
            {
                NlqContext resultContext = await primaryProvider.ProcessAsync(processedQueryForProvider, initialContext, cancellationToken);

                // Apply user-specific aliases from initialContext if any (could be done before or after provider)
                // For now, assume provider might use context.AppliedAliases or we can re-apply here if needed.

                // Fallback logic REQ-7-016
                if (!string.IsNullOrWhiteSpace(_options.FallbackProvider) &&
                    (resultContext.ConfidenceScore == null || resultContext.ConfidenceScore < _options.MinConfidenceForPrimary || !string.IsNullOrWhiteSpace(resultContext.ErrorMessage)))
                {
                    _logger.LogInformation("Primary provider result for '{Query}' has low confidence or error. Attempting fallback with {FallbackProvider}.", query, _options.FallbackProvider);
                    INlpProvider? fallbackProvider = _nlpProviders.FirstOrDefault(p =>
                        p.ProviderName.Equals(_options.FallbackProvider, StringComparison.OrdinalIgnoreCase));

                    if (fallbackProvider != null)
                    {
                        // Create a new context or reset parts of the existing one for the fallback provider
                        var fallbackInitialContext = new NlqContext(query) // Start fresh with original query for fallback
                        {
                             AppliedAliases = initialContext.AppliedAliases // Carry over user-specific aliases
                        };
                        fallbackInitialContext.ProcessedQuery = ApplyAliases(query, _options.GlobalAliases); // Apply global aliases for fallback too

                        NlqContext fallbackResultContext = await fallbackProvider.ProcessAsync(fallbackInitialContext.ProcessedQuery, fallbackInitialContext, cancellationToken);
                        
                        // Decide how to merge or replace results. For now, replace if fallback is better.
                        // This logic can be complex (e.g. combine entities if intents match etc.)
                        if (fallbackResultContext.ConfidenceScore > resultContext.ConfidenceScore || string.IsNullOrWhiteSpace(resultContext.ErrorMessage) && !string.IsNullOrWhiteSpace(fallbackResultContext.ErrorMessage) == false)
                        {
                            _logger.LogInformation("Fallback provider '{FallbackProviderName}' yielded a better or successful result.", fallbackProvider.ProviderName);
                            fallbackResultContext.FallbackApplied = true;
                            fallbackResultContext.ProviderUsed = fallbackProvider.ProviderName; // Ensure provider is correctly set
                            return fallbackResultContext;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Fallback NLP provider '{FallbackProvider}' not found.", _options.FallbackProvider);
                    }
                }
                
                resultContext.ProviderUsed = primaryProvider.ProviderName; // Ensure provider is correctly set on the returned context
                return resultContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NLQ '{Query}' with provider {ProviderName}.", query, primaryProvider.ProviderName);
                initialContext.ErrorMessage = $"Error during NLP processing: {ex.Message}";
                initialContext.ProviderUsed = primaryProvider.ProviderName;
                return initialContext;
            }
        }

        private string ApplyAliases(string query, IDictionary<string, string> aliases)
        {
            if (aliases == null || !aliases.Any())
            {
                return query;
            }

            string processedQuery = query;
            // Simple alias application, more sophisticated logic might be needed (e.g., whole word match)
            foreach (var alias in aliases)
            {
                processedQuery = processedQuery.Replace(alias.Key, alias.Value, StringComparison.OrdinalIgnoreCase);
            }
            return processedQuery;
        }
    }
}