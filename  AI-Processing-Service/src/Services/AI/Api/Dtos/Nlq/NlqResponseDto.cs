using System.Collections.Generic;

namespace AIService.Api.Dtos.Nlq
{
    public class NlqResponseDto
    {
        /// <summary>
        /// The original query text submitted by the user.
        /// </summary>
        public string OriginalQuery { get; set; }

        /// <summary>
        /// The query text after any preprocessing or normalization.
        /// </summary>
        public string ProcessedQuery { get; set; }

        /// <summary>
        /// The primary intent detected from the query.
        /// E.g., "GetTemperature", "PredictFailure", "ShowReport".
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// A list of entities extracted from the query.
        /// E.g., Asset names, time ranges, metric types.
        /// </summary>
        public List<NlqEntityDto> Entities { get; set; }

        /// <summary>
        /// The confidence score (typically 0.0 to 1.0) of the intent recognition
        /// and entity extraction process.
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// A direct response message or data payload if the NLQ resulted in
        /// immediate data retrieval or action. Could be a summary, a value, or a structured object.
        /// This field might be null if the NLQ is just for interpretation.
        /// </summary>
        public string ResponseMessage { get; set; } // Could be object for structured data

        /// <summary>
        /// Mapping of original text segments to canonical values based on user aliases.
        /// REQ-7-015
        /// </summary>
        public Dictionary<string, string> AppliedAliases { get; set; }

        /// <summary>
        /// Name or type of the NLP provider that processed the query.
        /// </summary>
        public string ProviderUsed { get; set; }

        /// <summary>
        /// Indicates if a fallback strategy was used for processing.
        /// REQ-7-016
        /// </summary>
        public bool FallbackApplied { get; set; }


        public NlqResponseDto()
        {
            Entities = new List<NlqEntityDto>();
            AppliedAliases = new Dictionary<string, string>();
        }
    }

    public class NlqEntityDto
    {
        /// <summary>
        /// The name or role of the entity (e.g., "AssetName", "Date").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The extracted value of the entity.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of the entity (e.g., "Location", "DeviceType", "TimePeriod").
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Confidence score for this specific entity extraction.
        /// </summary>
        public double Confidence { get; set; }
    }
}