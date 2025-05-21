namespace AIService.Domain.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents structured input data for an AI model execution.
    /// Ensures consistency and validation according to the model's input schema.
    /// (REQ-7-002, REQ-7-009)
    /// </summary>
    public class ModelInput
    {
        /// <summary>
        /// A dictionary holding the input features for the model.
        /// Keys are feature names, and values are the feature values.
        /// The structure and types should conform to the AiModel's InputSchema.
        /// </summary>
        public Dictionary<string, object> Features { get; set; }

        public ModelInput()
        {
            Features = new Dictionary<string, object>();
        }

        public ModelInput(Dictionary<string, object> features)
        {
            Features = features ?? new Dictionary<string, object>();
        }
    }
}