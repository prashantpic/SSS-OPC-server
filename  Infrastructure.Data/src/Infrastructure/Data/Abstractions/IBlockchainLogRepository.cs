using System.Threading;
using System.Threading.Tasks;
// using Opc.System.Domain.Models; // Assuming BlockchainTransaction is in a domain project

// Placeholder for Domain Model
public class BlockchainTransaction
{
    public Guid TransactionId { get; set; }
    public string DataHash { get; set; } = string.Empty;
}

namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// A contract for managing the off-chain metadata associated with on-chain data integrity records.
    /// </summary>
    public interface IBlockchainLogRepository
    {
        /// <summary>
        /// Persists a new blockchain transaction metadata record.
        /// </summary>
        /// <param name="logEntry">The transaction metadata to persist.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task LogTransactionAsync(BlockchainTransaction logEntry, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a transaction record by its unique data hash.
        /// </summary>
        /// <param name="dataHash">The hash of the data to find the transaction for.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The found blockchain transaction record, or null if not found.</returns>
        Task<BlockchainTransaction?> GetTransactionByHashAsync(string dataHash, CancellationToken cancellationToken);
    }
}