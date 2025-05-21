using System.Collections.Generic;
using System;

namespace OrchestrationService.Workflows.ReportGeneration;

/// <summary>
/// Represents the stateful aggregate for an instance of the ReportGenerationSaga.
/// Holds all persistent data, parameters, intermediate results, and current state
/// related to a specific report generation process.
/// </summary>
public class ReportGenerationSagaData
{
    /// <summary>
    /// Unique identifier for the report being generated.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Original input parameters that started this saga instance.
    /// </summary>
    public ReportGenerationSagaInput RequestParameters { get; set; } = new ReportGenerationSagaInput();

    /// <summary>
    /// Current status of the report generation saga.
    /// Examples: "Initiated", "AiAnalysisInProgress", "DataRetrievalComplete", "DocumentGenerated", "ValidationPending", "Distributed", "Archived", "Failed", "Compensating".
    /// </summary>
    public string CurrentStatus { get; set; } = "Not Started";

    /// <summary>
    /// URI or reference to the AI analysis results.
    /// </summary>
    public string? AiAnalysisResultUri { get; set; }

    /// <summary>
    /// URI or reference to the retrieved historical data.
    /// </summary>
    public string? HistoricalDataReference { get; set; }

    /// <summary>
    /// URI or path to the generated report document.
    /// </summary>
    public string? GeneratedDocumentUri { get; set; }

    /// <summary>
    /// List of recipients or locations for distribution (can be updated by DistributeReportActivity).
    /// </summary>
    public List<string>? DistributionList { get; set; }

    /// <summary>
    /// Status of the validation step (e.g., "Pending", "Approved", "Rejected").
    /// </summary>
    public string? ValidationStatus { get; set; }

    /// <summary>
    /// Reference or ID for the archived report version.
    /// </summary>
    public string? ArchiveReference { get; set; }

    /// <summary>
    /// Stores error details if the saga fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Log of compensation actions taken (optional, for diagnostics).
    /// </summary>
    public List<string> CompensationLog { get; set; } = new List<string>();
}