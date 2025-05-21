using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.Nlq
{
    public class NlqRequestDto
    {
        /// <summary>
        /// The natural language query string to be processed.
        /// REQ-7-013
        /// </summary>
        [Required]
        [MinLength(1)]
        public string QueryText { get; set; }

        /// <summary>
        /// Optional: Identifier for the user submitting the query.
        /// Can be used for personalization or context.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Optional: Identifier for the current session.
        /// Can be used for maintaining conversation context.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Optional: Additional context parameters relevant to the query.
        /// For example, current view in UI, or pre-selected filters.
        /// </summary>
        public Dictionary<string, string> ContextParameters { get; set; }

        public NlqRequestDto()
        {
            ContextParameters = new Dictionary<string, string>();
        }
    }
}