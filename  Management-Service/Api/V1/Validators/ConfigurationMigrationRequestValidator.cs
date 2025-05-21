using FluentValidation;
using ManagementService.Api.V1.DTOs;

namespace ManagementService.Api.V1.Validators;

public class ConfigurationMigrationRequestValidator : AbstractValidator<ConfigurationMigrationRequestDto>
{
    public ConfigurationMigrationRequestValidator()
    {
        RuleFor(x => x.FileContentBase64)
            .NotEmpty().WithMessage("File content (Base64) cannot be empty.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name cannot be empty.")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters.");

        RuleFor(x => x.SourceFormat)
            .NotEmpty().WithMessage("Source format cannot be empty.")
            .Must(BeAValidFormat).WithMessage("Source format must be 'CSV' or 'XML'.");
    }

    private bool BeAValidFormat(string format)
    {
        return !string.IsNullOrEmpty(format) && 
               (format.Equals("CSV", StringComparison.OrdinalIgnoreCase) || 
                format.Equals("XML", StringComparison.OrdinalIgnoreCase));
    }
}