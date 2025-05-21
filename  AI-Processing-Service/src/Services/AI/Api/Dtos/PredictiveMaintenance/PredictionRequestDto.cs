using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    public class PredictionRequestDto
    {
        [Required]
        public string ModelId { get; set; }

        [Required]
        public Dictionary<string, object> InputData { get; set; }
    }
}