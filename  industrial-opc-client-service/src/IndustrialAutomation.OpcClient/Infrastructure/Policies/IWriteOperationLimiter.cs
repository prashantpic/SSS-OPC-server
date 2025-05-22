using IndustrialAutomation.OpcClient.Application.DTOs.Common; // For WriteRequestDto
using IndustrialAutomation.OpcClient.Domain.Models; // For WriteLimitPolicy

namespace IndustrialAutomation.OpcClient.Infrastructure.Policies
{
    public interface IWriteOperationLimiter
    {
        bool IsWriteAllowed(WriteRequestDto request, TagDefinitionDto tagDefinition, IEnumerable<WriteLimitPolicy> policies, out string reason);
        void RecordSuccessfulWrite(WriteRequestDto request, TagDefinitionDto tagDefinition);
        bool RequiresConfirmation(WriteRequestDto request, TagDefinitionDto tagDefinition, IEnumerable<WriteLimitPolicy> policies);

        void LoadPolicies(IEnumerable<WriteLimitPolicy> policies);
    }
}