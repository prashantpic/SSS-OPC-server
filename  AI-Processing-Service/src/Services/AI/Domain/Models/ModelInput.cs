using System.Collections.Generic;

namespace AIService.Domain.Models
{
    /// <summary>
    /// Represents structured input data for an AI model execution,
    /// ensuring consistency and validation according to the model's input schema.
    /// </summary>
    public class ModelInput
    {
        /// <summary>
        /// A dictionary holding the feature names and their corresponding values.
        /// The actual types and structure should conform to the AiModel's InputSchema.
        /// For example, values could be scalars, arrays, or more complex objects
        /// that can be serialized/deserialized or directly used by execution engines.
        /// </summary>
        public Dictionary<string, object> Features { get; private set; }

        public ModelInput(Dictionary<string, object> features)
        {
            Features = features ?? new Dictionary<string, object>();
        }
    }
}