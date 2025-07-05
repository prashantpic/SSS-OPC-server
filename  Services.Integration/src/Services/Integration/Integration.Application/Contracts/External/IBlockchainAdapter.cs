namespace Opc.System.Services.Integration.Application.Contracts.External;

/// <summary>
/// Defines the contract for interacting with a permissioned blockchain.
/// This abstracts the specifics of blockchain libraries and smart contract calls.
/// </summary>
public interface IBlockchainAdapter
{
    /// <summary>
    /// Logs the hash of a data payload to the blockchain for tamper-evidence.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="dataHash">The pre-computed hash of the data to be logged.</param>
    /// <param name="metadata">Additional metadata to store with the hash on-chain.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction hash.</returns>
    Task<string> LogDataHashAsync(Guid connectionId, string dataHash, string metadata);

    /// <summary>
    /// Verifies if a transaction for a given data hash exists and is valid on the blockchain.
    /// </summary>
    /// <param name="connectionId">The unique identifier of the IntegrationConnection to use.</param>
    /// <param name="dataHash">The hash of the data to verify.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with a boolean indicating validity and the associated metadata if found.</returns>
    Task<(bool IsValid, string Metadata)> VerifyTransactionByHashAsync(Guid connectionId, string dataHash);
}