using IndustrialAutomation.OpcClient.Application.DTOs.Common; // For OpcPointDto
using IndustrialAutomation.OpcClient.Domain.Models; // For ValidationRule
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Infrastructure.DataHandling
{
    public interface IClientSideDataValidator
    {
        ValidationResult Validate(OpcPointDto dataPoint, List<ValidationRule> rules);
        ValidationResult ValidateWriteRequest(WriteRequestDto writeRequest, List<ValidationRule> rules, TagDefinitionDto tagDefinition);
    }

    public record ValidationResult
    {
        public bool IsValid { get; init; } = true;
        public List<string> ErrorMessages { get; init; } = new List<string>();
    }
}