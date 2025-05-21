using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.EdgeDeployment
{
    /// <summary>
    /// Data transfer object for specifying model details (ID or reference)
    /// and target edge device(s) for deployment via the REST API.
    /// REQ-8-001
    /// </summary>
    public class EdgeDeploymentRequestDto
    {
        /// <summary>
        /// The ID of the AI model to be deployed.
        /// </summary>
        [Required]
        public string ModelId { get; set; }

        /// <summary>
        /// The specific version of the AI model to deploy.
        /// If not provided, the latest or a default version might be assumed.
        /// </summary>
        [Required]
        public string ModelVersion { get; set; }

        /// <summary>
        /// A list of target edge device IDs where the model should be deployed.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> TargetDeviceIds { get; set; }

        /// <summary>
        /// Optional: Specific configuration parameters for the deployment on the edge devices.
        /// (e.g., runtime settings, resource limits).
        /// </summary>
        public Dictionary<string, string> DeploymentConfiguration { get; set; }
    }
}