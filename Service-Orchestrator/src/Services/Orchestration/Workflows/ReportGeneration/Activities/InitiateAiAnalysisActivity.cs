using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.AiService;
using OrchestrationService.Workflows.ReportGeneration.Models; // Assuming ReportGenerationSagaData is in this namespace
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity to initiate AI analysis for report generation.
    /// Corresponds to REQ-7-020.
    /// </summary>
    public class InitiateAiAnalysisActivity : StepBodyAsync
    {
        private readonly IAiServiceClient _aiServiceClient;
        private readonly ILogger<InitiateAiAnalysisActivity> _logger;

        public ReportGenerationSagaInput RequestParameters { get; set; }
        public string AiAnalysisResultUri { get; set; }


        public InitiateAiAnalysisActivity(IAiServiceClient aiServiceClient, ILogger<InitiateAiAnalysisActivity> logger)
        {
            _aiServiceClient = aiServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            _logger.LogInformation("Initiating AI analysis for ReportId: {ReportId}", ((ReportGenerationSagaData)context.Workflow.Data).ReportId);

            if (((ReportGenerationSagaData)context.Workflow.Data).RequestParameters == null)
            {
                _logger.LogError("RequestParameters are null for ReportId: {ReportId}. Cannot initiate AI analysis.", ((ReportGenerationSagaData)context.Workflow.Data).ReportId);
                // Update saga data status or throw to let workflow handle failure
                ((ReportGenerationSagaData)context.Workflow.Data).CurrentStatus = "Failed_AiAnalysis_MissingParams";
                ((ReportGenerationSagaData)context.Workflow.Data).FailureReason = "RequestParameters were not provided.";
                return ExecutionResult.Outcome("Error"); // Or throw specific exception
            }

            try
            {
                // Assuming ReportGenerationSagaInput can be directly used or mapped to AiService's expected input.
                // The SDS mentions IAiServiceClient.InitiateAnalysisAsync(ReportParameters parameters, ...)
                // For simplicity, let's assume ReportGenerationSagaInput is suitable, or AiServiceClient handles mapping.
                // Let's define a placeholder AiServiceRequestDto for IAiServiceClient
                var aiRequest = new AiServiceRequestDto
                {
                    ReportType = ((ReportGenerationSagaData)context.Workflow.Data).RequestParameters.ReportType,
                    Parameters = ((ReportGenerationSagaData)context.Workflow.Data).RequestParameters.Parameters,
                    // Add other necessary fields from RequestParameters
                };

                var analysisResult = await _aiServiceClient.InitiateAnalysisAsync(aiRequest, context.CancellationToken);

                if (analysisResult == null || string.IsNullOrEmpty(analysisResult.ResultUri))
                {
                    _logger.LogError("AI analysis result URI is null or empty for ReportId: {ReportId}.", ((ReportGenerationSagaData)context.Workflow.Data).ReportId);
                    ((ReportGenerationSagaData)context.Workflow.Data).CurrentStatus = "Failed_AiAnalysis_NoResultUri";
                    ((ReportGenerationSagaData)context.Workflow.Data).FailureReason = "AI service did not return a valid result URI.";
                    return ExecutionResult.Outcome("Error");
                }
                
                ((ReportGenerationSagaData)context.Workflow.Data).AiAnalysisResultUri = analysisResult.ResultUri;
                AiAnalysisResultUri = analysisResult.ResultUri; // Output mapping for WorkflowCore if needed for direct step output
                _logger.LogInformation("AI analysis initiated successfully. Result URI: {AiAnalysisResultUri} for ReportId: {ReportId}", AiAnalysisResultUri, ((ReportGenerationSagaData)context.Workflow.Data).ReportId);
                
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error initiating AI analysis for ReportId: {ReportId}. Error: {ErrorMessage}", ((ReportGenerationSagaData)context.Workflow.Data).ReportId, ex.Message);
                ((ReportGenerationSagaData)context.Workflow.Data).FailureReason = $"AI Analysis failed: {ex.Message}";
                // The workflow engine's retry policy will handle this if configured.
                // If not, or if retries are exhausted, the workflow will proceed to compensation if defined.
                throw; // Re-throw to allow WorkflowCore to handle retries/compensation
            }
        }
    }
}