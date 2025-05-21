using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Workflows.BlockchainSync.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.BlockchainSync.Activities
{
    /// <summary>
    /// Workflow activity to store voluminous data off-chain.
    /// Calls Data Service to store actual data, linking it to on-chain hash.
    /// Corresponds to REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class StoreOffChainDataActivity : StepBodyAsync
    {
        private readonly IDataServiceClient _dataServiceClient;
        private readonly ILogger<StoreOffChainDataActivity> _logger;

        public StoreOffChainDataActivity(IDataServiceClient dataServiceClient, ILogger<StoreOffChainDataActivity> logger)
        {
            _dataServiceClient = dataServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (BlockchainSyncSagaData)context.Workflow.Data;
            _logger.LogInformation("Storing off-chain data for DataId: {DataId}", sagaData.DataId);

            if (sagaData.InputDataRef == null ||
                (sagaData.InputDataRef.CriticalDataPayload == null && string.IsNullOrEmpty(sagaData.InputDataRef.CriticalDataReference)))
            {
                _logger.LogError("InputDataRef or its critical data is null/empty for DataId: {DataId}. Cannot store off-chain data.", sagaData.DataId);
                sagaData.CurrentStatus = "Failed_StoreOffChain_MissingInput";
                sagaData.FailureReason = "Critical data payload or reference for off-chain storage was not provided.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                byte[] dataToStore;
                // Assuming CriticalDataPayload is the voluminous data.
                // If CriticalDataReference points to data that needs fetching first, that logic would be here.
                if (sagaData.InputDataRef.CriticalDataPayload != null)
                {
                    dataToStore = sagaData.InputDataRef.CriticalDataPayload;
                }
                else
                {
                    // If CriticalDataReference itself is the data (e.g. large JSON string)
                    _logger.LogInformation("CriticalDataPayload is null, using CriticalDataReference as string data for off-chain storage. DataId: {DataId}", sagaData.DataId);
                    dataToStore = System.Text.Encoding.UTF8.GetBytes(sagaData.InputDataRef.CriticalDataReference);
                }

                // The SDS mentions IDataServiceClient.StoreOffChainDataAsync(data)
                var storagePath = await _dataServiceClient.StoreOffChainDataAsync(dataToStore, context.CancellationToken);

                if (string.IsNullOrEmpty(storagePath))
                {
                    _logger.LogError("Off-chain storage path is null or empty for DataId: {DataId}.", sagaData.DataId);
                    sagaData.CurrentStatus = "Failed_StoreOffChain_NoPath";
                    sagaData.FailureReason = "Data service did not return a valid off-chain storage path.";
                    return ExecutionResult.Outcome("Error");
                }

                sagaData.OffChainStoragePath = storagePath;
                _logger.LogInformation("Data stored off-chain successfully. Path: {OffChainStoragePath} for DataId: {DataId}", sagaData.OffChainStoragePath, sagaData.DataId);
                
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error storing off-chain data for DataId: {DataId}. Error: {ErrorMessage}", sagaData.DataId, ex.Message);
                sagaData.FailureReason = $"Off-Chain Data Storage failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}