using ManagementService.Infrastructure.ConfigurationMigration.Transformers; // For MigratedClientConfiguration
using ManagementService.Application.Abstractions.Clients; // For IClientInstanceRepository
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // For async
using ManagementService.Application.Features.ConfigurationMigrations.Services; // For ValidationResult

namespace ManagementService.Infrastructure.ConfigurationMigration.Validators;

public class MigratedConfigurationValidator
{
    private readonly IClientInstanceRepository _clientInstanceRepository;
    private readonly ILogger<MigratedConfigurationValidator> _logger;
    // Inject other services if needed, e.g., schema validator, IClientConfigurationRepository for existing checks

    public MigratedConfigurationValidator(
        IClientInstanceRepository clientInstanceRepository,
        ILogger<MigratedConfigurationValidator> logger)
    {
        _clientInstanceRepository = clientInstanceRepository;
        _logger = logger;
    }

    public async Task<ValidationResult<List<MigratedClientConfiguration>>> Validate(List<MigratedClientConfiguration> configurations, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating {Count} migrated configurations.", configurations.Count);
        var overallErrors = new List<ValidationError>();
        bool isOverallValid = true;

        foreach (var config in configurations)
        {
            // 1. Resolve ClientInstanceId based on ClientInstanceName
            var clientInstance = await _clientInstanceRepository.GetByNameAsync(config.ClientInstanceName, cancellationToken);
            if (clientInstance == null)
            {
                var error = new ValidationError { PropertyName = nameof(config.ClientInstanceName), ErrorMessage = $"Client instance '{config.ClientInstanceName}' not found." };
                config.ValidationErrors.Add(error);
                overallErrors.Add(error);
                isOverallValid = false;
                _logger.LogWarning("Validation failed for config '{ConfigName}': Client '{ClientName}' not found.", config.ConfigurationName, config.ClientInstanceName);
                continue; // Cannot validate further if client does not exist
            }
            config.ClientInstanceId = clientInstance.Id; // Set resolved ID

            // 2. Validate configuration name
            if (string.IsNullOrWhiteSpace(config.ConfigurationName))
            {
                var error = new ValidationError { PropertyName = nameof(config.ConfigurationName), ErrorMessage = "Configuration name cannot be empty." };
                config.ValidationErrors.Add(error);
                overallErrors.Add(error);
                isOverallValid = false;
            }

            // 3. Validate versions
            if (config.Versions == null || !config.Versions.Any())
            {
                var error = new ValidationError { PropertyName = nameof(config.Versions), ErrorMessage = "Configuration must have at least one version." };
                config.ValidationErrors.Add(error);
                overallErrors.Add(error);
                isOverallValid = false;
            }
            else
            {
                foreach (var version in config.Versions)
                {
                    if (string.IsNullOrWhiteSpace(version.Content))
                    {
                        var error = new ValidationError { PropertyName = $"Version[{config.Versions.IndexOf(version)}].Content", ErrorMessage = "Version content cannot be empty." };
                        config.ValidationErrors.Add(error);
                        overallErrors.Add(error);
                        isOverallValid = false;
                    }
                    // Potentially validate version content against a schema here if a schema validator is injected.
                    // e.g., if (featureFlag_EnableSchemaValidation && !_schemaValidator.IsValid(version.Content)) { errors... }
                }
            }
            
            // Add more specific business rule validations as needed.
            // For example, check for duplicate configuration names for the same client if that's a constraint.
            // This might involve querying IClientConfigurationRepository.
        }

        _logger.LogInformation("Validation completed. Overall valid: {IsValid}. Total errors: {ErrorCount}", isOverallValid, overallErrors.Count);
        return new ValidationResult<List<MigratedClientConfiguration>>(isOverallValid) { Errors = overallErrors };
    }
}

// Defined here as these are closely related to validator's output and Orchestrator.cs is not in scope.
// Normally, these might live in a shared Application/Common or similar.
// public class ValidationResult<T>
// {
//     public bool IsValid { get; set; }
//     public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

//     public ValidationResult(bool isValid)
//     {
//         IsValid = isValid;
//     }
// }

// public class ValidationError
// {
//     public string PropertyName { get; set; } = string.Empty;
//     public string ErrorMessage { get; set; } = string.Empty;
// }