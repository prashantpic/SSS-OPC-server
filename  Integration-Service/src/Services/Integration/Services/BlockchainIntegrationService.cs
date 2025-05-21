using IntegrationService.Interfaces;
using IntegrationService.Adapters.Blockchain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using IntegrationService.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Linq; // Added for Linq operations
using System.Text.RegularExpressions; // Added for Regex

namespace IntegrationService.Services
{
    /// <summary>
    /// Orchestrates Blockchain integrations, handling asynchronous data logging.
    /// </summary>
    public class BlockchainIntegrationService
    {
        private readonly ILogger<BlockchainIntegrationService> _logger;
        private readonly IntegrationSettings _settings;
        private readonly IBlockchainAdaptor _blockchainAdaptor;

        public BlockchainIntegrationService(
            ILogger<BlockchainIntegrationService> logger,
            IOptions<IntegrationSettings> settings,
            IBlockchainAdaptor blockchainAdaptor)
        {
            _logger = logger;
            _settings = settings.Value;
            _blockchainAdaptor = blockchainAdaptor;

             _logger.LogInformation("BlockchainIntegrationService initialized.");

            if (_settings.FeatureFlags.EnableBlockchainLogging)
            {
                 _blockchainAdaptor.ConnectAsync()
                     .ContinueWith(task => {
                         if(task.IsFaulted) {
                             _logger.LogError(task.Exception, "Failed to connect Blockchain Adaptor on startup.");
                         } else {
                             _logger.LogInformation("Blockchain Adaptor connected successfully on startup.");
                         }
                     });
            } else {
                 _logger.LogInformation("Blockchain logging is disabled, skipping adaptor connection on startup.");
            }
        }

        public Task ProcessDataForBlockchainLoggingAsync(object dataPayload, string sourceId, DateTimeOffset timestamp, object? metadata = null)
        {
            if (!_settings.FeatureFlags.EnableBlockchainLogging)
            {
                 _logger.LogDebug("Blockchain logging is disabled by feature flag. Skipping data processing for source {SourceId}.", sourceId);
                return Task.CompletedTask;
            }

            if (!_settings.BlockchainSettings.CriticalDataCriteria.Enabled)
            {
                 _logger.LogDebug("Blockchain critical data logging criteria are disabled. Skipping data processing for source {SourceId}.", sourceId);
                 return Task.CompletedTask;
            }

            _logger.LogDebug("Evaluating data for blockchain logging. Source: {SourceId}", sourceId);

            bool isCritical = false;
            var criteria = _settings.BlockchainSettings.CriticalDataCriteria;

            if (criteria.OpcTagMatchPatterns != null && criteria.OpcTagMatchPatterns.Any())
            {
                foreach (var pattern in criteria.OpcTagMatchPatterns)
                {
                    try
                    {
                        if (Regex.IsMatch(sourceId, pattern, RegexOptions.IgnoreCase))
                        {
                            isCritical = true;
                            _logger.LogInformation("Data from source {SourceId} matched critical data criteria pattern '{Pattern}'. Marking as critical.", sourceId, pattern);
                            break;
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(ex, "Invalid regex pattern '{Pattern}' in OpcTagMatchPatterns. Skipping this pattern.", pattern);
                    }
                }
            }
            // Add other criteria checks here (e.g., based on metadata.MinSeverityLevel)

            if (isCritical)
            {
                 _logger.LogInformation("Data from source {SourceId} determined as critical. Initiating blockchain logging.", sourceId);
                string dataHash = CalculateDataHash(dataPayload);
                string metadataJson = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : string.Empty;

                var request = new BlockchainTransactionRequest
                {
                    DataPayload = dataHash,
                    SourceId = sourceId,
                    Timestamp = timestamp,
                    Metadata = metadataJson
                };

                _ = LogCriticalDataAsync(request); // Fire and forget
                return Task.CompletedTask;
            }
            else
            {
                 _logger.LogDebug("Data from source {SourceId} is not considered critical. Skipping blockchain logging.", sourceId);
                 return Task.CompletedTask;
            }
        }

        private string CalculateDataHash(object dataPayload)
        {
            _logger.LogTrace("Calculating hash for data payload.");
            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(dataPayload);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonPayload));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private async Task LogCriticalDataAsync(BlockchainTransactionRequest request)
        {
            _logger.LogInformation("Attempting to log critical data to blockchain. Source: {SourceId}, Data Hash: {DataHash}", request.SourceId, request.DataPayload);

            if (!_blockchainAdaptor.IsConnected)
            {
                 _logger.LogWarning("Blockchain Adaptor is not connected. Attempting to connect before logging.");
                try
                {
                    await _blockchainAdaptor.ConnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect Blockchain Adaptor. Cannot log data for source {SourceId}.", request.SourceId);
                    return;
                }
            }

            try
            {
                string transactionHash = await _blockchainAdaptor.LogCriticalDataAsync(request);
                _logger.LogInformation("Blockchain logging request successful. Transaction Hash: {TransactionHash} for source {SourceId}.", transactionHash, request.SourceId);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to log critical data to blockchain for source {SourceId}.", request.SourceId);
            }
        }

        public Task<BlockchainRecord?> QueryBlockchainRecordAsync(string identifier)
        {
            if (!_settings.FeatureFlags.EnableBlockchainLogging)
            {
                 _logger.LogDebug("Blockchain logging is disabled by feature flag. Querying blockchain record is not possible.");
                 return Task.FromResult<BlockchainRecord?>(null);
            }
             _logger.LogInformation("Querying blockchain for record with identifier: {Identifier}", identifier);
            return _blockchainAdaptor.GetRecordAsync(identifier);
        }
    }
}