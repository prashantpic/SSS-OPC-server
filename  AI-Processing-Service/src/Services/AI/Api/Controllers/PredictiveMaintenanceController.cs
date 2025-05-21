using AIService.Api.Dtos.PredictiveMaintenance;
// Assuming AIService.Application.PredictiveMaintenance.Commands.GetPredictionCommand exists
// using AIService.Application.PredictiveMaintenance.Commands;
// Assuming AIService.Application.PredictiveMaintenance.Models.PredictionOutput exists
// using AIService.Application.PredictiveMaintenance.Models;
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
    public class PredictiveMaintenanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<PredictiveMaintenanceController> _logger;

        public PredictiveMaintenanceController(
            IMediator mediator,
            IMapper mapper,
            ILogger<PredictiveMaintenanceController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates a maintenance prediction based on input data.
        /// REQ-7-001, REQ-7-003
        /// </summary>
        /// <param name="requestDto">The prediction request data.</param>
        /// <returns>The prediction result.</returns>
        [HttpPost("predict")]
        [ProducesResponseType(typeof(PredictionResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostPredictionAsync([FromBody] PredictionRequestDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request DTO cannot be null.");
            }

            try
            {
                // In a real scenario, GetPredictionCommand would be a defined class
                // var command = _mapper.Map<GetPredictionCommand>(requestDto);
                // For now, assuming a placeholder command structure if GetPredictionCommand is not defined
                var command = new AIService.Application.PredictiveMaintenance.Commands.GetPredictionCommand
                {
                    ModelId = requestDto.ModelId,
                    InputFeatures = requestDto.InputFeatures
                };

                // PredictionOutput would be a defined class returned by the handler
                var result = await _mediator.Send(command);

                if (result == null)
                {
                    _logger.LogWarning("Prediction returned null result for input: {@RequestDto}", requestDto);
                    return NotFound("Prediction could not be generated or no result was found.");
                }
                
                var responseDto = _mapper.Map<PredictionResponseDto>(result);
                return Ok(responseDto);
            }
            catch (AutoMapperMappingException ex)
            {
                _logger.LogError(ex, "Mapping error during prediction processing.");
                // This might indicate a configuration problem or an issue with the result object from mediator
                return StatusCode(500, "An error occurred due to invalid data mapping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing prediction request.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}

// Placeholder for Application layer command and model (not generated in this step)
namespace AIService.Application.PredictiveMaintenance.Commands
{
    public class GetPredictionCommand : IRequest<AIService.Application.PredictiveMaintenance.Models.PredictionOutput>
    {
        public string ModelId { get; set; }
        public System.Collections.Generic.Dictionary<string, object> InputFeatures { get; set; }
    }
}
namespace AIService.Application.PredictiveMaintenance.Models
{
    public class PredictionOutput
    {
        public string PredictionId { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Results { get; set; }
        public string ModelVersionUsed { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}