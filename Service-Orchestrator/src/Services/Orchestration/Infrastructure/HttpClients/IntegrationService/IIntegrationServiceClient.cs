using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrchestrationService.Infrastructure.HttpClients.IntegrationService
{
    // Placeholder DTOs
    public record DistributionTargetDto(string TargetType, string TargetAddress, Dictionary<string, object>? AdditionalParameters = null); // e.g., TargetType="Email", TargetAddress="user@example.com"
    public record BlockchainCommitRequestDto(string DataHash, string OffChainReference, Dictionary<string, object> Metadata);
    public record BlockchainCommitResultDto(string TransactionId, string Status);
    public record ReportDistributionResultDto(bool Success, string? Message);

    /// <summary>
    /// Defines the contract for communication with the external Integration Service,
    /// used for tasks like report distribution or blockchain commitments.
    /// Implements REQ-7-020, REQ-8-007.
    /// </summary>
    public interface IIntegrationServiceClient
    {
        /// <summary>
        /// Distributes a report document via the Integration Service.
        /// </summary>
        /// <param name="documentUri">The URI of the report document.</param>
        /// <param name="target">The distribution target details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the distribution operation.</returns>
        Task<ReportDistributionResultDto> DistributeReportAsync(string documentUri, DistributionTargetDto target, CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits data (hash and reference) to the blockchain via the Integration Service.
        /// </summary>
        /// <param name="dataHash">The cryptographic hash of the critical data.</param>
        /// <param name="offChainRef">A reference to the off-chain storage of the voluminous data.</param>
        /// <param name="metadata">Associated metadata for the blockchain transaction.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The ID of the transaction on the blockchain.</returns>
        Task<BlockchainCommitResultDto> CommitToBlockchainAsync(string dataHash, string offChainRef, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a notification, e.g., about workflow failure or compensation.
        /// </summary>
        /// <param name="subject">The subject of the notification.</param>
        /// <param name="message">The message body.</param>
        /// <param name="recipients">List of recipients.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendNotificationAsync(string subject, string message, List<string> recipients, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts a corrective action on the blockchain or logs for manual intervention.
        /// Used in compensation if a blockchain transaction needs rollback or annotation.
        /// Note: True blockchain rollbacks are rare; this might involve a compensating transaction or off-chain status update.
        /// </summary>
        /// <param name="originalTransactionId">The original blockchain transaction ID that failed or needs compensation.</param>
        /// <param name="reason">Reason for compensation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<BlockchainCommitResultDto> CompensateBlockchainCommitAsync(string originalTransactionId, string reason, CancellationToken cancellationToken = default);
    }
}