using AIService.Api.Dtos.EdgeDeployment;
using AIService.Application.EdgeDeployment.Commands; // Assuming this namespace for command
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AIService.Api.Controllers
{
    [ApiController]
    [Route("api/ai/[controller]")]
    public class EdgeDeploymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<EdgeDeploymentController> _logger;

        public EdgeDeploymentController(IMediator mediator, IMapper mapper, ILogger<EdgeDeploymentController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates the deployment of an AI model to edge devices.
        /// </summary>
        /// <param name="dto">The edge deployment request data.</param>
        /// <returns>Status of the deployment initiation.</returns>
        /// <response code="202">If the deployment request was accepted for processing.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("deploy")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostDeployModelAsync([FromBody] EdgeDeploymentRequestDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ModelId) || dto.TargetDeviceIds == null || dto.TargetDeviceIds.Count == 0)
            {
                return BadRequest("Edge deployment request is invalid. ModelId and TargetDeviceIds are required.");
            }

            _logger.LogInformation("Received edge deployment request for model {ModelId} to devices: {DeviceIds}", dto.ModelId, string.Join(",", dto.TargetDeviceIds));

            try
            {
                // The SDS implies an IEdgeDeploymentAppService.
                // For consistency with other controllers, using MediatR.
                // The handler for DeployModelToEdgeCommand would then use IEdgeDeploymentAppService.
                var command = _mapper.Map<DeployModelToEdgeCommand>(dto); // Assumes DeployModelToEdgeCommand exists

                // Assuming the command handler returns a deployment ID or task ID
                var deploymentInitiationResult = await _mediator.Send(command); 

                _logger.LogInformation("Edge deployment for model {ModelId} initiated successfully. Task/Deployment ID: {DeploymentId}", dto.ModelId, deploymentInitiationResult);
                return Accepted(deploymentInitiationResult); // Return some identifier for the deployment task
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while initiating edge deployment for model {ModelId}", dto.ModelId);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}