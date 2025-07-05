namespace Services.Integration.Application.Interfaces;

/// <summary>
/// Defines the contract for logging critical data transactions to a blockchain for tamper-evidence.
/// </summary>
public interface IBlockchainAdapter
{
    /// <summary>
    /// Asynchronously logs a transaction to the configured blockchain.
    /// </summary>
    /// <param name="dataHash">A cryptographic hash of the data being logged (e.g., SHA-256).</param>
    /// <param name="transactionDetails">Additional metadata about the transaction (e.g., source, timestamp) in a structured format like JSON.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the blockchain transaction ID or hash as a string for auditing purposes.</returns>
    Task<string> LogTransactionAsync(string dataHash, string transactionDetails, CancellationToken cancellationToken);
}