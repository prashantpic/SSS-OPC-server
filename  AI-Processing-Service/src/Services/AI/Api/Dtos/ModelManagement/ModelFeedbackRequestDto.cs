using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    public class ModelFeedbackRequestDto
    {
        [Required]
        public string ModelId { get; set; }

        [Required]
        public string PredictionId { get; set; } // Identifier for the specific prediction being commented on

        [Required]
        public bool IsCorrect { get; set; } // Was the prediction correct?

        // Optional: if the prediction was incorrect, what should it have been?
        // Structure can be similar to PredictionResponseDto.OutputData
        public Dictionary<string, object> CorrectedOutput { get; set; } 

        [StringLength(1000)]
        public string Comments { get; set; }

        public string UserId { get; set; } // Optional: ID of the user providing feedback

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}