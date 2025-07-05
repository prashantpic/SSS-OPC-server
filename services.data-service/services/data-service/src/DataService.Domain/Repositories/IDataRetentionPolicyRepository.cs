using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataService.Domain.Entities;

namespace DataService.Domain.Repositories
{
    /// <summary>
    /// Defines the contract for a repository specifically for managing DataRetentionPolicy entities.
    /// This provides a focused persistence interface for data lifecycle rules.
    /// Fulfills requirement REQ-DLP-017.
    /// </summary>
    public interface IDataRetentionPolicyRepository
    {
        /// <summary>
        /// Retrieves a data retention policy by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the policy.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The found DataRetentionPolicy, or null if not found.</returns>
        Task<DataRetentionPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Retrieves all data retention policies from the persistence store.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An enumerable collection of all data retention policies.</returns>
        Task<IEnumerable<DataRetentionPolicy>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new data retention policy to the persistence store.
        /// </summary>
        /// <param name="policy">The new policy to add.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(DataRetentionPolicy policy, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Marks an existing data retention policy for update.
        /// The actual persistence is handled by a Unit of Work.
        /// </summary>
        /// <param name="policy">The policy entity with updated values.</param>
        void Update(DataRetentionPolicy policy);
        
        /// <summary>
        /// Marks a data retention policy for deletion.
        /// </summary>
        /// <param name="policy">The policy to delete.</param>
        void Delete(DataRetentionPolicy policy);
    }
}