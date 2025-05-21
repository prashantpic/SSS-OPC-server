using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.Nlq
{
    public class NlqRequestDto
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Query { get; set; }

        public string UserId { get; set; }
        public string SessionId { get; set; }
        public Dictionary<string, string> Context { get; set; } // e.g., "currentAssetId": "pump-123"
    }
}