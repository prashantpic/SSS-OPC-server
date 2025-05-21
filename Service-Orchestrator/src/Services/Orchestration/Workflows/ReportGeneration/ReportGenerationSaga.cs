using WorkflowCore.Interface;
using WorkflowCore.Models;
using OrchestrationService.Workflows.ReportGeneration.Activities;
using System;

namespace OrchestrationService.Workflows.ReportGeneration
{
    /// <summary>
    /// Defines the Saga (long-running process) for generating AI-driven reports.
    /// It orchestrates multiple steps including AI analysis, data retrieval, document creation,
    /// distribution, validation, versioning, and archiving, ensuring consistency across distributed services.
    /// Implements REQ-7-020, REQ-7-022.
    /// </summary>
    public class ReportGenerationSaga : IWorkflow<ReportGenerationSagaData>
    {
        public string Id => "ReportGenerationSaga";
        public int Version => 1;

        public void Build(IWorkflowBuilder<ReportGenerationSagaData> builder)
        {
            builder
                .StartWith<InitiateAiAnalysisActivity>()
                    .Input(step => step.RequestParameters, data => data.RequestParameters)
                    .Output(data => data.AiAnalysisResultUri, step => step.Output.AiAnalysisResultUri)
                    .Then<RetrieveHistoricalDataActivity>()
                        .Input(step => step.RequestParameters, data => data.RequestParameters)
                        .Input(step => step.AiAnalysisContext, data => data.AiAnalysisResultUri) // Context for potential filtering
                        .Output(data => data.HistoricalDataRef, step => step.Output.HistoricalDataRef)
                    .Then<GenerateReportDocumentActivity>()
                        .Input(step => step.RequestParameters, data => data.RequestParameters)
                        .Input(step => step.AiAnalysisResultUri, data => data.AiAnalysisResultUri)
                        .Input(step => step.HistoricalDataRef, data => data.HistoricalDataRef)
                        .Output(data => data.GeneratedDocumentUri, step => step.Output.GeneratedDocumentUri)
                    .Then<DistributeReportActivity>()
                        .Input(step => step.GeneratedDocumentUri, data => data.GeneratedDocumentUri)
                        .Input(step => step.DistributionTarget, data => data.RequestParameters.DistributionTarget)
                        .Input(step => step.ReportId, data => data.ReportId)
                    .If(data => data.RequestParameters.RequiresValidation)
                        .Do(then => then
                            .Then<ReportValidationStepActivity>()
                                .Input(step => step.ReportId, data => data.ReportId)
                                .Input(step => step.GeneratedDocumentUri, data => data.GeneratedDocumentUri)
                                .Output(data => data.ValidationStatus, step => step.Output.ValidationStatus)
                                .WaitFor("ReportValidatedEvent", (data, context) => context.Workflow.Id, data => DateTime.UtcNow.AddHours(24)) // Example timeout
                                    .Output(data => data.ValidationStatus, step => step.EventData as string) // Assuming event data is the status
                                .Then<ArchiveReportActivity>() // Archive only if validated or validation not required path merges
                                    .Input(step => step.ReportId, data => data.ReportId)
                                    .Input(step => step.GeneratedDocumentUri, data => data.GeneratedDocumentUri)
                                    .Output(data => data.ArchivedReportUri, step => step.Output.ArchivedReportUri)
                        )
                    .Otherwise()
                        .Do(otherwise => otherwise
                            .Then<ArchiveReportActivity>()
                                .Input(step => step.ReportId, data => data.ReportId)
                                .Input(step => step.GeneratedDocumentUri, data => data.GeneratedDocumentUri)
                                .Output(data => data.ArchivedReportUri, step => step.Output.ArchivedReportUri)
                        )
                .EndWorkflow()
                .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromMinutes(5)) // Global retry for transient issues
                .CompensateWith<CompensateReportGenerationActivity>(c =>
                {
                    c.Input(step => step.SagaDataAtFailure, data => data);
                });
        }
    }
}