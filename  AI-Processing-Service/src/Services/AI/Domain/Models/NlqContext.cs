using System;
using System.Collections.Generic;

namespace AIService.Domain.Models
{
    public class NlqEntity
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double? Confidence { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
    }

    /// <summary>
    /// Represents the context of an NLQ request, including original query,
    /// identified intent, extracted entities, and any user-defined aliases/mappings used during processing.
    /// Supports REQ-7-015.
    /// </summary>
    public class NlqContext
    {
        public Guid Id { get; private set; }
        public string OriginalQuery { get; private set; }
        public string? ProcessedQuery { get; set; }
        public string? IdentifiedIntent { get; set; }
        public List<NlqEntity> ExtractedEntities { get; set; }
        public Dictionary<string, string> AppliedAliases { get; set; }
        public double? ConfidenceScore { get; set; } // Overall confidence for the intent
        public string? ProviderUsed { get; set; }
        public bool FallbackApplied { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; private set; }

        public NlqContext(string originalQuery)
        {
            if (string.IsNullOrWhiteSpace(originalQuery))
                throw new ArgumentException("Original query cannot be null or whitespace.", nameof(originalQuery));

            Id = Guid.NewGuid();
            OriginalQuery = originalQuery;
            ExtractedEntities = new List<NlqEntity>();
            AppliedAliases = new Dictionary<string, string>();
            Timestamp = DateTime.UtcNow;
        }

        // For deserialization or ORM
        private NlqContext()
        {
            OriginalQuery = string.Empty; // Should be initialized
            ExtractedEntities = new List<NlqEntity>();
            AppliedAliases = new Dictionary<string, string>();
        }
    }
}