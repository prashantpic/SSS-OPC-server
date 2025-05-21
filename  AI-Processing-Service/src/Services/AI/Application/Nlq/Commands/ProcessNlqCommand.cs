using MediatR;
using AIService.Domain.Models; // For NlqContext as return type

namespace AIService.Application.Nlq.Commands
{
    /// <summary>
    /// Represents a request to process an NLQ, containing the query text and context.
    /// REQ-7-013: Natural Language Query Processing
    /// REQ-7-014: Integration with NLP Providers
    /// </summary>
    public class ProcessNlqCommand : IRequest<NlqProcessingResult>
    {
        /// <summary>
        /// The natural language query text from the user.
        /// </summary>
        public string QueryText { get; set; }

        /// <summary>
        /// Optional unique identifier for the user making the query.
        /// Can be used for personalization or context (e.g., applying user-defined aliases).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Optional session identifier for maintaining conversation context.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Optional existing NlqContext, e.g., from a previous turn in a conversation,
        /// to provide ongoing context to the NLP processor.
        /// </summary>
        public NlqContext PreviousContext { get; set; }


        public ProcessNlqCommand(string queryText, string userId = null, string sessionId = null, NlqContext previousContext = null)
        {
            QueryText = queryText;
            UserId = userId;
            SessionId = sessionId;
            PreviousContext = previousContext;
        }
    }

    /// <summary>
    /// Represents the result of NLQ processing.
    /// </summary>
    public class NlqProcessingResult
    {
        public bool Success { get; set; }
        public NlqContext ProcessedContext { get; set; }
        public string ErrorMessage { get; set; }
    }
}