using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.System.Services.Management.Domain.Aggregates;

namespace Opc.System.Services.Management.Domain.Repositories
{
    /// <summary>
    /// An interface specifying the data access methods for OpcClientInstance aggregates.
    /// It defines the persistence contract for the OpcClientInstance aggregate, isolating the domain model from data access concerns.
    /// </summary>
    public interface IOpcClientInstanceRepository
    {
        /// <summary>
        /// Retrieves an OpcClientInstance by its unique identifier.
        /// </summary>
        /// <param name="id">The client instance identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The OpcClientInstance if found; otherwise, null.</returns>
        Task<OpcClientInstance?> GetByIdAsync(ClientInstanceId id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all OpcClientInstance aggregates.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An enumerable collection of all OpcClientInstance aggregates.</returns>
        Task<IEnumerable<OpcClientInstance>> GetAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a collection of OpcClientInstances by their unique identifiers.
        /// </summary>
        /// <param name="ids">The collection of client instance identifiers.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An enumerable collection of the found OpcClientInstance aggregates.</returns>
        Task<IEnumerable<OpcClientInstance>> GetByIdsAsync(IEnumerable<ClientInstanceId> ids, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a new OpcClientInstance to the repository.
        /// </summary>
        /// <param name="clientInstance">The client instance to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(OpcClientInstance clientInstance, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing OpcClientInstance in the repository.
        /// </summary>
        /// <param name="clientInstance">The client instance to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(OpcClientInstance clientInstance, CancellationToken cancellationToken);
    }
}