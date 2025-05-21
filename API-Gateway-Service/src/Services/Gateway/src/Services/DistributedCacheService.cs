using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Services
{
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

        public async Task<T?> GetAsync<T>(string key, CancellationToken token = default) where T : class
        {
            try
            {
                var jsonData = await _cache.GetStringAsync(key, token);
                if (string.IsNullOrEmpty(jsonData))
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(jsonData, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON from cache for key: {Key}", key);
                // Optionally remove corrupted cache entry
                await RemoveAsync(key, token);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data from distributed cache for key: {Key}", key);
                return null; // Or rethrow depending on desired error handling
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow, CancellationToken token = default) where T : class
        {
            await SetAsync(key, value, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow }, token);
        }

        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
        {
            if (value == null)
            {
                _logger.LogWarning("Attempted to cache null value for key: {Key}. Removing key instead.", key);
                await RemoveAsync(key, token);
                return;
            }

            try
            {
                var jsonData = JsonSerializer.Serialize(value, _jsonSerializerOptions);
                await _cache.SetStringAsync(key, jsonData, options, token);
                _logger.LogDebug("Successfully set data in cache for key: {Key}", key);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to serialize JSON for caching for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting data in distributed cache for key: {Key}", key);
            }
        }


        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            try
            {
                await _cache.RemoveAsync(key, token);
                _logger.LogDebug("Successfully removed data from cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing data from distributed cache for key: {Key}", key);
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            try
            {
                await _cache.RefreshAsync(key, token);
                _logger.LogDebug("Successfully refreshed cache entry for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache entry for key: {Key}", key);
            }
        }

        // IDistributedCacheService raw byte methods (from SDS)
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
             try
            {
                await _cache.SetAsync(key, value, options, cancellationToken);
                _logger.LogDebug("Successfully set raw byte data in cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting raw byte data in distributed cache for key: {Key}", key);
            }
        }

        public async Task<byte[]?> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = await _cache.GetAsync(key, cancellationToken);
                 if (data == null || data.Length == 0)
                {
                    _logger.LogDebug("Cache miss for raw byte data with key: {Key}", key);
                    return null;
                }
                _logger.LogDebug("Cache hit for raw byte data with key: {Key}", key);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting raw byte data from distributed cache for key: {Key}", key);
                return null;
            }
        }
    }
}