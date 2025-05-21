using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AIService.Application.AnomalyDetection.Commands;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For List

namespace AIService.Application.AnomalyDetection.Handlers
{
    /// <summary>
    /// Processes the DetectAnomaliesCommand, executes the relevant anomaly detection model,
    /// and returns results.
    /// REQ-7-008: Anomaly Detection
    /// </summary>
    public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, DetectAnomaliesCommandResult>
    {
        private readonly IModelExecutionService _modelExecutionService;
        private readonly IModelRepository _modelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DetectAnomaliesCommandHandler> _logger;

        public DetectAnomaliesCommandHandler(
            IModelExecutionService modelExecutionService,
            IModelRepository modelRepository,
            IMapper mapper,
            ILogger<DetectAnomaliesCommandHandler> logger)
        {
            _modelExecutionService = modelExecutionService ?? throw new ArgumentNullException(nameof(modelExecutionService));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DetectAnomaliesCommandResult> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DetectAnomaliesCommand for ModelId: {ModelId}, Version: {ModelVersion}", request.ModelId, request.ModelVersion);

            try
            {
                var modelInput = _mapper.Map<ModelInput>(request.InputData);
                 if (modelInput == null)
                {
                    _logger.LogError("Failed to map InputData to ModelInput for Anomaly Detection ModelId: {ModelId}", request.ModelId);
                    return new DetectAnomaliesCommandResult { Success = false, ErrorMessage = "Invalid input data format." };
                }

                var modelOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, request.ModelVersion, modelInput);

                if (modelOutput == null || modelOutput.Outputs == null)
                {
                    _logger.LogWarning("Anomaly detection model execution for ModelId {ModelId} returned null or empty output.", request.ModelId);
                    return new DetectAnomaliesCommandResult { Success = false, ErrorMessage = "Model execution failed or returned no output." };
                }
                
                // The mapping from raw modelOutput.Outputs to DetectAnomaliesCommandResult.Anomalies
                // will be model-specific and should be handled carefully.
                // This might involve interpreting scores, labels, etc.
                // For now, a direct mapping or a simplified interpretation is assumed.
                var result = _mapper.Map<DetectAnomaliesCommandResult>(modelOutput); // AutoMapper profile needs to handle this
                result.Success = true;
                result.RawOutput = modelOutput.Outputs; // Preserve raw output if needed

                // Example: Post-process modelOutput.Outputs to populate result.Anomalies
                // This is highly dependent on the specific AD model's output structure.
                // if (modelOutput.Outputs.TryGetValue("is_anomaly", out var isAnomalyObj) && isAnomalyObj is bool isAnomaly && isAnomaly)
                // {
                //     var anomaly = new DetectedAnomaly
                //     {
                //         AnomalyType = modelOutput.Outputs.TryGetValue("anomaly_type", out var type) ? type.ToString() : "Generic",
                //         SeverityScore = modelOutput.Outputs.TryGetValue("anomaly_score", out var score) && score is double dScore ? dScore : 0.0,
                //         Description = "Anomaly detected based on model output.",
                //         Timestamp = DateTime.UtcNow // Or from input data if available
                //     };
                //     // Populate ContributingFactors if model provides them
                //     result.Anomalies.Add(anomaly);
                // }


                _logger.LogInformation("Successfully processed DetectAnomaliesCommand for ModelId: {ModelId}", request.ModelId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling DetectAnomaliesCommand for ModelId: {ModelId}", request.ModelId);
                return new DetectAnomaliesCommandResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}