using System.Collections.Generic;

namespace AIService.Domain.Models
{
    /// <summary>
    /// Represents structured output data from an AI model execution.
    /// </summary>
    public class ModelOutput
    {
        /// <summary>
        /// A dictionary holding the output names and their corresponding predicted values.
        /// The actual types and structure conform to the AiModel's OutputSchema.
        /// </summary>
        public Dictionary<string, object> Results { get; private set; }

        /// <summary>
        /// Optional. Raw output from the model if specific parsing is needed upstream or for debugging.
        /// </summary>
        public object? RawOutput { get; set; }

        /// <summary>
        /// Indicates if the prediction was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the prediction was not successful.
        /// </summary>
        public string? ErrorMessage { get; set; }


        public ModelOutput(Dictionary<string, object> results, bool isSuccess = true, string? errorMessage = null)
        {
            Results = results ?? new Dictionary<string, object>();
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
}