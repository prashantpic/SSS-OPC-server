namespace AIService.Domain.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents structured output data from an AI model execution.
    /// (REQ-7-001, REQ-7-008)
    /// </summary>
    public class ModelOutput
    {
        /// <summary>
        /// A dictionary holding the output predictions or results from the model.
        /// Keys are output names, and values are the prediction values.
        /// The structure and types should conform to the AiModel's OutputSchema.
        /// </summary>
        public Dictionary<string, object> Predictions { get; set; }

        /// <summary>
        /// Indicates whether the model execution was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Optional error message if the execution was not successful.
        /// </summary>
        public string ErrorMessage { get; set; }

        public ModelOutput()
        {
            Predictions = new Dictionary<string, object>();
            IsSuccess = true; 
        }

         public ModelOutput(Dictionary<string, object> predictions, bool isSuccess = true, string errorMessage = null)
        {
            Predictions = predictions ?? new Dictionary<string, object>();
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
}