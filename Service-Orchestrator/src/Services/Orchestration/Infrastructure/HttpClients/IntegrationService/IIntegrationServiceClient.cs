using System.Collections.Generic;
using System.Threading.Tasks;
using OrchestrationService.Workflows.ReportGeneration.Activities; // For DTOs like EmailDetailsDto
using OrchestrationService.Workflows.BlockchainSync.Activities;   // For DTOs like BlockchainTransactionResultDto

namespace OrchestrationService.Infrastructure.HttpClients.IntegrationService;

/// <summary>
/// Defines the contract for communication with the external Integration Service (REPO-INTEGRATION),
/// used for tasks like report distribution (e.g., email) or blockchain commitments.
/// </summary>
public interface IIntegrationServiceClient
{
    /// <summary>
    /// Sends an email for report distribution or notifications.
    /// </summary>
    /// <param name="details">Details of the email to be sent, including recipients, subject, body, and optionally attachments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendEmailAsync(EmailDetailsDto details);

    /// <summary>
    /// Commits a data hash and reference to the blockchain via the Integration Service.
    /// </summary>
    /// <param name="dataHash">The cryptographic hash of the data to be committed.</param>
    /// <param name="offChainStoragePath">A reference (e.g., URI) to where the voluminous data is stored off-chain.</param>
    /// <returns>A DTO containing the result of the blockchain transaction, such as the transaction ID.</returns>
    Task<BlockchainTransactionResultDto> CommitHashToBlockchainAsync(string dataHash, string offChainStoragePath);

    /// <summary>
    /// Sends a notification, typically used during compensation or for alerting.
    /// </summary>
    /// <param name="recipients">List of recipients (e.g., email addresses or user IDs).</param>
    /// <param name="subject">The subject of the notification.</param>
    /// <param name="body">The body content of the notification.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAdminNotificationAsync(List<string> recipients, string subject, string body);
}