using IntegrationService.Adapters.IoT.Models;
using Microsoft.Extensions.Logging;
using FluentValidation; // Assuming FluentValidation is used as per .csproj and SDS

namespace IntegrationService.Validation
{
    /// <summary>
    /// Validator for data received from IoT platforms (e.g., IoTCommand).
    /// </summary>
    public class IncomingIoTDataValidator : AbstractValidator<IoTCommand> // Validating IoTCommand as per SDS
    {
        private readonly ILogger<IncomingIoTDataValidator> _logger;

        public IncomingIoTDataValidator(ILogger<IncomingIoTDataValidator> logger)
        {
            _logger = logger;
             _logger.LogInformation("IncomingIoTDataValidator initialized.");

            // --- FluentValidation Rules for IoTCommand ---
            RuleFor(command => command.CommandName)
                .NotEmpty().WithMessage("CommandName is required.")
                .MaximumLength(100).WithMessage("CommandName cannot exceed 100 characters.");

            RuleFor(command => command.TargetDeviceId)
                .NotEmpty().WithMessage("TargetDeviceId is required.")
                .MaximumLength(256).WithMessage("TargetDeviceId cannot exceed 256 characters.");
            
            // Example: Basic validation for Parameters, assuming it's not null but could be an empty object
            RuleFor(command => command.Parameters)
                .NotNull().WithMessage("Parameters object cannot be null (can be an empty object).");

            // Example: CorrelationId can be optional but if present, has a max length
            RuleFor(command => command.CorrelationId)
                .MaximumLength(128).WithMessage("CorrelationId cannot exceed 128 characters.")
                .When(command => !string.IsNullOrEmpty(command.CorrelationId));

             _logger.LogDebug("FluentValidation rules configured for IoTCommand.");
        }

        /// <summary>
        /// Validates an incoming IoT command using FluentValidation.
        /// Note: With FluentValidation, you typically call validator.Validate(command) or use middleware.
        /// This method is kept for consistency with the original SDS structure but might be redundant
        /// if FluentValidation's ASP.NET Core integration is fully utilized.
        /// </summary>
        /// <param name="command">The command to validate.</param>
        /// <returns>True if valid, false otherwise. Errors are logged.</returns>
        public bool ValidateCommand(IoTCommand command)
        {
            _logger.LogDebug("Validating incoming IoT command for CommandName: {CommandName}, TargetDevice: {TargetDeviceId}", 
                             command.CommandName, command.TargetDeviceId);

            var validationResult = this.Validate(command); // 'this.Validate' comes from AbstractValidator

            if (!validationResult.IsValid)
            {
                 _logger.LogWarning("Validation failed for IoT command '{CommandName}' targeting device '{TargetDeviceId}'. Errors:", 
                                   command.CommandName, command.TargetDeviceId);
                 foreach (var error in validationResult.Errors)
                 {
                     _logger.LogWarning("- Property: {PropertyName}, Error: {ErrorMessage}, AttemptedValue: {AttemptedValue}", 
                                       error.PropertyName, error.ErrorMessage, error.AttemptedValue);
                 }
                 return false;
            }

            _logger.LogDebug("IoT command '{CommandName}' for device '{TargetDeviceId}' passed validation.", 
                             command.CommandName, command.TargetDeviceId);
            return true;
        }
    }
}