using AIService.Api.Dtos.Nlq;
using AIService.Application.Nlq.Commands; // Assuming this namespace for command
using AIService.Application.Nlq.Models;   // Assuming this namespace for result model
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
    public class NlqController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<NlqController> _logger;

        public NlqController(IMediator mediator, IMapper mapper, ILogger<NlqController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Submits a natural language query for processing.
        /// </summary>
        /// <param name="dto">The NLQ request data.</param>
        /// <returns>The processed NLQ result.</returns>
        /// <response code="200">Returns the processed NLQ result.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("process")]
        [ProducesResponseType(typeof(NlqResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PostProcessNlqAsync([FromBody] NlqRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Query))
            {
                return BadRequest("NLQ request cannot be null and query text must be provided.");
            }

            _logger.LogInformation("Received NLQ request: {Query}", dto.Query);

            try
            {
                var command = _mapper.Map<ProcessNlqCommand>(dto); // Assumes ProcessNlqCommand exists
                var nlqResult = await _mediator.Send(command);

                if (nlqResult == null)
                {
                    _logger.LogWarning("NLQ processing result was null for query: {Query}", dto.Query);
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to process NLQ query.");
                }
                
                var responseDto = _mapper.Map<NlqResponseDto>(nlqResult); // Assumes NlqProcessingResult maps to NlqResponseDto
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing NLQ request: {Query}", dto.Query);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}