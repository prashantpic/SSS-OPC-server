using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Services.Integration.Application.Interfaces;

namespace Services.Integration.Infrastructure.Adapters.Blockchain;

/// <summary>
/// Placeholder for Ethereum settings configuration.
/// </summary>
public class EthereumSettings
{
    public required string NodeUrl { get; set; }
    public required string PrivateKey { get; set; }
    public required string LoggingContractAddress { get; set; }
}

/// <summary>
/// Implements the IBlockchainAdapter for logging transactions to an Ethereum-compatible network using Nethereum.
/// </summary>
public class EthereumAdapter : IBlockchainAdapter
{
    private readonly ILogger<EthereumAdapter> _logger;
    private readonly EthereumSettings _settings;
    private readonly IWeb3 _web3;

    // A minimal ABI for a smart contract with a function like:
    // function logData(bytes32 dataHash, string calldata transactionDetails) public
    private const string ContractAbi = @"[{""type"":""function"",""name"":""logData"",""stateMutability"":""nonpayable"",""inputs"":[{""name"":""dataHash"",""type"":""bytes32""},{""name"":""transactionDetails"",""type"":""string""}],""outputs"":[]}]";

    /// <summary>
    /// Initializes a new instance of the <see cref="EthereumAdapter"/> class.
    /// </summary>
    /// <param name="logger">The logger for structured logging.</param>
    /// <param name="settingsOptions">The Ethereum network and account settings.</param>
    public EthereumAdapter(ILogger<EthereumAdapter> logger, IOptions<EthereumSettings> settingsOptions)
    {
        _logger = logger;
        _settings = settingsOptions.Value;

        if (string.IsNullOrEmpty(_settings.PrivateKey) || _settings.PrivateKey == "Reference to Key Vault")
        {
            throw new InvalidOperationException("Ethereum PrivateKey is not configured.");
        }

        var account = new Account(_settings.PrivateKey);
        _web3 = new Web3(account, _settings.NodeUrl);
    }

    /// <inheritdoc />
    public async Task<string> LogTransactionAsync(string dataHash, string transactionDetails, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to log transaction to Ethereum contract at address {ContractAddress}", _settings.LoggingContractAddress);
        
        try
        {
            var contract = _web3.Eth.GetContract(ContractAbi, _settings.LoggingContractAddress);
            var logDataFunction = contract.GetFunction("logData");
            
            // Convert string hash to byte[32] for bytes32 type
            var dataHashBytes = new byte[32];
            var sourceBytes = Encoding.UTF8.GetBytes(dataHash.StartsWith("0x") ? dataHash[2..] : dataHash);
            Buffer.BlockCopy(sourceBytes, 0, dataHashBytes, 0, Math.Min(32, sourceBytes.Length));

            _logger.LogDebug("Sending transaction to logData function with hash {DataHash}", dataHash);

            var transactionReceipt = await logDataFunction.SendTransactionAndWaitForReceiptAsync(
                from: _web3.TransactionManager.Account.Address,
                gas: null, // Let Nethereum estimate gas
                gasPrice: null,
                value: null,
                cancellationToken: cancellationToken,
                dataHashBytes, 
                transactionDetails);

            if (transactionReceipt.Status.Value == 1) // 1 means success
            {
                _logger.LogInformation("Successfully logged transaction to Ethereum. Transaction Hash: {TransactionHash}", transactionReceipt.TransactionHash);
                return transactionReceipt.TransactionHash;
            }
            else
            {
                _logger.LogError("Ethereum transaction failed. Transaction Hash: {TransactionHash}, Status: {Status}", transactionReceipt.TransactionHash, transactionReceipt.Status.Value);
                throw new InvalidOperationException($"Ethereum transaction failed with status {transactionReceipt.Status.Value}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while logging transaction to Ethereum.");
            throw;
        }
    }
}