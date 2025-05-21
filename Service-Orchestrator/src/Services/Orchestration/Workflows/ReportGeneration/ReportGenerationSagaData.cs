using System;
using System.Collections.Generic;

namespace OrchestrationService.Workflows.ReportGeneration
{
    /// <summary>
    /// Represents the stateful aggregate for an instance of the ReportGenerationSaga.
    /// Holds all persistent data, parameters, intermediate results, and current state
    /// related to a specific report generation process.
    /// Implements REQ-7-020, REQ-7-022.
    /// </summary>
    public class ReportGenerationSagaData
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public ReportGenerationSagaInput RequestParameters { get; set; } = new();
        public ReportStatus CurrentStatus { get; set; } = ReportStatus.Initiated;
        public string? AiAnalysisResultUri { get; set; }
        public string? HistoricalDataRef { get; set; }
        public string? GeneratedDocumentUri { get; set; }
        public List<string> DistributionList { get; set; } = new(); // Populated from RequestParameters.DistributionTarget or ManagementService
        public ReportValidationStatus ValidationStatus { get; set; } = ReportValidationStatus.Pending;
        public string? ArchivedReportUri { get; set; }
        public string? FailureReason { get; set; }
        public List<string> CompensatedSteps { get; set; } = new();
    }

    public enum ReportStatus
    {
        Initiated,
        AiAnalysisInProgress,
        AiAnalysisCompleted,
        DataRetrievalInProgress,
        DataRetrievalCompleted,
        GeneratingDocument,
        DocumentGenerated,
        DistributingReport,
        DistributionCompleted,
        PendingValidation,
        ValidationCompleted,
        ArchivingReport,
        ArchivingCompleted,
        Completed,
        Failed,
        Compensating,
        Compensated
    }

    public enum ReportValidationStatus
    {
        NotRequired,
        Pending,
        Approved,
        Rejected,
        TimedOut
    }
}