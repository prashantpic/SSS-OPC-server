using AIService.Api.Dtos.AnomalyDetection;
using AIService.Application.AnomalyDetection.Commands; // Assuming this namespace for command
using AIService.Application.AnomalyDetection.Models;   // Assuming this namespace for result model
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Net;

namespace AIService.Api.Controllers
{
    [ApiController]
    [Route("api/ai/[controller]")]
    public class AnomalyDetectionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<AnomalyDetectionController> _logger;

        public AnomalyDetectionController(IMediator mediator, IMapper mapper, ILogger<AnomalyDetectionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits data to detect anomalies.
        /// </summary>
        /// <param name="dto">The anomaly detection request data.</param>
        /// <returns>The anomaly detection results.</returns>
        /// <response code="200">Returns the detected anomalies.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("detect")]
        [ProducesResponseType(typeof(AnomalyDetectionResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostAnomalyDetectionAsync([FromBody] AnomalyDetectionRequestDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Anomaly detection request cannot be null.");
            }

            _logger.LogInformation("Received anomaly detection request for model {ModelId}", dto.ModelId);

            try
            {
                var command = _mapper.Map<DetectAnomaliesCommand>(dto); // Assumes DetectAnomaliesCommand exists
                var detectionResult = await _mediator.Send(command);

                if (detectionResult == null)
                {
                     _logger.LogWarning("Anomaly detection result was null for model {ModelId}", dto.ModelId);
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to get anomaly detection results.");
                }

                var responseDto = _mapper.Map<AnomalyDetectionResponseDto>(detectionResult); // Assumes AnomalyDetectionResult maps to AnomalyDetectionResponseDto
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing anomaly detection request for model {ModelId}", dto.ModelId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}