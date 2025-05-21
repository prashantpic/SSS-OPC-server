using MediatR;
using AIService.Application.Nlq.Models; // Assuming NlqProcessingResult will be here
using System.Collections.Generic;

namespace AIService.Application.Nlq.Commands
{
    /// <summary>
    /// Represents a request to process a Natural Language Query (NLQ).
    /// Contains the query text and any relevant context.
    /// REQ-7-013: Core functionality for Natural Language Query Processing.
    /// REQ-7-014: Integration with NLP Providers.
    /// </summary>
    public class ProcessNlqCommand : IRequest<NlqProcessingResult>
    {
        /// <summary>
        /// The natural language query text submitted by the user.
        /// </summary>
        public string QueryText { get; set; }

        /// <summary>
        /// Optional: Identifier of the user making the query, for context personalization.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Optional: Additional context parameters that might influence NLQ processing.
        /// For example, current dashboard view, selected asset, etc.
        /// </summary>
        public Dictionary<string, string>? ContextParameters { get; set; }

        /// <summary>
        /// Optional: Session identifier for conversational context.
        /// </summary>
        public string? SessionId { get; set; }


        public ProcessNlqCommand(string queryText, string? userId = null, Dictionary<string, string>? contextParameters = null, string? sessionId = null)
        {
            QueryText = queryText ?? throw new ArgumentNullException(nameof(queryText));
            UserId = userId;
            ContextParameters = contextParameters ?? new Dictionary<string, string>();
            SessionId = sessionId;
        }
    }
}