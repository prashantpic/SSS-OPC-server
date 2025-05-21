using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchestrationService.Api.Dtos;
using OrchestrationService.Workflows.ReportGeneration; // For ReportGenerationSaga
using OrchestrationService.Workflows.ReportGeneration.Models; // For ReportGenerationSagaInput, ReportGenerationSagaData
using OrchestrationService.Workflows.ReportGeneration.Activities; // For ReportValidatedEventPayload
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace OrchestrationService.Api.Controllers
{
    /// <summary>
    /// API controller for managing report generation workflows.
    /// Provides endpoints to trigger, monitor status of, or manage report generation sagas.
    /// Corresponds to REQ-7-020.
    /// </summary>
    [ApiController]
    [Route("api/workflows/report")]
    public class ReportWorkflowsController : ControllerBase
    {
        private readonly IWorkflowHost _workflowHost;
        private readonly ILogger<ReportWorkflowsController> _logger;

        public ReportWorkflowsController(IWorkflowHost workflowHost, ILogger<ReportWorkflowsController> logger)
        {
            _workflowHost = workflowHost ?? throw new ArgumentNullException(nameof(workflowHost));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts a new report generation workflow.
        /// </summary>
        /// <param name="request">The request containing workflow type and input data.</param>
        /// <returns>Status of the newly created workflow instance.</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(WorkflowStatusResponseDto), 202)] // Accepted
        [ProducesResponseType(400)] // Bad Request
        [ProducesResponseType(500)] // Internal Server Error
        public async Task<IActionResult> StartReportWorkflow([FromBody] StartWorkflowRequestDto<ReportGenerationSagaInput> request)
        {
            if (request == null || request.InputData == null)
            {
                _logger.LogWarning("StartReportWorkflow called with null request or input data.");
                return BadRequest("Request and InputData cannot be null.");
            }

            // Ensure WorkflowType matches, though for a specific controller it's implicit.
            // Could add a check: if (request.WorkflowType != "ReportGenerationSaga") return BadRequest(...);

            try
            {
                var sagaData = new ReportGenerationSagaData
                {
                    ReportId = Guid.NewGuid().ToString(), // Generate a unique ID for the report instance
                    RequestParameters = request.InputData,
                    CurrentStatus = "Initiated",
                    FailureReason = null,
                    CompensatedSteps = new System.Collections.Generic.List<string>()
                };

                var workflowId = await _workflowHost.StartWorkflow(nameof(ReportGenerationSaga), sagaData);
                _logger.LogInformation("Started ReportGenerationSaga with WorkflowId: {WorkflowId}, ReportId: {ReportId}", workflowId, sagaData.ReportId);

                var responseDto = new WorkflowStatusResponseDto
                {
                    WorkflowId = workflowId,
                    Status = sagaData.CurrentStatus, // Initial status
                    CurrentStep = "Start", // Or the first actual step name
                    LastEventTime = DateTime.UtcNow,
                    WorkflowData = sagaData, // Or a subset/summary
                    FailureReason = null
                };

                return Accepted(responseDto); // HTTP 202 Accepted is common for async operations
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting ReportGenerationSaga. Input: {@InputData}", request.InputData);
                return StatusCode(500, $"An error occurred while starting the report workflow: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the status of a specific report generation workflow instance.
        /// </summary>
        /// <param name="workflowId">The ID of the workflow instance.</param>
        /// <returns>The current status and data of the workflow instance.</returns>
        [HttpGet("{workflowId}/status")]
        [ProducesResponseType(typeof(WorkflowStatusResponseDto), 200)]
        [ProducesResponseType(404)] // Not Found
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReportWorkflowStatus(string workflowId)
        {
            if (string.IsNullOrWhiteSpace(workflowId))
            {
                return BadRequest("Workflow ID cannot be null or empty.");
            }

            try
            {
                var instance = await _workflowHost.PersistenceStore.GetWorkflowInstance(workflowId, HttpContext.RequestAborted);
                if (instance == null)
                {
                    _logger.LogWarning("Report workflow instance not found for WorkflowId: {WorkflowId}", workflowId);
                    return NotFound($"Workflow instance with ID '{workflowId}' not found.");
                }

                var sagaData = instance.Data as ReportGenerationSagaData;
                var responseDto = new WorkflowStatusResponseDto
                {
                    WorkflowId = instance.Id,
                    Status = sagaData?.CurrentStatus ?? instance.Status.ToString(),
                    CurrentStep = instance.ExecutionPointers.FirstOrDefault(p => p.Active && !string.IsNullOrEmpty(p.StepName))?.StepName ?? "Unknown",
                    LastEventTime = instance.CompleteTime ?? instance.CreateTime, // Simplification
                    WorkflowData = sagaData, // Could be sensitive, decide what to expose
                    FailureReason = sagaData?.FailureReason
                };
                
                // If workflow is complete, use CompleteTime, else LastErrorTime or CreateTime
                if (instance.CompleteTime.HasValue) responseDto.LastEventTime = instance.CompleteTime.Value;
                else if (instance.Status == WorkflowCore.Models.WorkflowStatus.Terminated) responseDto.LastEventTime = DateTime.UtcNow; // No good property for termination time
                else if (instance.Status == WorkflowCore.Models.WorkflowStatus.Suspended) responseDto.LastEventTime = DateTime.UtcNow; // No good property for last active time
                else responseDto.LastEventTime = instance.CreateTime;


                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status for Report WorkflowId: {WorkflowId}", workflowId);
                return StatusCode(500, $"An error occurred while retrieving workflow status: {ex.Message}");
            }
        }


        /// <summary>
        /// Submits a validation outcome for a report awaiting validation.
        /// Publishes an external event that the ReportValidationStepActivity is waiting for.
        /// </summary>
        /// <param name="workflowId">The ID of the workflow instance (used as EventKey).</param>
        /// <param name="validationDto">The validation outcome details.</param>
        /// <returns>Status of the operation.</returns>
        [HttpPost("{workflowId}/validate")]
        [ProducesResponseType(202)] // Accepted
        [ProducesResponseType(400)] // Bad Request
        [ProducesResponseType(404)] // Not Found (if workflow doesn't exist or isn't waiting)
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateReport(string workflowId, [FromBody] ReportValidationDto validationDto)
        {
            if (string.IsNullOrWhiteSpace(workflowId) || validationDto == null)
            {
                return BadRequest("Workflow ID and validation data cannot be null.");
            }

            try
            {
                var instance = await _workflowHost.PersistenceStore.GetWorkflowInstance(workflowId, HttpContext.RequestAborted);
                if (instance == null)
                {
                    _logger.LogWarning("Report workflow instance not found for validation. WorkflowId: {WorkflowId}", workflowId);
                    return NotFound($"Workflow instance with ID '{workflowId}' not found.");
                }

                // Check if the workflow is actually waiting for this event
                // This check is a bit complex with WorkflowCore internals, but a basic status check might help.
                var sagaData = instance.Data as ReportGenerationSagaData;
                if (sagaData == null || sagaData.CurrentStatus != "Validating" || !sagaData.RequestParameters.RequiresValidation) {
                     _logger.LogWarning("Report workflow {WorkflowId} is not in 'Validating' state or does not require validation.", workflowId);
                    return BadRequest($"Workflow {workflowId} is not awaiting validation or does not require it.");
                }


                var eventPayload = new ReportValidatedEventPayload // Defined in ReportValidationStepActivity.cs for now
                {
                    ReportId = sagaData.ReportId, // The event key should match what WaitFor expects
                    ValidationOutcome = validationDto.Outcome,
                    ValidatedBy = validationDto.ValidatedBy ?? User.Identity?.Name ?? "System", // Example
                    ValidationTimestamp = DateTime.UtcNow,
                    Comments = validationDto.Comments
                };

                // The event name must match what ReportValidationStepActivity or the .WaitFor step expects.
                // The event key must match how ReportValidationStepActivity or .WaitFor is keyed.
                // SDS 3.1: "ReportValidationStepActivity: Wait for external event ... ReportValidatedEvent ... for the specific ReportId"
                // So, eventName = "ReportValidatedEvent", eventKey = sagaData.ReportId (which might be different from workflowId)
                // Let's assume the event key is `sagaData.ReportId` as used in `ReportValidationStepActivity`
                await _workflowHost.PublishEvent("ReportValidatedEvent", sagaData.ReportId, eventPayload, DateTime.UtcNow);

                _logger.LogInformation("Published 'ReportValidatedEvent' for WorkflowId: {WorkflowId} (ReportId: {ReportId}), Outcome: {Outcome}", workflowId, sagaData.ReportId, validationDto.Outcome);
                return Accepted($"Validation event published for workflow {workflowId}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing 'ReportValidatedEvent' for WorkflowId: {WorkflowId}", workflowId);
                return StatusCode(500, $"An error occurred while publishing validation event: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// DTO for submitting report validation.
    /// </summary>
    public class ReportValidationDto
    {
        /// <summary>
        /// Validation outcome, e.g., "Approved", "Rejected".
        /// </summary>
        public string Outcome { get; set; }
        public string ValidatedBy { get; set; }
        public string Comments { get; set; }
    }
}