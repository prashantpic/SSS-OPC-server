using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchestrationService.Api.Dtos;
using OrchestrationService.Workflows.BlockchainSync; // For BlockchainSyncSaga
using OrchestrationService.Workflows.BlockchainSync.Models; // For BlockchainSyncSagaInput, BlockchainSyncSagaData
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace OrchestrationService.Api.Controllers
{
    /// <summary>
    /// API controller for managing blockchain synchronization workflows.
    /// Provides HTTP endpoints to initiate, monitor status of, or manage blockchain synchronization sagas.
    /// Corresponds to REQ-8-007.
    /// </summary>
    [ApiController]
    [Route("api/workflows/blockchain")]
    public class BlockchainWorkflowsController : ControllerBase
    {
        private readonly IWorkflowHost _workflowHost;
        private readonly ILogger<BlockchainWorkflowsController> _logger;

        public BlockchainWorkflowsController(IWorkflowHost workflowHost, ILogger<BlockchainWorkflowsController> logger)
        {
            _workflowHost = workflowHost ?? throw new ArgumentNullException(nameof(workflowHost));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Starts a new blockchain synchronization workflow.
        /// </summary>
        /// <param name="request">The request containing workflow type and input data for blockchain sync.</param>
        /// <returns>Status of the newly created workflow instance.</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(WorkflowStatusResponseDto), 202)] // Accepted
        [ProducesResponseType(400)] // Bad Request
        [ProducesResponseType(500)] // Internal Server Error
        public async Task<IActionResult> StartBlockchainSyncWorkflow([FromBody] StartWorkflowRequestDto<BlockchainSyncSagaInput> request)
        {
            if (request == null || request.InputData == null)
            {
                _logger.LogWarning("StartBlockchainSyncWorkflow called with null request or input data.");
                return BadRequest("Request and InputData cannot be null.");
            }

            // if (request.WorkflowType != "BlockchainSyncSaga") return BadRequest("Invalid WorkflowType for this controller.");

            try
            {
                var sagaData = new BlockchainSyncSagaData
                {
                    DataId = Guid.NewGuid().ToString(), // Generate a unique ID for this sync operation
                    InputDataRef = request.InputData,
                    CurrentStatus = "Initiated",
                    FailureReason = null,
                    CompensatedSteps = new System.Collections.Generic.List<string>()
                };

                var workflowId = await _workflowHost.StartWorkflow(nameof(BlockchainSyncSaga), sagaData);
                _logger.LogInformation("Started BlockchainSyncSaga with WorkflowId: {WorkflowId}, DataId: {DataId}", workflowId, sagaData.DataId);
                
                var responseDto = new WorkflowStatusResponseDto
                {
                    WorkflowId = workflowId,
                    Status = sagaData.CurrentStatus,
                    CurrentStep = "Start",
                    LastEventTime = DateTime.UtcNow,
                    WorkflowData = sagaData,
                    FailureReason = null
                };

                return Accepted(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting BlockchainSyncSaga. Input: {@InputData}", request.InputData);
                return StatusCode(500, $"An error occurred while starting the blockchain sync workflow: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the status of a specific blockchain synchronization workflow instance.
        /// </summary>
        /// <param name="workflowId">The ID of the workflow instance.</param>
        /// <returns>The current status and data of the workflow instance.</returns>
        [HttpGet("{workflowId}/status")]
        [ProducesResponseType(typeof(WorkflowStatusResponseDto), 200)]
        [ProducesResponseType(404)] // Not Found
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBlockchainSyncWorkflowStatus(string workflowId)
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
                    _logger.LogWarning("Blockchain sync workflow instance not found for WorkflowId: {WorkflowId}", workflowId);
                    return NotFound($"Workflow instance with ID '{workflowId}' not found.");
                }

                var sagaData = instance.Data as BlockchainSyncSagaData;
                var responseDto = new WorkflowStatusResponseDto
                {
                    WorkflowId = instance.Id,
                    Status = sagaData?.CurrentStatus ?? instance.Status.ToString(),
                    CurrentStep = instance.ExecutionPointers.FirstOrDefault(p => p.Active && !string.IsNullOrEmpty(p.StepName))?.StepName ?? "Unknown",
                    WorkflowData = sagaData,
                    FailureReason = sagaData?.FailureReason
                };

                if (instance.CompleteTime.HasValue) responseDto.LastEventTime = instance.CompleteTime.Value;
                else if (instance.Status == WorkflowCore.Models.WorkflowStatus.Terminated) responseDto.LastEventTime = DateTime.UtcNow;
                else if (instance.Status == WorkflowCore.Models.WorkflowStatus.Suspended) responseDto.LastEventTime = DateTime.UtcNow;
                else responseDto.LastEventTime = instance.CreateTime;

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status for Blockchain Sync WorkflowId: {WorkflowId}", workflowId);
                return StatusCode(500, $"An error occurred while retrieving workflow status: {ex.Message}");
            }
        }
    }
}