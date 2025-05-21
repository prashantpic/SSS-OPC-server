using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.Nlq
{
    /// <summary>
    /// Data transfer object for carrying the natural language query string
    /// and any relevant context (e.g., user information, previous interactions)
    /// via the REST API.
    /// REQ-7-013, REQ-7-014
    /// </summary>
    public class NlqRequestDto
    {
        /// <summary>
        /// The natural language query text submitted by the user.
        /// </summary>
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string QueryText { get; set; }

        /// <summary>
        /// Optional: Identifier for the user making the query.
        /// Can be used for personalization or context.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Optional: Identifier for the current session.
        /// Can be used to maintain conversation context.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Optional: Additional context data that might be relevant for
        /// interpreting the query (e.g., current view in UI, selected asset).
        /// </summary>
        public Dictionary<string, string> ContextData { get; set; }
    }
}