using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchestrationService.Workflows.ReportGeneration.Activities; // For DTOs like ResolvedDistributionListDto

namespace OrchestrationService.Infrastructure.HttpClients.ManagementService;

/// <summary>
/// Defines the contract for communication with the external Management Service (REPO-SERVER-MGMT),
/// e.g., for fetching configurations or user/role details for RBAC checks in workflows.
/// </summary>
public interface IManagementServiceClient
{
    /// <summary>
    /// Retrieves user role details for Role-Based Access Control (RBAC) checks.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A DTO containing the user's roles and potentially other relevant details.</returns>
    Task<UserRoleDetailsDto> GetUserRolesAsync(Guid userId);

    /// <summary>
    /// Resolves a list of distribution identifiers (e.g., role names, group names)
    /// into a concrete list of recipients (e.g., email addresses).
    /// </summary>
    /// <param name="distributionIdentifiers">A list of identifiers to resolve for report distribution.</param>
    /// <returns>A DTO containing the resolved list of email addresses or other recipient details.</returns>
    Task<ResolvedDistributionListDto> ResolveDistributionListAsync(List<string> distributionIdentifiers);

    /// <summary>
    /// Notifies the Management Service or an external system that a report requires validation.
    /// </summary>
    /// <param name="workflowInstanceId">The ID of the workflow instance requiring validation.</param>
    /// <param name="reportUri">The URI of the report document that needs validation.</param>
    /// <param name="validatorRole">Optional: The role or group responsible for validation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RequestReportValidationAsync(string workflowInstanceId, string reportUri, string? validatorRole);
}