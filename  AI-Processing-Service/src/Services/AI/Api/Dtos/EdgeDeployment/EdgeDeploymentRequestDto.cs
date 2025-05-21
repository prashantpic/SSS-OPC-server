using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.EdgeDeployment
{
    public class EdgeDeploymentRequestDto
    {
        /// <summary>
        /// The ID of the AI model to be deployed.
        /// REQ-8-001
        /// </summary>
        [Required]
        public string ModelId { get; set; }

        /// <summary>
        /// Optional: Specific version of the model to deploy.
        /// If not provided, the latest suitable or registered version might be used.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// A list of target edge device identifiers where the model should be deployed.
        /// REQ-8-001
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> TargetDeviceIds { get; set; }

        /// <summary>
        /// Optional: Dictionary for any specific configuration parameters required for
        /// deploying or running the model on the edge devices.
        /// E.g., {"HardwareAcceleration": "GPU", "InputTopic": "sensors/camera1"}
        /// </summary>
        public Dictionary<string, string> DeploymentConfiguration { get; set; }

        public EdgeDeploymentRequestDto()
        {
            TargetDeviceIds = new List<string>();
            DeploymentConfiguration = new Dictionary<string, string>();
        }
    }
}