using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using LicensingService.Domain.Enums;

#region Placeholder DTOs and Commands
// These types would normally be in their own files in the Application layer.
// They are defined here to make the controller compilable in this generation batch,
// assuming their real files exist elsewhere.
namespace LicensingService.Api.Models.Requests
{
    public record GenerateLicenseRequest(Guid CustomerId, LicenseType Type, LicenseTier Tier, DateTime? ExpirationDate);
}

namespace LicensingService.Application.Features.Licenses.Commands.GenerateLicense
{
    public record GenerateLicenseCommand(Guid CustomerId, LicenseType Type, LicenseTier Tier, DateTime? ExpirationDate) : IRequest<string>;
}

namespace LicensingService.Application.Features.Licenses.Commands.RevokeLicense
{
    public record RevokeLicenseCommand(string LicenseKey) : IRequest;
}

namespace LicensingService.Application.Features.Licenses.Queries.GetLicenseDetails
{
    // A simplified DTO for demonstration
    public record LicenseDetailsDto(Guid Id, string Key, LicenseStatus Status, LicenseType Type, LicenseTier Tier, Guid CustomerId, DateTime? ExpirationDate);
    public record GetLicenseDetailsQuery(string LicenseKey) : IRequest<LicenseDetailsDto>;
}
#endregion


namespace LicensingService.Api.Controllers
{
    /// <summary>
    /// Exposes administrative REST endpoints for license generation and management.
    /// Access is restricted to authorized personnel.
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/licenses")]
    //[Authorize(Policy = "AdminOnly")] // This would be enabled in a real scenario
    public class AdminController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="sender">The MediatR sender interface.</param>
        public AdminController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Generates a new software license.
        /// </summary>
        /// <param name="request">The details for the license to be generated.</param>
        /// <returns>The newly generated license key.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateLicense([FromBody] Models.Requests.GenerateLicenseRequest request)
        {
            var command = new Application.Features.Licenses.Commands.GenerateLicense.GenerateLicenseCommand(
                request.CustomerId, request.Type, request.Tier, request.ExpirationDate);
            
            var licenseKey = await _sender.Send(command);
            
            return CreatedAtAction(nameof(GetLicenseDetails), new { licenseKey }, licenseKey);
        }

        /// <summary>
        /// Revokes an existing license.
        /// </summary>
        /// <param name="licenseKey">The license key to revoke.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("{licenseKey}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeLicense(string licenseKey)
        {
            var command = new Application.Features.Licenses.Commands.RevokeLicense.RevokeLicenseCommand(licenseKey);
            await _sender.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Retrieves detailed information for a specific license.
        /// </summary>
        /// <param name="licenseKey">The license key to retrieve details for.</param>
        /// <returns>A DTO with detailed license information.</returns>
        [HttpGet("{licenseKey}")]
        [ProducesResponseType(typeof(Application.Features.Licenses.Queries.GetLicenseDetails.LicenseDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLicenseDetails(string licenseKey)
        {
            var query = new Application.Features.Licenses.Queries.GetLicenseDetails.GetLicenseDetailsQuery(licenseKey);
            var result = await _sender.Send(query);
            return Ok(result);
        }
    }
}