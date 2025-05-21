using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.EdgeDeployment
{
    public class EdgeDeploymentRequestDto
    {
        [Required]
        public string ModelId { get; set; }

        // Optional: if not provided, a default/latest version might be assumed by the backend
        public string ModelVersion { get; set; } 

        [Required]
        [MinLength(1)]
        public List<string> TargetDeviceIds { get; set; }
    }
}