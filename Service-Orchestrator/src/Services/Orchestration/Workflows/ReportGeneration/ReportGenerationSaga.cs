using WorkflowCore.Interface;
using OrchestrationService.Workflows.ReportGeneration.Activities;

namespace OrchestrationService.Workflows.ReportGeneration;

/// <summary>
/// Defines the Saga (long-running process) for generating AI-driven reports.
/// It orchestrates multiple steps including AI analysis, data retrieval, document creation,
/// distribution, validation, versioning, and archiving, ensuring consistency across distributed services.
/// </summary>
public class ReportGenerationSaga : IWorkflow<ReportGenerationSagaData>
{
    public string Id => "ReportGenerationSaga"; // Unique ID for this workflow definition
    public int Version => 1; // Version of the workflow definition

    /// <summary>
    /// Builds the workflow definition.
    /// </summary>
    /// <param name="builder">The workflow builder instance.</param>
    public void Build(IWorkflowBuilder<ReportGenerationSagaData> builder)
    {
        builder
            .StartWith(context =>
            {
                context.Workflow.Data.CurrentStatus = "Initiated";
                Console.WriteLine($"Report Generation Saga {context.Workflow.Id} (ReportId: {context.Workflow.Data.ReportId}) initiated.");
                return ExecutionResult.Next();
            })
            .Then<InitiateAiAnalysisActivity>()
                .Input(step => step.Input = step.Workflow.Data.RequestParameters) // Pass relevant input from saga data
                .Output(step => step.Workflow.Data.AiAnalysisResultUri = step.Output) // Store output in saga data
                .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10)) // Retry on error
                .CompensateWith<CompensateReportGenerationActivity>() // Specify compensation step
            .Then<RetrieveHistoricalDataActivity>()
                .Input(step => step.Input = step.Workflow.Data.RequestParameters.DataFilters)
                .Output(step => step.Workflow.Data.HistoricalDataReference = step.Output)
                .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10))
                .CompensateWith<CompensateReportGenerationActivity>()
            .Then<GenerateReportDocumentActivity>()
                .Input(step => new GenerateReportDocumentActivity.ActivityInput
                {
                    ReportParameters = step.Workflow.Data.RequestParameters,
                    AiAnalysisResultUri = step.Workflow.Data.AiAnalysisResultUri,
                    HistoricalDataReference = step.Workflow.Data.HistoricalDataReference
                })
                .Output(step => step.Workflow.Data.GeneratedDocumentUri = step.Output)
                .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(15))
                .CompensateWith<CompensateReportGenerationActivity>()
            // Optional Validation Step
            .If(data => data.RequestParameters.RequiresValidation) // REQ-7-022: Conditional validation
                .Do(then => then
                    .Then<ReportValidationStepActivity>() // This activity would interact with the validation process
                        .Input(step => new ReportValidationStepActivity.ActivityInput { ReportUri = step.Workflow.Data.GeneratedDocumentUri, WorkflowInstanceId = step.Workflow.Id })
                        .Output(step => step.Workflow.Data.ValidationStatus = step.Output) // Output of validation
                        .WaitFor("ReportValidatedEvent", (data, context) => context.Workflow.Id, data => DateTime.UtcNow.AddHours(1)) // REQ-7-022: timeout
                            .Output( (step, data) => data.ValidationStatus = (string)step.EventData) // Update based on event
                        .Then(context => {
                            if (context.Workflow.Data.ValidationStatus != "Approved")
                            {
                                Console.WriteLine($"Report validation not approved for {context.Workflow.Id}. Status: {context.Workflow.Data.ValidationStatus}");
                                context.Workflow.Data.ErrorMessage = $"Validation failed: {context.Workflow.Data.ValidationStatus}";
                                // This will trigger compensation for preceding steps if needed or handle error
                                return ExecutionResult.Outcome(new ReportValidationFailedOutcome());
                            }
                            Console.WriteLine($"Report validation approved for {context.Workflow.Id}.");
                            return ExecutionResult.Next();
                        })
                )
            .Then<DistributeReportActivity>() // REQ-7-020 ReportDistributionSteps
                 .Input(step => new DistributeReportActivity.ActivityInput
                 {
                     ReportUri = step.Workflow.Data.GeneratedDocumentUri,
                     DistributionList = step.Workflow.Data.RequestParameters.TargetRecipients
                 })
                 .Output(step => step.Workflow.Data.DistributionList = step.Output) // Actual list after distribution
                 .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10))
                 .CompensateWith<CompensateReportGenerationActivity>()
            .Then<ArchiveReportActivity>() // REQ-7-022 ReportArchivingSteps
                 .Input(step => new ArchiveReportActivity.ActivityInput
                 {
                     ReportUri = step.Workflow.Data.GeneratedDocumentUri,
                     ReportMetadata = new ReportMetadataDto
                     {
                         ReportId = step.Workflow.Data.ReportId,
                         ReportType = step.Workflow.Data.RequestParameters.ReportType,
                         GeneratedTime = DateTimeOffset.UtcNow, // Or from workflow context
                         Version = 1 // Example versioning REQ-7-022
                     }
                 })
                 .Output(step => step.Workflow.Data.ArchiveReference = step.Output)
                 .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10))
                 // No direct compensation for archive, but prior steps can be compensated if archive fails.
            .Then(context =>
            {
                context.Workflow.Data.CurrentStatus = "Completed";
                Console.WriteLine($"Report Generation Saga {context.Workflow.Id} (ReportId: {context.Workflow.Data.ReportId}) completed successfully.");
                return ExecutionResult.Next();
            })
            .OnError(WorkflowCore.Models.WorkflowErrorHandling.Compensate); // Global error handling: trigger compensation
    }
}

// Custom outcome for validation failure
public class ReportValidationFailedOutcome { }