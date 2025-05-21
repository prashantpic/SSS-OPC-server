using System.Collections.Generic;

namespace OrchestrationService.Workflows.ReportGeneration
{
    /// <summary>
    /// Defines the parameters required to initiate a report generation saga,
    /// such as report type, schedule information, triggering event data,
    /// target users/roles for distribution, and any specific data filters.
    /// Implements REQ-7-020.
    /// </summary>
    public class ReportGenerationSagaInput
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new(); // e.g., date range, tags, filters
        public string TriggeringEvent { get; set; } = string.Empty; // API, Schedule, Event
        public string RequestedBy { get; set; } = string.Empty; // User or system initiating
        public string DistributionTarget { get; set; } = string.Empty; // Email list, user group, file path
        public bool RequiresValidation { get; set; } = false;
    }
}