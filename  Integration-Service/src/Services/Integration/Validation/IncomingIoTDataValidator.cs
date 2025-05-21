namespace IntegrationService.Validation
{
    using FluentValidation;
    using IntegrationService.Adapters.IoT.Models; // Assuming DTOs are here
    using System.Text.Json;

    public class IncomingIoTDataValidator : AbstractValidator<IoTDataMessage>
    {
        public IncomingIoTDataValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID must not be empty.")
                .MaximumLength(100).WithMessage("Device ID must not exceed 100 characters.");

            RuleFor(x => x.Timestamp)
                .NotEmpty().WithMessage("Timestamp must not be empty.")
                .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddMinutes(5)).WithMessage("Timestamp cannot be in the distant future.")
                .GreaterThan(DateTimeOffset.UtcNow.AddDays(-7)).WithMessage("Timestamp cannot be older than 7 days."); // Example range

            RuleFor(x => x.Payload)
                .NotNull().WithMessage("Payload must not be null.");
                // .Must(BeValidJson).WithMessage("Payload must be valid JSON."); // If payload is string
                // If JsonElement, it's already parsed. Can add custom rules for payload structure:
                // RuleFor(x => x.Payload.ValueKind == JsonValueKind.Object || x.Payload.ValueKind == JsonValueKind.Array)
                //    .Equal(true).WithMessage("Payload must be a JSON object or array.");

            When(x => x.Metadata != null, () =>
            {
                RuleForEach(x => x.Metadata!.Keys)
                    .NotEmpty().MaximumLength(50);
                RuleForEach(x => x.Metadata!.Values)
                    .NotEmpty().MaximumLength(250);
            });
        }

        // private bool BeValidJson(JsonElement payload)
        // {
        //     // JsonElement itself implies it was parsed from valid JSON.
        //     // If payload were a string, this would be useful:
        //     // try
        //     // {
        //     //     JsonDocument.Parse(payload);
        //     //     return true;
        //     // }
        //     // catch (JsonException)
        //     // {
        //     //     return false;
        //     // }
        //     return true;
        // }
    }

    public class IncomingIoTCommandValidator : AbstractValidator<IoTCommand>
    {
        public IncomingIoTCommandValidator()
        {
            RuleFor(x => x.CommandName)
                .NotEmpty().WithMessage("Command name must not be empty.")
                .MaximumLength(100).WithMessage("Command name must not exceed 100 characters.");

            RuleFor(x => x.TargetDeviceId)
                .NotEmpty().WithMessage("Target Device ID must not be empty.")
                .MaximumLength(100).WithMessage("Target Device ID must not exceed 100 characters.");

            RuleFor(x => x.Parameters)
                .NotNull().WithMessage("Parameters must not be null.");
                // .Must(BeValidJson).WithMessage("Parameters must be valid JSON.");

            When(x => !string.IsNullOrEmpty(x.CorrelationId), () =>
            {
                RuleFor(x => x.CorrelationId)
                    .MaximumLength(128).WithMessage("Correlation ID must not exceed 128 characters.");
            });
        }
    }
}