using OrchestrationService.Workflows.ReportGeneration;

namespace OrchestrationService.Application.Events
{
    /// <summary>
    /// Represents an event (e.g., from a message queue or an internal system trigger)
    /// that signals a request to initiate the report generation saga.
    /// Contains necessary data for starting the workflow.
    /// Implements REQ-7-020.
    /// </summary>
    public class ReportGenerationRequestedEvent
    {
        public ReportGenerationSagaInput Input { get; set; } = new();
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }
    }
}