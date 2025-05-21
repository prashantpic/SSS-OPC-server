using AIService.Api.Dtos.PredictiveMaintenance;
using AIService.Application.PredictiveMaintenance.Commands; // Assuming this namespace for command
using AIService.Application.PredictiveMaintenance.Models;   // Assuming this namespace for result model
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
    public class PredictiveMaintenanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<PredictiveMaintenanceController> _logger;

        public PredictiveMaintenanceController(IMediator mediator, IMapper mapper, ILogger<PredictiveMaintenanceController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates a maintenance prediction based on input data.
        /// </summary>
        /// <param name="dto">The prediction request data.</param>
        /// <returns>The prediction result.</returns>
        /// <response code="200">Returns the prediction result.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("predict")]
        [ProducesResponseType(typeof(PredictionResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostPredictionAsync([FromBody] PredictionRequestDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Prediction request cannot be null.");
            }

            _logger.LogInformation("Received prediction request for model {ModelId}", dto.ModelId);

            try
            {
                var command = _mapper.Map<GetPredictionCommand>(dto);
                var predictionResult = await _mediator.Send(command);

                if (predictionResult == null)
                {
                    _logger.LogWarning("Prediction result was null for model {ModelId}", dto.ModelId);
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to get prediction.");
                }
                
                var responseDto = _mapper.Map<PredictionResponseDto>(predictionResult);
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing prediction request for model {ModelId}", dto.ModelId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}