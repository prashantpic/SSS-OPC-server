using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrchestrationService.Infrastructure.HttpClients.ManagementService
{
    // Placeholder DTOs
    public record UserRolesDto(string UserId, List<string> Roles);
    public record DistributionDetailsDto(string TargetId, string TargetType, List<string> Recipients, Dictionary<string,object> Configuration);
    public record ReportValidationStatusDto(string ReportId, string Status, string? ValidatedBy, System.DateTime? ValidationTimestamp);


    /// <summary>
    /// Defines the contract for communication with the external Management Service,
    /// e.g., for fetching configurations or user/role details for RBAC checks in workflows.
    /// Implements REQ-7-022.
    /// </summary>
    public interface IManagementServiceClient
    {
        /// <summary>
        /// Gets user roles for a given user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User roles information.</returns>
        Task<UserRolesDto?> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets distribution details for a given distribution target ID.
        /// </summary>
        /// <param name="distributionTargetId">The ID of the distribution target (e.g., a group name).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Distribution details including recipients and configuration.</returns>
        Task<DistributionDetailsDto?> GetDistributionDetailsAsync(string distributionTargetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the validation status of a report from the Management Service.
        /// </summary>
        /// <param name="reportId">The ID of the report to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The validation status of the report.</returns>
        Task<ReportValidationStatusDto?> GetReportValidationStatusAsync(string reportId, CancellationToken cancellationToken = default);
    }
}