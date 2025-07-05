using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration;
using Opc.System.Services.Management.Application.ClientInstances.Queries.GetAllClients;
using Opc.System.Services.Management.Application.ClientInstances.Queries.GetClientDetails;
using Opc.System.Services.Management.Application.Shared;

namespace Opc.System.Services.Management.Api.Controllers
{
    /// <summary>
    /// Provides REST endpoints for retrieving client summaries, getting detailed configurations, and updating client settings.
    /// </summary>
    [ApiController]
    [Route("api/client-instances")]
    public class ClientInstancesController : ControllerBase
    {
        private readonly ISender _sender;

        public ClientInstancesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Retrieves a summary list of all managed clients.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClientSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClientSummaries(CancellationToken cancellationToken)
        {
            var query = new GetAllClientsQuery();
            var result = await _sender.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the detailed configuration and status for a specific client.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClientDetails(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetClientDetailsQuery(id);
            var result = await _sender.Send(query, cancellationToken);
            return result is not null ? Ok(result) : NotFound();
        }

        /// <summary>
        /// Updates the configuration for a specific client.
        /// </summary>
        [HttpPut("{id:guid}/configuration")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateConfiguration(Guid id, [FromBody] ClientConfigurationDto request, CancellationToken cancellationToken)
        {
            var command = new UpdateClientConfigurationCommand(id, request);
            Result result = await _sender.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                // This simple check assumes not found is the primary failure mode.
                // A more robust implementation would inspect the result error type.
                return NotFound(result.Error);
            }
            
            return NoContent();
        }
    }
}