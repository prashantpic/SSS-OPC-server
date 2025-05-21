using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using IntegrationService.Adapters.Blockchain.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts; // For Account usage if private key is managed directly
// For Nethereum.KeyStore if using keystore files
// For Nethereum.HdWallet if using HD wallets

using Polly.CircuitBreaker;
using Polly.Retry;


namespace IntegrationService.Adapters.Blockchain
{
    public class NethereumBlockchainAdaptor : IBlockchainAdaptor
    {
        private readonly BlockchainSettings _settings;
        private readonly ICredentialManager _credentialManager;
        private readonly ILogger<NethereumBlockchainAdaptor> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private Web3 _web3;
        private string _contractAbi;
        private Account _account; // If using private key directly

        public NethereumBlockchainAdaptor(
            IOptions<BlockchainSettings> settings,
            ICredentialManager credentialManager,
            ILogger<NethereumBlockchainAdaptor> logger,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _credentialManager = credentialManager ?? throw new ArgumentNullException(nameof(credentialManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _retryPolicy = retryPolicyFactory.CreateAsyncRetryPolicy(_settings.Resiliency?.RetryAttempts ?? 3, 
                                                                    TimeSpan.FromSeconds(_settings.Resiliency?.RetryDelaySeconds ?? 2));
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.CreateAsyncCircuitBreakerPolicy(
                                                                    _settings.Resiliency?.ExceptionsAllowedBeforeBreaking ?? 5, 
                                                                    TimeSpan.FromSeconds(_settings.Resiliency?.DurationOfBreakSeconds ?? 30));
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Connecting to blockchain RPC endpoint: {RpcUrl}", _settings.RpcUrl);
            
            // Retrieve private key securely
            var privateKey = await _credentialManager.GetCredentialAsync(_settings.CredentialIdentifier, cancellationToken);
            if (string.IsNullOrEmpty(privateKey))
            {
                _logger.LogError("Private key could not be retrieved for blockchain interaction using identifier: {CredentialIdentifier}", _settings.CredentialIdentifier);
                throw new InvalidOperationException("Blockchain private key is missing or could not be retrieved.");
            }
            
            try
            {
                _account = new Account(privateKey, _settings.ChainId.HasValue ? new BigInteger(_settings.ChainId.Value) : null);
                _web3 = new Web3(_account, _settings.RpcUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Web3 account with private key from {CredentialIdentifier}", _settings.CredentialIdentifier);
                throw;
            }


            // Load ABI
            if (!File.Exists(_settings.SmartContractAbiPath))
            {
                _logger.LogError("Smart contract ABI file not found at path: {SmartContractAbiPath}", _settings.SmartContractAbiPath);
                throw new FileNotFoundException("Smart contract ABI file not found.", _settings.SmartContractAbiPath);
            }
            _contractAbi = await File.ReadAllTextAsync(_settings.SmartContractAbiPath, cancellationToken);

            // Test connection
            try
            {
                var blockNumber = await _retryPolicy.ExecuteAsync(ct => _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync(), cancellationToken);
                _logger.LogInformation("Successfully connected to blockchain. Current block number: {BlockNumber}", blockNumber.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to blockchain RPC endpoint {RpcUrl} or retrieve block number.", _settings.RpcUrl);
                throw;
            }
        }

        public async Task<BlockchainRecord> LogCriticalDataAsync(BlockchainTransactionRequest request, CancellationToken cancellationToken)
        {
            if (_web3 == null || string.IsNullOrEmpty(_contractAbi))
            {
                _logger.LogWarning("Blockchain adaptor is not connected. Attempting to connect before logging data.");
                await ConnectAsync(cancellationToken); // Ensure connection
                 if (_web3 == null || string.IsNullOrEmpty(_contractAbi))
                 {
                    _logger.LogError("Failed to log critical data. Blockchain adaptor not connected.");
                    throw new InvalidOperationException("Blockchain adaptor not connected.");
                 }
            }

            _logger.LogInformation("Attempting to log critical data to blockchain. DataHash: {DataHash}, SourceId: {SourceId}", request.DataHash, request.SourceId);

            var contract = _web3.Eth.GetContract(_contractAbi, _settings.SmartContractAddress);
            
            // Assuming a smart contract function like: logData(bytes32 dataHash, string sourceId, uint256 timestamp)
            // Nethereum expects byte[] for bytes32, not hex string directly for function params.
            // Hex string dataHash needs to be converted to byte[32].
            // For simplicity, if DataHash is already a hex string like "0x...", Nethereum might handle it.
            // If it's a raw hash, it needs padding/conversion.
            // Let's assume request.DataHash is "0x" prefixed hex string.
            
            // Ensure DataHash is 32 bytes (64 hex characters + "0x")
            byte[] dataHashBytes;
            try 
            {
                // dataHashBytes = Nethereum.Hex.HexConvertors.Extensions.HexToByteArray(request.DataHash.StartsWith("0x") ? request.DataHash.Substring(2) : request.DataHash);
                // if (dataHashBytes.Length != 32) throw new ArgumentException("DataHash must be 32 bytes.");
                // For Nethereum contract function calls, it often handles "0x" prefixed strings for bytes32 correctly.
                // Let's pass it as string and see if Nethereum's parameter encoder handles it. If not, explicit byte[] conversion is needed.
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "Invalid DataHash format: {DataHash}. Must be a 32-byte hex string.", request.DataHash);
                throw new ArgumentException("Invalid DataHash format.", nameof(request.DataHash), ex);
            }


            // The function name in ABI needs to match. E.g., "logData"
            var logDataFunction = contract.GetFunction(_settings.SmartContractFunctionName ?? "logData"); 

            TransactionReceipt transactionReceipt = null;
            string transactionHash = null;

            try
            {
                // Parameters should match the smart contract function signature
                // Example: logData(bytes32 dataHash, string sourceId, uint timestamp)
                // Nethereum uses object[] for parameters.
                var functionParams = new object[]
                {
                    request.DataHash, // Assuming this is a string like "0x..." Nethereum can convert or pre-convert to byte[]
                    request.SourceId,
                    new BigInteger(request.Timestamp.ToUnixTimeSeconds()) // Convert DateTimeOffset to Unix timestamp (uint256)
                };

                if (request.SmartContractParameters != null && request.SmartContractParameters.Count > 0)
                {
                    // If generic parameters are provided, use them instead
                    functionParams = request.SmartContractParameters.ToArray();
                }


                await _circuitBreakerPolicy.ExecuteAsync(async ct =>
                {
                    // Estimate gas
                    var gasEstimate = await _retryPolicy.ExecuteAsync(token =>
                        logDataFunction.EstimateGasAsync(_account.Address, null, null, functionParams), ct);

                    var gasPrice = _settings.GasPriceGwei.HasValue 
                        ? Web3.Convert.ToWei(_settings.GasPriceGwei.Value, Nethereum.Util.UnitConversion.EthUnit.Gwei) 
                        : null; // Let Nethereum determine or use network default


                    _logger.LogDebug("Sending transaction to contract {ContractAddress}, function {FunctionName}. GasEstimate: {Gas}, GasPrice: {GasPrice}",
                        _settings.SmartContractAddress, logDataFunction.Name, gasEstimate.Value, gasPrice?.ToString() ?? "auto");

                    transactionHash = await _retryPolicy.ExecuteAsync(token =>
                        logDataFunction.SendTransactionAsync(_account.Address, gasEstimate, gasPrice, null, token, functionParams), ct);
                    
                    _logger.LogInformation("Transaction sent. Hash: {TransactionHash}. Waiting for receipt...", transactionHash);

                    // Wait for receipt (configurable timeout)
                    // This can take time. Consider making this part truly async / fire-and-forget with a callback or event.
                    transactionReceipt = await _web3.Eth.Transactions.GetTransactionReceipt
                        .SendRequestAsync(transactionHash, new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token); // Add timeout

                    int receiptPollCount = 0;
                    while (transactionReceipt == null && receiptPollCount < (_settings.ReceiptPollMaxAttempts ?? 20) && !ct.IsCancellationRequested) // Max 20 polls, ~1 minute if 3s delay
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_settings.ReceiptPollIntervalSeconds ?? 3), ct);
                        transactionReceipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash, ct);
                        receiptPollCount++;
                        _logger.LogDebug("Polling for transaction receipt... Attempt {Attempt}", receiptPollCount);
                    }

                    if (transactionReceipt == null)
                    {
                        _logger.LogError("Transaction receipt not found for hash {TransactionHash} after multiple polls.", transactionHash);
                        throw new TimeoutException($"Transaction receipt not obtained for {transactionHash}.");
                    }

                    if (transactionReceipt.Status.Value == BigInteger.Zero) // 0 for failure, 1 for success
                    {
                        _logger.LogError("Blockchain transaction failed. Hash: {TransactionHash}, Status: {Status}, GasUsed: {GasUsed}, BlockNumber: {BlockNumber}",
                            transactionHash, transactionReceipt.Status.Value, transactionReceipt.GasUsed.Value, transactionReceipt.BlockNumber.Value);
                        // Potentially inspect logs for revert reasons if available in receipt
                        throw new Exception($"Blockchain transaction {transactionHash} failed with status 0.");
                    }

                    _logger.LogInformation("Transaction successful. Hash: {TransactionHash}, BlockNumber: {BlockNumber}, GasUsed: {GasUsed}",
                        transactionHash, transactionReceipt.BlockNumber.Value, transactionReceipt.GasUsed.Value);

                }, cancellationToken);


                return new BlockchainRecord
                {
                    TransactionHash = transactionReceipt.TransactionHash,
                    BlockNumber = (long)transactionReceipt.BlockNumber.Value,
                    GasUsed = (long)transactionReceipt.GasUsed.Value,
                    Status = transactionReceipt.Status.Value == BigInteger.One ? "Success" : "Failed",
                    Timestamp = request.Timestamp, // Or from block if preferred: await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(transactionReceipt.BlockNumber)
                    DataHash = request.DataHash, // Assuming DataHash is part of the request
                    SourceId = request.SourceId   // Assuming SourceId is part of the request
                };
            }
            catch (BrokenCircuitException bce)
            {
                _logger.LogError(bce, "Circuit breaker open. Failed to log data to blockchain.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data to blockchain. TransactionHash (if available): {TransactionHash}", transactionHash ?? "N/A");
                throw; // Rethrow to indicate failure
            }
        }
    }
}