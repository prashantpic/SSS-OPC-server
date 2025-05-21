using MediatR;
using AIService.Application.AnomalyDetection.Commands;
using AIService.Application.AnomalyDetection.Models;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Application.AnomalyDetection.Handlers
{
    /// <summary>
    /// Processes the DetectAnomaliesCommand, executes the relevant anomaly detection model
    /// via ModelExecutionService, and returns the detected anomalies.
    /// REQ-7-008: Core functionality for anomaly detection.
    /// </summary>
    public class DetectAnomaliesCommandHandler : IRequestHandler<DetectAnomaliesCommand, IEnumerable<AnomalyDetails>>
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

        public async Task<IEnumerable<AnomalyDetails>> Handle(DetectAnomaliesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DetectAnomaliesCommand for ModelId: {ModelId}, Version: {ModelVersion}, DataPoints Count: {DataPointsCount}",
                request.ModelId, request.ModelVersion ?? "Latest", request.DataPoints.Count);

            try
            {
                // Anomaly detection models might take a single complex input or a batch.
                // This example assumes the model can process a list of data points, 
                // or ModelExecutionService handles batching if the underlying model takes one instance at a time.
                // For simplicity, we'll assume ModelInput can represent a batch or sequence if needed.
                // Mapping from List<Dictionary<string, object>> to Domain.ModelInput needs to be defined.
                
                // If model expects one input for all data points:
                var featuresForModel = new Dictionary<string, object> { { "datapoints_sequence", request.DataPoints } };
                var modelInput = _mapper.Map<ModelInput>(featuresForModel); // This mapping needs to be smart
                
                ModelOutput modelOutput = await _modelExecutionService.ExecuteModelAsync(request.ModelId, modelInput, request.ModelVersion, cancellationToken);

                // The ModelOutput for anomaly detection needs to be parsed into AnomalyDetails.
                // This parsing logic could be complex and model-specific.
                // For example, modelOutput.Results might contain a list of booleans, scores, etc.
                var anomalyResults = ParseAnomalyDetectionOutput(modelOutput, request.DataPoints, modelOutput.ModelVersionUsed);
                
                _logger.LogInformation("Successfully processed DetectAnomaliesCommand for ModelId: {ModelId}. Found {AnomalyCount} anomalies.", request.ModelId, anomalyResults.Count(a => a.IsAnomaly));
                return anomalyResults;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling DetectAnomaliesCommand for ModelId: {ModelId}", request.ModelId);
                throw;
            }
        }

        private IEnumerable<AnomalyDetails> ParseAnomalyDetectionOutput(ModelOutput modelOutput, List<Dictionary<string, object>> originalDataPoints, string? modelVersionUsed)
        {
            // This is a placeholder. Actual parsing depends heavily on the model's output structure.
            // Example: modelOutput.Results could be a list of dictionaries, or a dictionary with lists.
            // Let's assume modelOutput.Results["anomalies"] is a list of booleans
            // and modelOutput.Results["scores"] is a list of doubles, corresponding to originalDataPoints.

            var results = new List<AnomalyDetails>();
            
            if (modelOutput.Results.TryGetValue("IsAnomaly", out object? isAnomalyObj) && 
                isAnomalyObj is IList<bool> isAnomalyList && 
                isAnomalyList.Count == originalDataPoints.Count)
            {
                IList<double>? scoresList = null;
                if (modelOutput.Results.TryGetValue("AnomalyScore", out object? scoresObj) && scoresObj is IList<double> sList)
                {
                    scoresList = sList;
                }

                IList<string>? explanationsList = null;
                 if (modelOutput.Results.TryGetValue("Explanation", out object? explanationsObj) && explanationsObj is IList<string> eList)
                {
                    explanationsList = eList;
                }


                for (int i = 0; i < originalDataPoints.Count; i++)
                {
                    results.Add(new AnomalyDetails
                    {
                        DataPoint = originalDataPoints[i],
                        IsAnomaly = isAnomalyList[i],
                        AnomalyScore = (scoresList != null && scoresList.Count == originalDataPoints.Count) ? scoresList[i] : (double?)null,
                        Explanation = (explanationsList != null && explanationsList.Count == originalDataPoints.Count) ? explanationsList[i] : null,
                        ModelVersionUsed = modelVersionUsed
                    });
                }
            }
            else
            {
                 // Fallback or simpler interpretation if structured output is not available
                 // This might indicate a single anomaly result for the entire batch
                bool overallAnomaly = false;
                if(modelOutput.Results.TryGetValue("IsAnomaly", out var singleAnomalyFlag) && singleAnomalyFlag is bool flag) {
                    overallAnomaly = flag;
                }
                double? overallScore = null;
                if(modelOutput.Results.TryGetValue("AnomalyScore", out var singleScore) && singleScore is double scoreVal) {
                    overallScore = scoreVal;
                }
                // In such a case, might return one AnomalyDetails for the whole batch or apply to all
                // For this example, let's log a warning and return empty or a generic result.
                _logger.LogWarning("Could not parse anomaly detection output as expected list. ModelOutput: {@ModelOutput}", modelOutput);
                // Create a single result if that's the expectation for some models
                if (originalDataPoints.Count == 1) // Or if model output implies single result for single input
                {
                     results.Add(new AnomalyDetails
                    {
                        DataPoint = originalDataPoints.FirstOrDefault(),
                        IsAnomaly = overallAnomaly,
                        AnomalyScore = overallScore,
                        ModelVersionUsed = modelVersionUsed,
                        Explanation = modelOutput.Results.TryGetValue("Explanation", out var exp) && exp is string s ? s : null
                    });
                }
            }
            return results;
        }
    }
}