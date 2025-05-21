namespace AIService.Domain.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the context of an NLQ request, including original query,
    /// identified intent, extracted entities, and processing metadata.
    /// Supports REQ-7-014, REQ-7-015.
    /// </summary>
    public class NlqContext
    {
        /// <summary>
        /// The original natural language query submitted by the user.
        /// </summary>
        public string OriginalQuery { get; set; }

        /// <summary>
        /// The query after normalization or processing, potentially with aliases applied.
        /// </summary>
        public string ProcessedQuery { get; set; }

        /// <summary>
        /// The main intent detected from the query (e.g., "GetTemperature", "PredictFailure").
        /// </summary>
        public string IdentifiedIntent { get; set; }

        /// <summary>
        /// List of entities extracted from the query.
        /// </summary>
        public List<NlqEntity> ExtractedEntities { get; set; }

        /// <summary>
        /// Mapping of original text segments to canonical values based on user aliases (REQ-7-015).
        /// Key: Original text segment, Value: Canonical value.
        /// </summary>
        public Dictionary<string, string> AppliedAliases { get; set; }

        /// <summary>
        /// Confidence score of the intent recognition and/or entity extraction.
        /// </summary>
        public double? ConfidenceScore { get; set; }

        /// <summary>
        /// Name or type of the NLP provider that processed the query.
        /// </summary>
        public string ProviderUsed { get; set; }

        /// <summary>
        /// Indicates if a fallback strategy was used during processing (REQ-7-016).
        /// </summary>
        public bool FallbackApplied { get; set; }

        /// <summary>
        /// Optional additional metadata or results from the NLP processing.
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; }


        public NlqContext(string originalQuery)
        {
            OriginalQuery = originalQuery;
            ProcessedQuery = originalQuery; // Initially same as original
            ExtractedEntities = new List<NlqEntity>();
            AppliedAliases = new Dictionary<string, string>();
            AdditionalData = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Represents an entity extracted during NLP processing.
    /// </summary>
    public class NlqEntity
    {
        /// <summary>
        /// The type or category of the entity (e.g., "Asset", "Metric", "TimeRange").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The value of the extracted entity.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Optional confidence score for this specific entity extraction.
        /// </summary>
        public double? Confidence { get; set; }

        /// <summary>
        /// Optional starting position of the entity in the original query.
        /// </summary>
        public int? StartIndex { get; set; }

        /// <summary>
        /// Optional length of the entity text in the original query.
        /// </summary>
        public int? Length { get; set; }
    }
}