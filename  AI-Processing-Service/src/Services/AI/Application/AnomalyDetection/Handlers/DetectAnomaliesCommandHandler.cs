using MediatR;
using AutoMapper;
using AIService.Application.AnomalyDetection.Commands;
using AIService.Application.AnomalyDetection.Models;
using AIService.Domain.Services;
using AIService.Domain.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // For List<AnomalyDetail>

namespace AIService.Application.AnomalyDetection.Handlers
{
    /// <summary>
    /// Processes the DetectAnomaliesCommand, executes the relevant anomaly detection model,
    /// and returns results.
    /// REQ-7-008: Anomaly Detection
    /// </summary>
    public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, AnomalyDetectionResult>
    {
        private readonly IModelExecutionService _modelExecutionService;
        private readonly IMapper _mapper;
        private readonly ILogger<DetectAnomaliesCommandHandler> _logger;

        public DetectAnomaliesCommandHandler(
            IModelExecutionService modelExecutionService,
            IMapper mapper,
            ILogger<DetectAnomaliesCommandHandler> logger)
        {
            _modelExecutionService = modelExecutionService ?? throw new ArgumentNullException(nameof(modelExecutionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AnomalyDetectionResult> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DetectAnomaliesCommand for ModelId: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion);

            var modelInput = new ModelInput { Features = request.InputData };

            try
            {
                ModelOutput domainOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, request.ModelVersion, modelInput);
                
                // Mapping from generic ModelOutput to specific AnomalyDetectionResult
                // This mapping might be complex and involve interpreting the ModelOutput.Results
                AnomalyDetectionResult applicationResult = _mapper.Map<AnomalyDetectionResult>(domainOutput);
                applicationResult.Success = true;

                _logger.LogInformation("Successfully detected anomalies for ModelId: {ModelId}", request.ModelId);
                return applicationResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalies for ModelId: {ModelId}", request.ModelId);
                return new AnomalyDetectionResult
                {
                    Success = false,
                    Message = $"Failed to detect anomalies: {ex.Message}",
                    Anomalies = new List<AnomalyDetail>() // Empty list on failure
                };
            }
        }
    }

    // Define placeholder for AnomalyDetectionResult and AnomalyDetail if not already defined elsewhere
    // These would typically be in AIService.Application.AnomalyDetection.Models
    namespace AIService.Application.AnomalyDetection.Models
    {
        public class AnomalyDetectionResult
        {
            public bool Success { get; set; }
            public List<AnomalyDetail> Anomalies { get; set; }
            public string Message { get; set; }

            public AnomalyDetectionResult()
            {
                Anomalies = new List<AnomalyDetail>();
            }
        }

        public class AnomalyDetail
        {
            public string Description { get; set; }
            public double Score { get; set; }
            public DateTime Timestamp { get; set; } // Or relevant identifier
            public Dictionary<string, object> Details { get; set; }
            public AnomalyDetail()
            {
                Details = new Dictionary<string, object>();
            }
        }
    }
}