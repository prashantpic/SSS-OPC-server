using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// A contract for managing transactions and persisting changes to the underlying relational data store atomically.
    /// This ensures data consistency by grouping multiple database operations into a single transaction.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Persists all changes made in the current transaction to the database.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}