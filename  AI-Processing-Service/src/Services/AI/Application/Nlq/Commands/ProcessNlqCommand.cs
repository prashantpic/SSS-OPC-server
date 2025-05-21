using MediatR;
using System.Collections.Generic;
using AIService.Application.Nlq.Models; // Assuming this namespace for result type

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
        /// The natural language query text to be processed.
        /// </summary>
        public string QueryText { get; set; }

        /// <summary>
        /// Optional context data, such as user ID, session ID, or pre-defined aliases.
        /// </summary>
        public Dictionary<string, string> ContextData { get; set; }

        public ProcessNlqCommand(string queryText, Dictionary<string, string> contextData = null)
        {
            QueryText = queryText;
            ContextData = contextData ?? new Dictionary<string, string>();
        }
    }
}