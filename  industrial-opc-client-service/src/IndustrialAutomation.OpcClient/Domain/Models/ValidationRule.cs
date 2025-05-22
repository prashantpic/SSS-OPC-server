using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Defines a specific rule for client-side data validation, 
    /// such as range constraints, data type checks, or custom logic.
    /// </summary>
    public record ValidationRule
    {
        /// <summary>
        /// Identifier of the tag this rule applies to. Can be a specific tag ID or a wildcard (e.g., "*").
        /// </summary>
        public required string TagIdPattern { get; init; }

        /// <summary>
        /// Type of validation rule (e.g., "RangeCheck", "DataTypeCheck", "RegexMatch", "NotNullOrEmpty").
        /// </summary>
        public required string RuleType { get; init; }

        /// <summary>
        /// JSON string containing parameters for the rule.
        /// Example for RangeCheck: {"Min": 0, "Max": 100}
        /// Example for DataTypeCheck: {"ExpectedType": "System.Int32"}
        /// </summary>
        public string? ParameterJson { get; init; }

        /// <summary>
        /// Indicates if this validation rule is currently active.
        /// </summary>
        public bool Enabled { get; init; } = true;

        /// <summary>
        /// Severity of the validation failure (e.g., "Error", "Warning").
        /// </summary>
        public string Severity { get; init; } = "Error";

        /// <summary>
        /// Custom message to display or log when validation fails for this rule.
        /// </summary>
        public string? CustomErrorMessage { get; init; }
    }
}