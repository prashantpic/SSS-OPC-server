using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Defines a specific rule for client-side data validation, 
/// such as range constraints, data type checks, or custom logic.
/// </summary>
public record ValidationRule
{
    public required string TagId { get; init; } // TagId this rule applies to, or "*" for global to a type
    public required string RuleType { get; init; } // e.g., "Range", "DataType", "Regex", "NotNullOrEmpty"
    
    // Parameters for the rule, stored as a JSON string for flexibility.
    // Example for "Range": "{ \"Min\": 0, \"Max\": 100 }"
    // Example for "DataType": "{ \"ExpectedType\": \"System.Int32\" }"
    public string? ParameterJson { get; init; } 
    
    public bool Enabled { get; init; } = true;
    public string? Message { get; init; } // Custom message on validation failure
}