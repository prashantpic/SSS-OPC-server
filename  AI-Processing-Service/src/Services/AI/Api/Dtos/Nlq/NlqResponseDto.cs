using System.Collections.Generic;

namespace AIService.Api.Dtos.Nlq
{
    /// <summary>
    /// Represents a detected entity in an NLQ query.
    /// </summary>
    public class NlqEntityDto
    {
        /// <summary>
        /// The type of the entity (e.g., "Device", "Location", "TimeRange").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The extracted value of the entity.
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// The original text segment from the query that represents this entity.
        /// </summary>
        public string RawValue { get; set; }

        /// <summary>
        /// Starting index of the entity in the original query text.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Ending index of the entity in the original query text.
        /// </summary>
        public int EndIndex { get; set; }
    }

    /// <summary>
    /// Data transfer object for returning the interpreted meaning (intent, entities)
    /// or data retrieved from an NLQ query via the REST API.
    /// REQ-7-013, REQ-7-014
    /// </summary>
    public class NlqResponseDto
    {
        /// <summary>
        /// The original query text submitted by the user.
        /// </summary>
        public string OriginalQuery { get; set; }

        /// <summary>
        /// The query text after any pre-processing (e.g., normalization, alias replacement).
        /// </summary>
        public string ProcessedQuery { get; set; }

        /// <summary>
        /// The primary intent detected from the query.
        /// (e.g., "GetTemperature", "FindAsset", "PredictFailure")
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// A list of entities extracted from the query.
        /// </summary>
        public List<NlqEntityDto> Entities { get; set; } = new List<NlqEntityDto>();

        /// <summary>
        /// Confidence score (typically 0.0 to 1.0) for the detected intent and entities.
        /// </summary>
        public double ConfidenceScore { get; set; }

        /// <summary>
        /// Optional: A direct textual response or summary generated based on the query.
        /// This could be the answer itself or a message indicating action taken.
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Optional: Data retrieved as a result of the NLQ, if applicable.
        /// Could be a structured object or a simple value.
        /// </summary>
        public object DataPayload { get; set; }
    }
}