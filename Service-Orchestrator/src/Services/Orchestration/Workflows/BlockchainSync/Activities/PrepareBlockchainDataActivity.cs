using Microsoft.Extensions.Logging;
using OrchestrationService.Workflows.BlockchainSync.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.BlockchainSync.Activities
{
    /// <summary>
    /// Workflow activity to prepare data for blockchain logging.
    /// Generates cryptographic hash and structures minimal on-chain data.
    /// Corresponds to REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class PrepareBlockchainDataActivity : StepBodyAsync
    {
        private readonly ILogger<PrepareBlockchainDataActivity> _logger;

        // Inputs from SagaData (implicitly via context)
        // Outputs to SagaData

        public PrepareBlockchainDataActivity(ILogger<PrepareBlockchainDataActivity> logger)
        {
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (BlockchainSyncSagaData)context.Workflow.Data;
            _logger.LogInformation("Preparing blockchain data for DataId: {DataId}", sagaData.DataId);

            if (sagaData.InputDataRef == null || 
                (sagaData.InputDataRef.CriticalDataPayload == null && string.IsNullOrEmpty(sagaData.InputDataRef.CriticalDataReference)))
            {
                _logger.LogError("InputDataRef or its critical data is null/empty for DataId: {DataId}. Cannot prepare blockchain data.", sagaData.DataId);
                sagaData.CurrentStatus = "Failed_PrepareData_MissingInput";
                sagaData.FailureReason = "Critical data payload or reference was not provided.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                byte[] dataToHash;
                if (sagaData.InputDataRef.CriticalDataPayload != null)
                {
                    dataToHash = sagaData.InputDataRef.CriticalDataPayload;
                }
                else
                {
                    // If CriticalDataReference is a path or URI, logic to fetch it would go here.
                    // For this example, let's assume CriticalDataReference IS the string data if payload is null.
                    // This part needs clarification based on actual usage of CriticalDataReference.
                    // Let's assume if payload is null, CriticalDataReference is a string to be hashed.
                    _logger.LogInformation("CriticalDataPayload is null, using CriticalDataReference as string data for hashing. DataId: {DataId}", sagaData.DataId);
                    dataToHash = Encoding.UTF8.GetBytes(sagaData.InputDataRef.CriticalDataReference); 
                }

                // Compute hash (e.g., SHA-256)
                using (var sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(dataToHash);
                    sagaData.DataHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                _logger.LogInformation("Data hash generated: {DataHash} for DataId: {DataId}", sagaData.DataHash, sagaData.DataId);

                // Potentially structure minimal on-chain data (e.g., metadata + hash)
                // For this example, we'll just store the hash. The CommitToBlockchainActivity will use this and metadata.
                // If specific on-chain data structure needs to be prepared here (e.g., a JSON string), do it.
                // sagaData.OnChainPayload = JsonSerializer.Serialize(new { Hash = sagaData.DataHash, Metadata = sagaData.InputDataRef.Metadata });

                return await Task.FromResult(ExecutionResult.Next());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error preparing blockchain data for DataId: {DataId}. Error: {ErrorMessage}", sagaData.DataId, ex.Message);
                sagaData.FailureReason = $"Blockchain Data Preparation failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}