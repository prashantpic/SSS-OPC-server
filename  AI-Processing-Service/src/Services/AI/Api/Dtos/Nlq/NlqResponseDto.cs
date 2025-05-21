using System.Collections.Generic;

namespace AIService.Api.Dtos.Nlq
{
    public class NlqResponseDto
    {
        public string Intent { get; set; }
        public List<NlqEntityDto> Entities { get; set; }
        public string FormattedResponse { get; set; } // User-friendly response text
        public double Confidence { get; set; } // Overall confidence for the interpretation
        public Dictionary<string, object> DataPayload { get; set; } // If query resulted in data retrieval
    }

    public class NlqEntityDto
    {
        public string Type { get; set; } // e.g., "Device", "Metric", "TimeRange"
        public string Value { get; set; } // e.g., "Pump A", "Temperature", "Last 24 hours"
        public double Confidence { get; set; }
        public Dictionary<string, object> Metadata { get; set; } // Additional details about the entity
    }
}