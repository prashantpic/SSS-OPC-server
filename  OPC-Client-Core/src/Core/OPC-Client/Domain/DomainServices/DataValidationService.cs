using OPC.Client.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using OPC.Client.Core.Application; // For DataValidationConfiguration

namespace OPC.Client.Core.Domain.DomainServices
{
    /// <summary>
    /// Domain service for performing client-side data validation rules (e.g., range checks, data type checks) before writing data to OPC servers.
    /// Encapsulates the logic for validating data against configurable rules.
    /// REQ-CSVC-007
    /// </summary>
    public class DataValidationService
    {
        private readonly ILogger<DataValidationService> _logger;
        private readonly DataValidationConfiguration? _config;

        public DataValidationService(ILogger<DataValidationService> logger, DataValidationConfiguration? config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config; // Can be null if validation is disabled
        }

        /// <summary>
        /// Validates a value intended to be written to an OPC tag against configured rules.
        /// </summary>
        /// <param name="nodeAddress">The address of the tag.</param>
        /// <param name="value">The value to be written.</param>
        /// <returns>True if validation passes or no rule is applicable/configured; false otherwise.</returns>
        public bool ValidateWriteValue(NodeAddress nodeAddress, object? value)
        {
            if (_config == null || !_config.EnableValidation)
            {
                _logger.LogTrace("Data validation is disabled. Allowing write for Node: {NodeAddress}", nodeAddress);
                return true; // Validation disabled or no configuration
            }

            string nodeKey = nodeAddress.ToString(); // Use a consistent key for rules dictionary
            if (_config.ValidationRules == null || !_config.ValidationRules.TryGetValue(nodeKey, out var ruleDefinition))
            {
                _logger.LogTrace("No specific validation rule found for Node: {NodeAddress}. Allowing write.", nodeAddress);
                return true; // No rule for this tag
            }

            _logger.LogDebug("Validating Node: {NodeAddress} with Value: '{Value}' against Rule: '{RuleDefinition}'", nodeAddress, value, ruleDefinition);

            // Simple rule parsing example: "Range:0-100", "Type:Int32", "NotEmpty"
            // A more robust parser or dedicated rule engine objects would be better for complex rules.
            var ruleParts = ruleDefinition.Split(new[] { ':' }, 2);
            var ruleType = ruleParts[0].Trim().ToUpperInvariant();
            var ruleParams = ruleParts.Length > 1 ? ruleParts[1].Trim() : string.Empty;

            try
            {
                switch (ruleType)
                {
                    case "RANGE":
                        return ValidateRange(value, ruleParams, nodeAddress);
                    case "TYPE":
                        return ValidateType(value, ruleParams, nodeAddress);
                    case "NOTEMPTY":
                        return ValidateNotEmpty(value, nodeAddress);
                    // Add more rule types: REGEX, ENUM, LENGTH, etc.
                    default:
                        _logger.LogWarning("Unsupported validation rule type '{RuleType}' for Node: {NodeAddress}", ruleType, nodeAddress);
                        return true; // Or false, depending on policy for unknown rules
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during validation for Node: {NodeAddress}, Rule: {RuleDefinition}", nodeAddress, ruleDefinition);
                return false; // Fail validation on error
            }
        }

        private bool ValidateRange(object? value, string ruleParams, NodeAddress nodeAddress)
        {
            if (value == null || !double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue))
            {
                _logger.LogWarning("Range validation failed for Node: {NodeAddress}. Value '{Value}' is not a valid number.", nodeAddress, value);
                return false;
            }

            var rangeParts = ruleParams.Split('-');
            if (rangeParts.Length != 2 ||
                !double.TryParse(rangeParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double min) ||
                !double.TryParse(rangeParts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double max))
            {
                _logger.LogWarning("Invalid range parameters '{RuleParams}' for Node: {NodeAddress}", ruleParams, nodeAddress);
                return false; // Invalid rule format
            }

            bool isValid = numericValue >= min && numericValue <= max;
            if (!isValid)
            {
                _logger.LogWarning("Range validation failed for Node: {NodeAddress}. Value {NumericValue} is outside range [{Min}-{Max}].", nodeAddress, numericValue, min, max);
            }
            return isValid;
        }

        private bool ValidateType(object? value, string ruleParams, NodeAddress nodeAddress)
        {
            if (value == null) // Whether null is a valid type depends on specific requirements not covered here
            {
                _logger.LogWarning("Type validation for Node: {NodeAddress}. Value is null, rule expects {ExpectedType}.", nodeAddress, ruleParams);
                return false; // Or true if null is acceptable for some types
            }

            Type expectedType;
            switch (ruleParams.ToUpperInvariant())
            {
                case "INT32": case "INT": expectedType = typeof(int); break;
                case "DOUBLE": expectedType = typeof(double); break;
                case "BOOLEAN": case "BOOL": expectedType = typeof(bool); break;
                case "STRING": expectedType = typeof(string); break;
                case "DATETIME": expectedType = typeof(DateTime); break;
                // Add more types as needed
                default:
                    _logger.LogWarning("Unsupported type '{RuleParams}' in validation rule for Node: {NodeAddress}", ruleParams, nodeAddress);
                    return false; // Unknown type
            }

            bool isValid = value.GetType() == expectedType || (expectedType.IsAssignableFrom(value.GetType()));
            if (!isValid) // Try to convert if direct type match fails, e.g. int for a double field
            {
                try
                {
                    Convert.ChangeType(value, expectedType, CultureInfo.InvariantCulture);
                    isValid = true; // Conversion successful
                }
                catch
                {
                    isValid = false; // Conversion failed
                }
            }


            if (!isValid)
            {
                _logger.LogWarning("Type validation failed for Node: {NodeAddress}. Value type {ActualType} does not match expected type {ExpectedType}.", nodeAddress, value.GetType().Name, expectedType.Name);
            }
            return isValid;
        }

        private bool ValidateNotEmpty(object? value, NodeAddress nodeAddress)
        {
            if (value == null)
            {
                _logger.LogWarning("NotEmpty validation failed for Node: {NodeAddress}. Value is null.", nodeAddress);
                return false;
            }
            if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                _logger.LogWarning("NotEmpty validation failed for Node: {NodeAddress}. String value is empty or whitespace.", nodeAddress);
                return false;
            }
            // Could add checks for empty collections if applicable
            return true;
        }
    }
}