using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Workflows.ReportGeneration.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity to retrieve historical data for reports.
    /// Corresponds to REQ-7-020.
    /// </summary>
    public class RetrieveHistoricalDataActivity : StepBodyAsync
    {
        private readonly IDataServiceClient _dataServiceClient;
        private readonly ILogger<RetrieveHistoricalDataActivity> _logger;

        // Inputs from SagaData (implicitly via context)
        // Outputs to SagaData

        public RetrieveHistoricalDataActivity(IDataServiceClient dataServiceClient, ILogger<RetrieveHistoricalDataActivity> logger)
        {
            _dataServiceClient = dataServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            _logger.LogInformation("Retrieving historical data for ReportId: {ReportId}", sagaData.ReportId);

            if (sagaData.RequestParameters == null)
            {
                _logger.LogError("RequestParameters are null for ReportId: {ReportId}. Cannot retrieve historical data.", sagaData.ReportId);
                sagaData.CurrentStatus = "Failed_DataRetrieval_MissingParams";
                sagaData.FailureReason = "RequestParameters for historical data retrieval were not provided.";
                return ExecutionResult.Outcome("Error");
            }
            
            try
            {
                // Assuming RequestParameters.Parameters contains necessary info for historical data query
                // The SDS mentions IDataServiceClient.QueryHistoricalDataAsync(DataQueryParameters parameters, ...)
                // Let's define a placeholder DataQueryParameters DTO for IDataServiceClient
                var queryParams = new DataQueryParameters
                {
                    // Map from sagaData.RequestParameters.Parameters or other relevant sagaData fields
                    // Example:
                    // Filters = sagaData.RequestParameters.Parameters,
                    // ReportType = sagaData.RequestParameters.ReportType
                    // AiAnalysisContext = sagaData.AiAnalysisResultUri // As per SDS: "AiAnalysisResultUri (for context, though not directly used for data query)"
                };
                if (sagaData.RequestParameters.Parameters.TryGetValue("DateRange", out var dateRange))
                {
                    queryParams.Filters.Add("DateRange", dateRange);
                }
                 if (sagaData.RequestParameters.Parameters.TryGetValue("Tags", out var tags))
                {
                    queryParams.Filters.Add("Tags", tags);
                }
                // Add AI context if relevant for the query, as per SDS
                // queryParams.AiAnalysisContext = sagaData.AiAnalysisResultUri;


                var historicalDataResult = await _dataServiceClient.QueryHistoricalDataAsync(queryParams, context.CancellationToken);

                if (historicalDataResult == null || string.IsNullOrEmpty(historicalDataResult.DataReference))
                {
                     _logger.LogWarning("Historical data retrieval returned null or empty reference for ReportId: {ReportId}.", sagaData.ReportId);
                    sagaData.CurrentStatus = "Warning_DataRetrieval_NoData"; // Or "Failed_DataRetrieval_NoData" depending on policy
                    sagaData.HistoricalDataRef = null; // Explicitly set to null or an empty marker
                }
                else
                {
                    sagaData.HistoricalDataRef = historicalDataResult.DataReference;
                    _logger.LogInformation("Historical data retrieved successfully. Reference: {HistoricalDataRef} for ReportId: {ReportId}", sagaData.HistoricalDataRef, sagaData.ReportId);
                }
                
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving historical data for ReportId: {ReportId}. Error: {ErrorMessage}", sagaData.ReportId, ex.Message);
                sagaData.FailureReason = $"Historical Data Retrieval failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore to handle
            }
        }
    }
}