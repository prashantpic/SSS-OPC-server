using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.Management.Application.ClientInstances.Commands.ExecuteBulkUpdate;
using Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration;

namespace Opc.System.Services.Management.Api.Controllers
{
    /// <summary>
    /// Exposes an endpoint for applying a configuration template to multiple client instances in a single API call.
    /// </summary>
    [ApiController]
    [Route("api/bulk")]
    public class BulkOperationsController : ControllerBase
    {
        private readonly ISender _sender;

        public BulkOperationsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Applies a single configuration to multiple clients.
        /// </summary>
        /// <param name="request">The request containing the list of client IDs and the configuration to apply.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A result detailing which updates were successful and which failed.</returns>
        [HttpPost("update-configuration")]
        [ProducesResponseType(typeof(BulkUpdateResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApplyBulkConfiguration([FromBody] BulkUpdateRequest request, CancellationToken cancellationToken)
        {
            var command = new ExecuteBulkUpdateCommand(request.ClientInstanceIds, request.ConfigurationToApply);
            var result = await _sender.Send(command, cancellationToken);
            return Ok(result);
        }
    }
    
    /// <summary>
    /// The request body for a bulk update operation.
    /// </summary>
    public record BulkUpdateRequest(
        IEnumerable<Guid> ClientInstanceIds,
        ClientConfigurationDto ConfigurationToApply);
}