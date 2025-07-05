using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using LicensingService.Application.Features.Licenses.Commands.ActivateLicense;
using LicensingService.Application.Features.Licenses.Queries.ValidateLicense;

#region Placeholder DTOs and Commands
// These types would normally be in their own files in the Application layer.
// They are defined here to make the controller compilable in this generation batch,
// assuming their real files exist elsewhere.

namespace LicensingService.Application.Features.Licenses.Queries.CheckFeatureEntitlement
{
    public record FeatureEntitlementDto(string FeatureCode, bool IsEnabled);
    public record CheckFeatureEntitlementQuery(string LicenseKey, string FeatureCode) : IRequest<FeatureEntitlementDto>;
}

namespace LicensingService.Api.Models.Requests
{
    public record OfflineActivationRequestDto(string LicenseKey, string MachineId);
    public record CompleteOfflineActivationRequestDto(string SignedVendorResponse);
}

namespace LicensingService.Application.Features.Licenses.Commands.OfflineActivation
{
    using LicensingService.Api.Models.Requests;
    public record GenerateOfflineActivationRequestCommand(string LicenseKey, string MachineId) : IRequest<string>;
    public record CompleteOfflineActivationCommand(string SignedVendorResponse) : IRequest;
}
#endregion

namespace LicensingService.Api.Controllers
{
    /// <summary>
    /// Exposes public-facing REST endpoints for license activation and validation.
    /// </summary>
    [ApiController]
    [Route("api/v1/licenses")]
    public class LicensesController : ControllerBase
    {
        private readonly ISender _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensesController"/> class.
        /// </summary>
        /// <param name="sender">The MediatR sender interface.</param>
        public LicensesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Activates a license using online activation.
        /// </summary>
        /// <param name="request">The activation request containing the license key and metadata.</param>
        /// <returns>A status of 200 OK on success.</returns>
        [HttpPost("activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateOnline([FromBody] ActivateLicenseCommand request)
        {
            await _sender.Send(request);
            return Ok();
        }

        /// <summary>
        /// Validates a given license key.
        /// </summary>
        /// <param name="licenseKey">The license key to validate.</param>
        /// <returns>A DTO with the validation result.</returns>
        [HttpGet("{licenseKey}/validate")]
        [ProducesResponseType(typeof(LicenseValidationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Validate(string licenseKey)
        {
            var result = await _sender.Send(new ValidateLicenseQuery(licenseKey));
            return Ok(result);
        }

        /// <summary>
        /// Checks if a specific feature is enabled for a given license key.
        /// </summary>
        /// <param name="licenseKey">The license key to check.</param>
        /// <param name="featureCode">The unique code for the feature.</param>
        /// <returns>A DTO with the feature entitlement status.</returns>
        [HttpGet("{licenseKey}/features/{featureCode}")]
        [ProducesResponseType(typeof(Application.Features.Licenses.Queries.CheckFeatureEntitlement.FeatureEntitlementDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckFeatureEntitlement(string licenseKey, string featureCode)
        {
            var query = new Application.Features.Licenses.Queries.CheckFeatureEntitlement.CheckFeatureEntitlementQuery(licenseKey, featureCode);
            var result = await _sender.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Generates a request file for offline activation.
        /// </summary>
        /// <param name="request">The offline activation request details.</param>
        /// <returns>The signed request file content as a string.</returns>
        [HttpPost("offline/request")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateOfflineActivationRequest([FromBody] Models.Requests.OfflineActivationRequestDto request)
        {
            var command = new Application.Features.Licenses.Commands.OfflineActivation.GenerateOfflineActivationRequestCommand(request.LicenseKey, request.MachineId);
            var result = await _sender.Send(command);
            return Ok(result);
        }
        
        /// <summary>
        /// Completes the offline activation process using a signed response file.
        /// </summary>
        /// <param name="request">The signed vendor response file content.</param>
        /// <returns>A status of 200 OK on success.</returns>
        [HttpPost("offline/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteOfflineActivation([FromBody] Models.Requests.CompleteOfflineActivationRequestDto request)
        {
            var command = new Application.Features.Licenses.Commands.OfflineActivation.CompleteOfflineActivationCommand(request.SignedVendorResponse);
            await _sender.Send(command);
            return Ok();
        }
    }
}