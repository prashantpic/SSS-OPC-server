using Microsoft.EntityFrameworkCore;
using Opc.System.Infrastructure.Data.Abstractions;
using Opc.System.Infrastructure.Data.Relational;
// using Opc.System.Domain.Models; // Assuming BlockchainTransaction is in a domain project
using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Infrastructure.Data.Relational.Repositories
{
    /// <summary>
    /// Handles the storage and retrieval of blockchain transaction metadata from the PostgreSQL database.
    /// </summary>
    public class BlockchainLogRepository : IBlockchainLogRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockchainLogRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public BlockchainLogRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists a new blockchain transaction metadata record. The change is staged and must be saved via IUnitOfWork.
        /// </summary>
        /// <param name="logEntry">The transaction metadata to persist.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation. (Not used in this implementation, but required by interface).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task LogTransactionAsync(BlockchainTransaction logEntry, CancellationToken cancellationToken)
        {
            _context.BlockchainTransactions.Add(logEntry);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves a transaction record by its unique data hash.
        /// </summary>
        /// <param name="dataHash">The hash of the data to find the transaction for.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The found blockchain transaction record, or null if not found.</returns>
        public async Task<BlockchainTransaction?> GetTransactionByHashAsync(string dataHash, CancellationToken cancellationToken)
        {
            return await _context.BlockchainTransactions
                .FirstOrDefaultAsync(t => t.DataHash == dataHash, cancellationToken);
        }
    }
}