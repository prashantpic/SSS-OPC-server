using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Domain.Models;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Infrastructure.DataHandling
{
    // The instruction for this file was specific:
    // Define a C# interface IClientSideDataValidator. 
    // Include a method Validate(OpcPointDto dataPoint, List<ValidationRule> rules) 
    // returning a validation result (e.g., a class or record indicating bool IsValid, string ErrorMessage).
    //
    // This is different from the SDS's implied methods like ValidateWrite and ValidateDataForTransmission.
    // Sticking to the file-specific instruction.

    public record ValidationResult(bool IsValid, string? ErrorMessage = null);

    public interface IClientSideDataValidator
    {
        ValidationResult Validate(OpcPointDto dataPoint, List<ValidationRule> rules);
        // To align more with SDS, we might also need:
        // bool ValidateWriteRequest(WriteRequestDto request, IEnumerable<ValidationRule> applicableRules, out List<string> validationErrors);
        // bool ValidateBufferedItem(BufferedDataItem item, IEnumerable<ValidationRule> applicableRules, out List<string> validationErrors);
        // For now, implementing as per specific instruction.
    }
}