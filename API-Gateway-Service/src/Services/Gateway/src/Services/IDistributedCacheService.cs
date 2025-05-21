using Microsoft.Extensions.Caching.Distributed;
using System;
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
        /// Sets a value in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to cache as a byte array.</param>
        /// <param name="options">The cache entry options.</param>
        /// <param name="token">Optional cancellation token.</param>
        Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// Gets a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">Optional cancellation token.</param>
        /// <returns>The cached value as a byte array, or null if not found.</returns>
        Task<byte[]?> GetAsync(string key, CancellationToken token = default);

        /// <summary>
        /// Refreshes a value in the cache, extending its sliding expiration.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">Optional cancellation token.</param>
        Task RefreshAsync(string key, CancellationToken token = default);

        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">Optional cancellation token.</param>
        Task RemoveAsync(string key, CancellationToken token = default);

        /// <summary>
        /// Sets a JSON serializable object in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The object to cache.</param>
        /// <param name="options">The cache entry options.</param>
        /// <param name="token">Optional cancellation token.</param>
        Task SetJsonAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default);

        /// <summary>
        /// Gets a JSON deserializable object from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the object to retrieve.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="token">Optional cancellation token.</param>
        /// <returns>The deserialized object, or null if not found or on deserialization error.</returns>
        Task<T?> GetJsonAsync<T>(string key, CancellationToken token = default) where T : class;
    }
}