using AIService.Api.Dtos.AnomalyDetection;
// Assuming AIService.Application.AnomalyDetection.Commands.DetectAnomaliesCommand exists
// using AIService.Application.AnomalyDetection.Commands;
// Assuming AIService.Application.AnomalyDetection.Models.AnomalyDetectionOutput exists
// using AIService.Application.AnomalyDetection.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AIService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnomalyDetectionController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper; // IMapper might be used if a profile is available
        private readonly ILogger<AnomalyDetectionController> _logger;

        public AnomalyDetectionController(
            IMediator mediator,
            IMapper mapper,
            ILogger<AnomalyDetectionController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); // Even if not used directly, it's a common dependency
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits data to detect anomalies.
        /// REQ-7-008
        /// </summary>
        /// <param name="requestDto">The anomaly detection request data.</param>
        /// <returns>The anomaly detection results.</returns>
        [HttpPost("detect")]
        [ProducesResponseType(typeof(AnomalyDetectionResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostAnomalyDetectionAsync([FromBody] AnomalyDetectionRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request DTO cannot be null.");
            }

            try
            {
                // Assuming DetectAnomaliesCommand structure
                var command = new AIService.Application.AnomalyDetection.Commands.DetectAnomaliesCommand
                {
                    ModelId = requestDto.ModelId,
                    DataPoints = requestDto.DataPoints
                };

                // Assuming the handler returns AnomalyDetectionOutput which is mapped or is AnomalyDetectionResponseDto
                var result = await _mediator.Send(command);

                if (result == null)
                {
                     _logger.LogWarning("Anomaly detection returned null result for input: {@RequestDto}", requestDto);
                    return NotFound("Anomaly detection could not be performed or no anomalies found with current configuration.");
                }

                // If AnomalyDetectionProfile is not defined, this mapping might fail or be an identity mapping.
                // For this exercise, assuming the result from MediatR is directly usable or a profile exists.
                // If `result` is of type AnomalyDetectionOutput, and a mapper exists:
                // var responseDto = _mapper.Map<AnomalyDetectionResponseDto>(result);
                // If `result` is already AnomalyDetectionResponseDto or very similar:
                var responseDto = result as AnomalyDetectionResponseDto; 
                if (responseDto == null && result is AIService.Application.AnomalyDetection.Models.AnomalyDetectionOutput output)
                {
                    // Manual mapping if no AutoMapper profile for this specific DTO
                    responseDto = new AnomalyDetectionResponseDto
                    {
                        Anomalies = output.Anomalies?.ConvertAll(a => new AnomalyDto
                        {
                            Timestamp = a.Timestamp,
                            Severity = a.Severity,
                            Description = a.Description,
                            AffectedFeatures = a.AffectedFeatures,
                            Confidence = a.Confidence,
                            RawScore = a.RawScore
                        }) ?? new System.Collections.Generic.List<AnomalyDto>(),
                        ModelIdUsed = output.ModelIdUsed,
                        ExecutionId = output.ExecutionId
                    };
                } else if (responseDto == null) {
                     _logger.LogError("Result from Anomaly Detection handler is not of expected type or null.");
                    return StatusCode(500, "An error occurred due to unexpected result format from anomaly detection service.");
                }


                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing anomaly detection request.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}

// Placeholder for Application layer command and model (not generated in this step)
namespace AIService.Application.AnomalyDetection.Commands
{
    public class DetectAnomaliesCommand : IRequest<AIService.Application.AnomalyDetection.Models.AnomalyDetectionOutput> // Or IRequest<AnomalyDetectionResponseDto>
    {
        public string ModelId { get; set; }
        public System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> DataPoints { get; set; }
    }
}

namespace AIService.Application.AnomalyDetection.Models
{
    public class AnomalyDetectionOutput // This could be directly AnomalyDetectionResponseDto or a separate model
    {
        public System.Collections.Generic.List<AnomalyOutput> Anomalies { get; set; }
        public string ModelIdUsed { get; set; }
        public string ExecutionId { get; set; }
        public AnomalyDetectionOutput()
        {
            Anomalies = new System.Collections.Generic.List<AnomalyOutput>();
        }
    }

    public class AnomalyOutput
    {
        public System.DateTime Timestamp { get; set; }
        public int Severity { get; set; }
        public string Description { get; set; }
        public System.Collections.Generic.List<string> AffectedFeatures { get; set; }
        public double Confidence { get; set; }
        public double RawScore {get; set;}
    }
}