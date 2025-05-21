using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    /// <summary>
    /// Defines the contract for a distributed cache service.
    /// </summary>
    public interface IDistributedCacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default);
        Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);
        Task SetStringAsync(string key, string value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides an abstraction layer for distributed caching operations.
    /// This service is used by Ocelot's caching mechanism or custom caching handlers
    /// to store and retrieve responses or other data, supporting cross-service caching strategies.
    /// </summary>
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                // Add other options as needed
            };
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            try
            {
                var jsonData = await _cache.GetStringAsync(key, cancellationToken);
                if (jsonData == null)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return default;
                }
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(jsonData, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item from cache. Key: {Key}", key);
                return default; // Or rethrow, depending on error handling strategy
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            options ??= new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Default sliding expiration

            try
            {
                var jsonData = JsonSerializer.Serialize(value, _jsonSerializerOptions);
                await _cache.SetStringAsync(key, jsonData, options, cancellationToken);
                _logger.LogDebug("Item set in cache. Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting item in cache. Key: {Key}", key);
                // Or rethrow
            }
        }

        /// <inheritdoc/>
        public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (value == null)
                {
                    _logger.LogDebug("Cache miss (string) for key: {Key}", key);
                }
                else
                {
                    _logger.LogDebug("Cache hit (string) for key: {Key}", key);
                }
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting string from cache. Key: {Key}", key);
                return null; // Or rethrow
            }
        }

        /// <inheritdoc/>
        public async Task SetStringAsync(string key, string value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value)); // Or allow empty string based on need

            options ??= new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Default sliding expiration

            try
            {
                await _cache.SetStringAsync(key, value, options, cancellationToken);
                _logger.LogDebug("String set in cache. Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting string in cache. Key: {Key}", key);
                // Or rethrow
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Item removed from cache. Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cache. Key: {Key}", key);
                // Or rethrow
            }
        }

        /// <inheritdoc/>
        public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            try
            {
                await _cache.RefreshAsync(key, cancellationToken);
                _logger.LogDebug("Item refreshed in cache. Key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing item in cache. Key: {Key}", key);
                // Or rethrow
            }
        }
    }
}