using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Domain.Models;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Infrastructure.Policies
{
    // Sticking to the specific instruction for this file:
    // IsWriteAllowed(WriteRequestDto request, out string reason), 
    // RecordSuccessfulWrite(WriteRequestDto request), 
    // RequiresConfirmation(WriteRequestDto request).
    public interface IWriteOperationLimiter
    {
        bool IsWriteAllowed(WriteRequestDto request, out string reason);
        void RecordSuccessfulWrite(WriteRequestDto request);
        bool RequiresConfirmation(WriteRequestDto request); // e.g., for writes below a significant change threshold
        void LoadPolicies(List<WriteLimitPolicy> policies);
    }
}