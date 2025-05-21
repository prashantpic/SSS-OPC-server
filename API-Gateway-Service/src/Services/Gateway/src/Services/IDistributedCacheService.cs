using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    /// <summary>
    /// Interface for distributed caching operations.
    /// Defines the contract for a service that interacts with a distributed cache,
    /// abstracting the underlying cache provider.
    /// </summary>
    public interface IDistributedCacheService
    {
        /// <summary>
        /// Gets a string from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>The cached string, or null if the key is not found.</returns>
        Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a string in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The string value to cache.</param>
        /// <param name="options">The cache entry options.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>The cached value, or default(T) if the key is not found or deserialization fails.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="options">The cache entry options.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes a value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="cancellationToken">Optional. A CancellationToken to observe while waiting for the task to complete.</param>
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    }
}