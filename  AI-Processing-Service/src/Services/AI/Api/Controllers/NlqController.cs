using AIService.Api.Dtos.Nlq;
// Assuming AIService.Application.Nlq.Commands.ProcessNlqCommand exists
// using AIService.Application.Nlq.Commands;
// Assuming AIService.Application.Nlq.Models.NlqProcessingResult exists
// using AIService.Application.Nlq.Models;
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
    public class NlqController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<NlqController> _logger;

        public NlqController(
            IMediator mediator,
            IMapper mapper,
            ILogger<NlqController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits a natural language query for processing.
        /// REQ-7-013, REQ-7-014
        /// </summary>
        /// <param name="requestDto">The NLQ request data.</param>
        /// <returns>The processed NLQ result.</returns>
        [HttpPost("process")]
        [ProducesResponseType(typeof(NlqResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PostProcessNlqAsync([FromBody] NlqRequestDto requestDto)
        {
            if (requestDto == null || string.IsNullOrWhiteSpace(requestDto.QueryText))
            {
                return BadRequest("Request DTO cannot be null and QueryText must be provided.");
            }

            try
            {
                // var command = _mapper.Map<ProcessNlqCommand>(requestDto);
                // For now, assuming a placeholder command structure
                 var command = new AIService.Application.Nlq.Commands.ProcessNlqCommand
                {
                    QueryText = requestDto.QueryText,
                    UserId = requestDto.UserId,
                    SessionId = requestDto.SessionId,
                    ContextParameters = requestDto.ContextParameters
                };


                var result = await _mediator.Send(command);

                if (result == null)
                {
                    _logger.LogWarning("NLQ processing returned null result for query: {QueryText}", requestDto.QueryText);
                    return NotFound("NLQ processing failed or no interpretation found.");
                }

                var responseDto = _mapper.Map<NlqResponseDto>(result);
                return Ok(responseDto);
            }
            catch (AutoMapperMappingException ex)
            {
                _logger.LogError(ex, "Mapping error during NLQ processing.");
                return StatusCode(500, "An error occurred due to invalid data mapping during NLQ processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing NLQ request.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}

// Placeholder for Application layer command and model (not generated in this step)
namespace AIService.Application.Nlq.Commands
{
    public class ProcessNlqCommand : IRequest<AIService.Application.Nlq.Models.NlqProcessingResult>
    {
        public string QueryText { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public System.Collections.Generic.Dictionary<string, string> ContextParameters { get; set; }
    }
}

namespace AIService.Application.Nlq.Models
{
    public class NlqProcessingResult
    {
        public string OriginalQuery { get; set; }
        public string ProcessedQuery { get; set; }
        public string Intent { get; set; }
        public System.Collections.Generic.List<NlqEntity> Entities { get; set; }
        public double ConfidenceScore { get; set; }
        public string ResponseMessage { get; set; } // Direct answer if applicable
        public System.Collections.Generic.Dictionary<string, string> AppliedAliases { get; set; }
        public string ProviderUsed { get; set; }
        public bool FallbackApplied { get; set; }

        public NlqProcessingResult()
        {
            Entities = new System.Collections.Generic.List<NlqEntity>();
            AppliedAliases = new System.Collections.Generic.Dictionary<string, string>();
        }
    }

    public class NlqEntity
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public double Confidence { get; set; }
    }
}