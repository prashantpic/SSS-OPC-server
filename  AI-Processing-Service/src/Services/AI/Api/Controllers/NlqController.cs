using Microsoft.AspNetCore.Mvc;
using AIService.Api.Dtos.Nlq;
using MediatR;
using AutoMapper;
using System.Threading.Tasks;
using AIService.Application.Nlq.Commands; // Assuming this command exists
using AIService.Application.Nlq.Models;   // Assuming this model exists
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AIService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/nlq")]
    public class NlqController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<NlqController> _logger;

        public NlqController(IMediator mediator, IMapper mapper, ILogger<NlqController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Processes a natural language query.
        /// REQ-7-013: Natural Language Query Processing
        /// REQ-7-014: Integration with NLP Providers (handled by application/domain layer)
        /// </summary>
        /// <param name="requestDto">The NLQ request data.</param>
        /// <returns>The processed NLQ result.</returns>
        [HttpPost("process")]
        [ProducesResponseType(typeof(NlqResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostProcessNlqAsync([FromBody] NlqRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for PostProcessNlqAsync.");
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Received NLQ request: {QueryText}", requestDto.QueryText);

            var command = _mapper.Map<ProcessNlqCommand>(requestDto);

            // Assuming ProcessNlqCommand and NlqProcessingResult exist in the Application layer
            // var nlqProcessingResult = await _mediator.Send(command);

            // Placeholder logic
            await Task.Delay(100); // Simulate async work
            var nlqProcessingResult = new NlqProcessingResult // Placeholder
            {
                OriginalQuery = requestDto.QueryText,
                ProcessedQuery = requestDto.QueryText.ToLower(), // Simplified processing
                Intent = "GetTemperature",
                ConfidenceScore = 0.95,
                ResponseMessage = "Temperature for Pump A is 75°C.",
                Entities = new List<Application.Nlq.Models.NlqEntity> // Placeholder for Application.Nlq.Models.NlqEntity
                {
                    new Application.Nlq.Models.NlqEntity { Type = "Device", Value = "Pump A", RawValue = "Pump A", StartIndex = 20, EndIndex = 25 },
                    new Application.Nlq.Models.NlqEntity { Type = "Measurement", Value = "Temperature", RawValue = "Temperature", StartIndex = 0, EndIndex = 10 }
                }
            };

            if (nlqProcessingResult == null)
            {
                _logger.LogError("NLQ processing result was null for query: {QueryText}", requestDto.QueryText);
                return StatusCode(500, "An error occurred while processing the NLQ request.");
            }

            var responseDto = _mapper.Map<NlqResponseDto>(nlqProcessingResult);
            _logger.LogInformation("NLQ processing successful for query: {QueryText}, Intent: {Intent}", responseDto.OriginalQuery, responseDto.Intent);
            return Ok(responseDto);
        }
    }
}