using OPC.Client.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OPC.Client.Core.Application; // For WriteOperationLimitConfiguration

namespace OPC.Client.Core.Domain.DomainServices
{
    /// <summary>
    /// Domain service for enforcing configurable limits on data write operations.
    /// Includes rate limits and value change thresholds.
    /// REQ-CSVC-010
    /// </summary>
    public class WriteOperationLimiterService : IDisposable
    {
        private readonly ILogger<WriteOperationLimiterService> _logger;
        private readonly WriteOperationLimitConfiguration? _config;

        // State for rate limiting per connection
        private readonly ConcurrentDictionary<string, (int Count, DateTime StartTime)> _writeRateState =
            new ConcurrentDictionary<string, (int Count, DateTime StartTime)>();

        // State for value change thresholds per tag
        private readonly ConcurrentDictionary<NodeAddress, object?> _lastWrittenValues =
            new ConcurrentDictionary<NodeAddress, object?>();

        private Timer? _rateLimitResetTimer;

        public WriteOperationLimiterService(ILogger<WriteOperationLimiterService> logger, WriteOperationLimitConfiguration? config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config;

            if (_config?.EnableLimiting == true && _config.MaxWritesPerSecond.HasValue && _config.MaxWritesPerSecond > 0)
            {
                // Timer to reset rate limiting state periodically (e.g., every second for per-second limits)
                // This is a simplified approach. A more robust solution might use a sliding window or token bucket.
                _rateLimitResetTimer = new Timer(ResetRateLimits, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                _logger.LogInformation("Write operation rate limiting enabled: Max {MaxWritesPerSecond}/sec per connection.", _config.MaxWritesPerSecond);
            }
            if (_config?.EnableLimiting == true && (_config.ValueChangeThresholds?.Any() ?? false))
            {
                _logger.LogInformation("Write operation value change threshold limiting enabled for configured tags.");
            }

            if (_config == null || !_config.EnableLimiting)
            {
                 _logger.LogInformation("Write operation limiting is disabled.");
            }
        }

        /// <summary>
        /// Checks if a write operation is allowed based on configured rate limits and value change thresholds.
        /// </summary>
        /// <param name="connectionId">Identifier of the OPC connection.</param>
        /// <param name="tagValue">The OpcDataValue to be written.</param>
        /// <returns>True if the write is allowed, false otherwise.</returns>
        public bool IsWriteAllowed(string connectionId, OpcDataValue tagValue)
        {
            if (_config == null || !_config.EnableLimiting)
            {
                return true; // Limiting disabled
            }

            // 1. Check Rate Limit
            if (_config.MaxWritesPerSecond.HasValue && _config.MaxWritesPerSecond > 0)
            {
                var rateState = _writeRateState.GetOrAdd(connectionId, _ => (0, DateTime.UtcNow));
                
                // If current window is over, reset
                if ((DateTime.UtcNow - rateState.StartTime).TotalSeconds >= 1)
                {
                    rateState = (0, DateTime.UtcNow);
                }

                if (rateState.Count >= _config.MaxWritesPerSecond.Value)
                {
                    _logger.LogWarning("Rate limit exceeded for Connection: {ConnectionId}. Count: {Count}, Max: {MaxWritesPerSecond}/sec",
                        connectionId, rateState.Count, _config.MaxWritesPerSecond.Value);
                    return false; // Rate limit exceeded
                }
                // Increment count for this attempt if allowed (or after successful write)
                // For IsWriteAllowed, we check if *this* write would exceed. If it would, return false.
                // The actual increment should happen *after* the write succeeds or is confirmed to proceed.
                // For simplicity of this check method:
                // _writeRateState[connectionId] = (rateState.Count + 1, rateState.StartTime);
                // This approach means a call to IsWriteAllowed counts towards the limit.
                // A better approach is to have an `AttemptWrite` that updates state or separate `RecordWrite`.
            }

            // 2. Check Value Change Threshold
            string nodeKey = tagValue.NodeAddress.ToString();
            if (_config.ValueChangeThresholds != null && _config.ValueChangeThresholds.TryGetValue(nodeKey, out double threshold))
            {
                if (_lastWrittenValues.TryGetValue(tagValue.NodeAddress, out object? lastValue))
                {
                    if (lastValue != null && tagValue.Value != null && IsNumeric(lastValue) && IsNumeric(tagValue.Value))
                    {
                        try
                        {
                            double numericOldVal = Convert.ToDouble(lastValue, CultureInfo.InvariantCulture);
                            double numericNewVal = Convert.ToDouble(tagValue.Value, CultureInfo.InvariantCulture);

                            if (Math.Abs(numericNewVal - numericOldVal) < threshold)
                            {
                                _logger.LogInformation("Write for Node: {NodeAddress} blocked by value change threshold. Change |{NewVal} - {OldVal}| = {Diff} < {Threshold}",
                                    tagValue.NodeAddress, numericNewVal, numericOldVal, Math.Abs(numericNewVal - numericOldVal), threshold);
                                return false; // Change is less than threshold
                            }
                        }
                        catch(Exception ex)
                        {
                             _logger.LogWarning(ex, "Error comparing values for threshold check on Node: {NodeAddress}. Allowing write.", tagValue.NodeAddress);
                        }
                    }
                }
                // If no previous value or types are not numeric, allow (or define behavior)
            }

            return true; // All checks passed
        }

        /// <summary>
        /// Call this method after a write has been successfully performed (or is about to be sent)
        /// to update the internal state for rate limiting and value thresholds.
        /// </summary>
        public void RecordSuccessfulWrite(string connectionId, OpcDataValue writtenValue)
        {
            if (_config == null || !_config.EnableLimiting) return;

            // Update rate limit state
            if (_config.MaxWritesPerSecond.HasValue && _config.MaxWritesPerSecond > 0)
            {
                _writeRateState.AddOrUpdate(connectionId, (1, DateTime.UtcNow), (_, existing) =>
                {
                    if ((DateTime.UtcNow - existing.StartTime).TotalSeconds >= 1)
                    {
                        return (1, DateTime.UtcNow); // Reset window
                    }
                    return (existing.Count + 1, existing.StartTime);
                });
            }

            // Update last written value for threshold checks
             string nodeKey = writtenValue.NodeAddress.ToString();
            if (_config.ValueChangeThresholds != null && _config.ValueChangeThresholds.ContainsKey(nodeKey))
            {
                _lastWrittenValues[writtenValue.NodeAddress] = writtenValue.Value;
            }
        }


        private void ResetRateLimits(object? state)
        {
            // This simple reset clears all connection counts.
            // More fine-grained per-connection window management is better.
            // For this timer approach, it effectively gives a fresh quota each second.
            _writeRateState.Clear();
           // _logger.LogTrace("Write operation rate limit counts reset.");
        }

        private bool IsNumeric(object? value)
        {
            if (value == null) return false;
            return value is sbyte || value is byte || value is short || value is ushort || value is int ||
                   value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
        }

        public void Dispose()
        {
            _rateLimitResetTimer?.Dispose();
        }
    }
}