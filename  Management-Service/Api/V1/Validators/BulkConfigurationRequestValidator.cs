using FluentValidation;
using ManagementService.Api.V1.DTOs;

namespace ManagementService.Api.V1.Validators;

public class BulkConfigurationRequestValidator : AbstractValidator<BulkConfigurationRequestDto>
{
    public BulkConfigurationRequestValidator()
    {
        RuleFor(x => x.ClientInstanceIds)
            .NotNull().WithMessage("Client instance IDs list cannot be null.")
            .NotEmpty().WithMessage("Client instance IDs list cannot be empty.");

        RuleForEach(x => x.ClientInstanceIds)
            .NotEmpty().WithMessage("Client instance ID cannot be an empty GUID.");

        RuleFor(x => x.ConfigurationVersionId)
            .NotEmpty().WithMessage("Configuration version ID cannot be an empty GUID.");
    }
}