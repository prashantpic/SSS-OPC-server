using Microsoft.AspNetCore.Mvc;
using AIService.Api.Dtos.ModelManagement;
using AIService.Application.Interfaces; // Assuming IModelManagementAppService is here
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MediatR;
using AIService.Application.ModelManagement.Commands; // Assuming RegisterModelFeedbackCommand is here

namespace AIService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/models")]
    public class ModelManagementController : ControllerBase
    {
        private readonly IModelManagementAppService _modelManagementAppService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ModelManagementController> _logger;

        public ModelManagementController(
            IModelManagementAppService modelManagementAppService,
            IMediator mediator,
            IMapper mapper,
            ILogger<ModelManagementController> logger)
        {
            _modelManagementAppService = modelManagementAppService;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a custom AI model.
        /// REQ-7-004: MLOps Integration (partially, via AppService)
        /// REQ-7-005: Model Feedback Loop (related to model lifecycle)
        /// </summary>
        /// <param name="dto">The model upload request data.</param>
        /// <returns>Status of the upload operation.</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostModelUploadAsync([FromForm] ModelUploadRequestDto dto)
        {
            if (!ModelState.IsValid || dto.ModelFile == null || dto.ModelFile.Length == 0)
            {
                _logger.LogWarning("Invalid model upload request.");
                return BadRequest("Invalid model upload request. Ensure all fields are provided and the file is not empty.");
            }
            
            _logger.LogInformation("Received model upload request for model: {ModelName} version {ModelVersion}", dto.ModelName, dto.ModelVersion);

            // In a real app, IModelManagementAppService.UploadModelAsync would take more structured input
            // possibly mapped from the DTO or the DTO itself.
            // For now, this is a simplified call.
            // The actual implementation for UploadModelAsync should be in the Application layer.
            // public async Task<string> UploadModelAsync(string modelName, stringmodelVersion, string modelType, string modelFormat, string description, Stream modelStream, string fileName)

            using var modelStream = dto.ModelFile.OpenReadStream();
            var modelId = await _modelManagementAppService.UploadModelAsync(
                dto.ModelName,
                dto.ModelVersion,
                dto.ModelType,
                dto.ModelFormat,
                dto.Description,
                modelStream,
                dto.ModelFile.FileName);

            if (string.IsNullOrEmpty(modelId))
            {
                 _logger.LogError("Model upload failed for model: {ModelName} version {ModelVersion}", dto.ModelName, dto.ModelVersion);
                return StatusCode(500, new { message = "Model upload failed." });
            }
            
            _logger.LogInformation("Model {ModelName} version {ModelVersion} uploaded successfully with ID: {ModelId}", dto.ModelName, dto.ModelVersion, modelId);
            return CreatedAtAction(nameof(GetModelStatusAsync), new { modelId = modelId }, new { modelId = modelId, message = "Model uploaded successfully." });
        }

        /// <summary>
        /// Submits feedback for an AI model's prediction.
        /// REQ-7-005: Model Feedback Loop
        /// REQ-7-011: Anomaly Labeling (covered by feedback DTO)
        /// </summary>
        /// <param name="dto">The model feedback request data.</param>
        /// <returns>Status of the feedback submission.</returns>
        [HttpPost("feedback")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostModelFeedbackAsync([FromBody] ModelFeedbackRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model feedback request.");
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Received model feedback for model ID: {ModelId}, Prediction ID: {PredictionId}", dto.ModelId, dto.PredictionId);

            var command = _mapper.Map<RegisterModelFeedbackCommand>(dto);
            await _mediator.Send(command); // Fire-and-forget or handle result
            
            _logger.LogInformation("Model feedback for model ID: {ModelId} processed.", dto.ModelId);
            return Accepted(new { message = "Feedback received and is being processed." });
        }

        /// <summary>
        /// Gets the status of a specific AI model. (Placeholder for a GET endpoint)
        /// REQ-7-010: AI Model Performance Monitoring (status is part of this)
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <returns>The model status.</returns>
        [HttpGet("{modelId}/status")]
        [ProducesResponseType(typeof(object), 200)] // Replace object with a ModelStatusDto
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetModelStatusAsync(string modelId)
        {
             _logger.LogInformation("Requesting status for model ID: {ModelId}", modelId);
            // var status = await _modelManagementAppService.GetModelStatusAsync(modelId);
            // Placeholder logic
            await Task.Delay(50);
            var status = new { ModelId = modelId, Status = "Ready", LastUpdated = System.DateTimeOffset.UtcNow };

            if (status == null)
            {
                _logger.LogWarning("Model with ID {ModelId} not found for status check.", modelId);
                return NotFound(new { message = $"Model with ID {modelId} not found." });
            }
            _logger.LogInformation("Status for model ID {ModelId} is {Status}", modelId, status.Status);
            return Ok(status);
        }
    }
}