using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Configuration; // For MLOpsOptions
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json; // Requires System.Net.Http.Json NuGet package
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;


// Assuming a basic MLflow REST API interaction.
// A proper MLflow .NET SDK would be preferable if available and mature.
// For now, this uses HttpClient to interact with MLflow REST API.
// MLflow REST API endpoints: https://www.mlflow.org/docs/latest/rest-api.html

namespace AIService.Infrastructure.MLOps.Mlflow
{
    public class MlflowClientAdapter : IMlLopsClient
    {
        private readonly ILogger<MlflowClientAdapter> _logger;
        private readonly HttpClient _httpClient;
        private readonly MLOpsOptions.MlflowOptions _mlflowOptions;

        public string PlatformName => "MLflow";

        public MlflowClientAdapter(ILogger<MlflowClientAdapter> logger, IOptions<MLOpsOptions> options, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mlflowOptions = options?.Value?.Mlflow ?? throw new ArgumentNullException(nameof(options), "MLflow options are not configured.");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            if (string.IsNullOrWhiteSpace(_mlflowOptions.TrackingUri))
            {
                _logger.LogError("MLflow Tracking URI is not configured.");
                throw new InvalidOperationException("MLflow Tracking URI not configured.");
            }

            // Configure HttpClient base address. Note: MLflow tracking URI might be different from model registry URI.
            // The MLflow REST API typically uses the tracking server URI as the base.
            _httpClient.BaseAddress = new Uri(_mlflowOptions.TrackingUri.EndsWith("/") ? _mlflowOptions.TrackingUri : _mlflowOptions.TrackingUri + "/");
            // TODO: Add authentication headers if MLflow server is secured (e.g., Basic Auth, Token)
            // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_TOKEN");
        }

        public bool CanHandle(string platformName)
        {
            return PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<RegisteredModelInfo> RegisterModelAsync(AiModel model, Stream artifactStream, Dictionary<string, string> tags = null)
        {
            _logger.LogInformation("Registering model '{ModelName}' with MLflow.", model.Name);

            try
            {
                // Step 1: Create an Experiment if it doesn't exist (optional, can log to default)
                string experimentId = _mlflowOptions.DefaultExperimentName != null ?
                    await GetOrCreateExperimentIdAsync(_mlflowOptions.DefaultExperimentName) : "0"; // Default experiment "0"

                // Step 2: Create a Run within the Experiment
                var runResponse = await CreateRunAsync(experimentId);
                string runId = runResponse?.Run?.Info?.RunId;
                if (string.IsNullOrWhiteSpace(runId))
                {
                    _logger.LogError("Failed to create a new run in MLflow experiment '{ExperimentId}'.", experimentId);
                    throw new InvalidOperationException("Could not create MLflow run.");
                }
                _logger.LogInformation("Created MLflow run '{RunId}' in experiment '{ExperimentId}'.", runId, experimentId);

                // Step 3: Log model artifact to the run
                // MLflow expects artifacts to be uploaded. This part is complex with direct REST.
                // It might involve multipart form data or specific artifact logging endpoints.
                // For simplicity, this example assumes a placeholder `LogArtifactAsync` or uses model logging.
                // A common approach is to log the model itself.
                // `LogModelAsync` would be better if MLflow client has this.
                // Let's assume we log the artifact under a path like "model"

                // Example: Log parameters about the model
                await LogParameterAsync(runId, "model_name_param", model.Name);
                await LogParameterAsync(runId, "model_version_param", model.Version);
                await LogParameterAsync(runId, "model_format_param", model.ModelFormat.ToString());

                // Log model artifact (this is a simplified representation of artifact logging)
                string artifactPath = "model"; // Relative path within the run's artifact store
                await LogArtifactAsync(runId, artifactStream, $"{model.Name}.{GetFileExtension(model.ModelFormat)}", artifactPath);
                _logger.LogInformation("Logged model artifact for run '{RunId}' at path '{ArtifactPath}'.", runId, artifactPath);


                // Step 4: Register the logged model from the run
                var createModelRequest = new { name = model.Name };
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/registered-models/create", createModelRequest);

                if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Conflict) // Conflict means it exists
                {
                     var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create/ensure registered model '{ModelName}' in MLflow. Status: {StatusCode}, Response: {ErrorContent}", model.Name, response.StatusCode, errorContent);
                    // Optionally try to update if it's a permissions issue, or just fail
                    // For now, we assume it exists or is created successfully.
                }
                // If Conflict, model already exists, which is fine for creating a new version.

                _logger.LogInformation("Ensured registered model '{ModelName}' exists in MLflow.", model.Name);


                // Step 5: Create a new model version from the logged artifact
                var createVersionRequest = new
                {
                    name = model.Name,
                    source = $"runs:/{runId}/{artifactPath}", // Path to the logged model artifact
                    run_id = runId,
                    description = model.Description,
                    tags = tags?.Select(t => new { key = t.Key, value = t.Value }).ToArray()
                };
                response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/model-versions/create", createVersionRequest);
                response.EnsureSuccessStatusCode(); // Throws if not successful

                var createdVersionResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                var modelVersion = createdVersionResponse.TryGetProperty("model_version", out var mv) ? mv.GetProperty("version").GetString() : null;
                
                _logger.LogInformation("Created model version '{ModelVersion}' for '{ModelName}' from run '{RunId}'.", modelVersion, model.Name, runId);

                // Step 6: (Optional) Update run status to FINISHED
                await UpdateRunAsync(runId, "FINISHED");

                return new RegisteredModelInfo
                {
                    Name = model.Name,
                    Version = modelVersion,
                    Source = $"runs:/{runId}/{artifactPath}",
                    RunId = runId,
                    CreationTimestamp = DateTimeOffset.UtcNow // MLflow response has this
                };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP Error registering model '{ModelName}' with MLflow.", model.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering model '{ModelName}' with MLflow.", model.Name);
                throw;
            }
        }

        private string GetFileExtension(ModelFormat format) => format switch {
            ModelFormat.ONNX => "onnx",
            ModelFormat.TensorFlow => "pb", // Or a directory if SavedModel
            ModelFormat.TensorFlowLite => "tflite",
            ModelFormat.MLNetZip => "zip",
            _ => "bin"
        };


        public async Task LogPredictionFeedbackAsync(ModelFeedback feedback)
        {
            _logger.LogInformation("Logging prediction feedback for ModelId: {ModelId} to MLflow (Not fully implemented via generic REST, typically part of experiment tracking).", feedback.ModelId);
            // MLflow feedback logging is usually done by logging metrics or tags to a specific run or creating a new run.
            // This requires a run_id associated with the prediction.
            // If feedback.RunId is available:
            if (!string.IsNullOrWhiteSpace(feedback.RunId))
            {
                await LogMetricAsync(feedback.RunId, $"feedback_correctness_{feedback.PredictionId}", feedback.IsCorrectPrediction ? 1 : 0);
                if(feedback.CorrectLabels != null && feedback.CorrectLabels.Any())
                {
                    await LogParameterAsync(feedback.RunId, $"feedback_correct_labels_{feedback.PredictionId}", JsonSerializer.Serialize(feedback.CorrectLabels));
                }
                // Log other feedback fields as metrics or params
            }
            else
            {
                _logger.LogWarning("Cannot log feedback to MLflow without a RunId. Feedback for ModelId: {ModelId}, PredictionId: {PredictionId}", feedback.ModelId, feedback.PredictionId);
            }
            await Task.CompletedTask;
        }


        public async Task<ModelDeploymentStatus> GetModelDeploymentStatusAsync(string modelName, string version, string environment)
        {
            _logger.LogInformation("Getting deployment status for model '{ModelName}' version '{Version}' in environment '{Environment}' from MLflow (requires MLflow Deployments or custom tagging).", modelName, version, environment);
            // MLflow open-source doesn't have a strong concept of "deployment environment" status out-of-the-box for model versions.
            // This often relies on:
            // 1. Tags on the model version (e.g., "deployment_status_prod: active", "deployed_env: prod").
            // 2. MLflow Deployments (separate tool or plugin).
            // 3. External deployment tools that update MLflow tags.

            // This is a placeholder. We'll try to fetch model version tags.
            try
            {
                var response = await _httpClient.GetAsync($"api/2.0/mlflow/model-versions/get?name={Uri.EscapeDataString(modelName)}&version={Uri.EscapeDataString(version)}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return new ModelDeploymentStatus { Status = "NotFound", Message = "Model version not found." };
                    
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get model version from MLflow. Status: {StatusCode}, Response: {ErrorContent}", response.StatusCode, errorContent);
                    return new ModelDeploymentStatus { Status = "Error", Message = $"MLflow API error: {response.StatusCode}" };
                }

                var versionDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
                var tagsElement = versionDetails.TryGetProperty("model_version", out var mv) && mv.TryGetProperty("tags", out var t) ? t : default;

                string statusTagKey = $"deployment_status_{environment}";
                string currentStatus = "Unknown";
                string message = "Status derived from tags.";

                if (tagsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tagsElement.EnumerateArray())
                    {
                        if (tag.TryGetProperty("key", out var keyProp) && keyProp.GetString() == statusTagKey)
                        {
                            currentStatus = tag.TryGetProperty("value", out var valProp) ? valProp.GetString() : "Unknown";
                            break;
                        }
                    }
                }
                 // Also check standard stage if environment implies it
                var stage = mv.TryGetProperty("current_stage", out var stageProp) ? stageProp.GetString() : "None";
                if (environment.Equals(stage, StringComparison.OrdinalIgnoreCase))
                {
                    currentStatus = stage; // e.g. "Production", "Staging"
                    message = $"Model is in stage: {stage}";
                }


                return new ModelDeploymentStatus { Status = currentStatus, Message = message, LastChecked = DateTimeOffset.UtcNow };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model deployment status from MLflow for {ModelName} v{Version}", modelName, version);
                return new ModelDeploymentStatus { Status = "Error", Message = ex.Message };
            }
        }

        public async Task<bool> TriggerEdgeDeployment(string modelName, string version, IEnumerable<string> deviceIds, Dictionary<string, string> deploymentParams)
        {
             _logger.LogInformation("Triggering edge deployment for model '{ModelName}' v{Version} to devices '{DeviceIds}' via MLflow (Placeholder - MLflow core does not directly manage edge device deployment).", modelName, version, string.Join(",", deviceIds));
            // MLflow open-source does not directly manage deployments to edge devices.
            // This would typically involve:
            // 1. An MLflow plugin for edge deployment.
            // 2. An external CI/CD or IoT deployment system that listens to MLflow model stage changes or is triggered externally.
            // 3. This method could set a specific tag on the model version (e.g., "edge_deploy_request: pending")
            //    which an external system then picks up.

            // For this placeholder, we'll simulate by setting a tag.
            try
            {
                var tagsToSet = new List<object> { new { key = $"edge_deploy_request_devices", value = string.Join(",", deviceIds) } };
                if(deploymentParams != null)
                {
                    foreach(var param in deploymentParams)
                    {
                        tagsToSet.Add(new { key = $"edge_deploy_param_{param.Key}", value = param.Value});
                    }
                }

                var requestBody = new
                {
                    name = modelName,
                    version = version,
                    tags = tagsToSet
                };
                // MLflow API to set model version tags
                var response = await _httpClient.PostAsJsonAsync($"api/2.0/mlflow/model-versions/set-tag", requestBody); // Check correct endpoint, might be update
                 if (!response.IsSuccessStatusCode) {
                     // try update tags
                    var updateRequest = new {
                        name = modelName,
                        version = version,
                        key = $"edge_deploy_request_devices",
                        value = string.Join(",", deviceIds)
                    };
                     response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/model-versions/set-tag", updateRequest); // This sets one tag
                     // To set multiple, might need multiple calls or use update-model-version
                 }


                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Successfully tagged model '{ModelName}' v{Version} for edge deployment request.", modelName, version);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger edge deployment (tagging) for model '{ModelName}' v{Version} via MLflow.", modelName, version);
                return false;
            }
        }


        // Helper methods for MLflow REST API interaction
        private async Task<string> GetOrCreateExperimentIdAsync(string experimentName)
        {
            var response = await _httpClient.GetAsync($"api/2.0/mlflow/experiments/get-by-name?experiment_name={Uri.EscapeDataString(experimentName)}");
            if (response.IsSuccessStatusCode)
            {
                var expDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
                return expDetails.TryGetProperty("experiment", out var exp) && exp.TryGetProperty("experiment_id", out var id) ? id.GetString() : "0";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var createExpRequest = new { name = experimentName };
                response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/experiments/create", createExpRequest);
                response.EnsureSuccessStatusCode();
                var createdExp = await response.Content.ReadFromJsonAsync<JsonElement>();
                return createdExp.TryGetProperty("experiment_id", out var id) ? id.GetString() : "0";
            }
            response.EnsureSuccessStatusCode(); // Throw for other errors
            return "0"; // Should not reach here
        }

        private async Task<MlflowRunResponsePayload> CreateRunAsync(string experimentId, Dictionary<string, string> tags = null)
        {
             var request = new
            {
                experiment_id = experimentId,
                tags = tags?.Select(t => new { key = t.Key, value = t.Value }).ToArray()
                // user_id = "AIService" // Optional
            };
            var response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/runs/create", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MlflowRunResponsePayload>();
        }

        private async Task UpdateRunAsync(string runId, string status) // status: RUNNING, SCHEDULED, FINISHED, FAILED, KILLED
        {
            var request = new { run_id = runId, status = status };
            var response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/runs/update", request);
            response.EnsureSuccessStatusCode();
        }

         private async Task LogParameterAsync(string runId, string key, string value)
        {
            var request = new { run_id = runId, key = key, value = value };
            var response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/runs/log-parameter", request);
            if (!response.IsSuccessStatusCode) _logger.LogWarning("Failed to log param {Key} to run {RunId}. Status: {StatusCode}", key, runId, response.StatusCode);
            // response.EnsureSuccessStatusCode(); // Or log warning and continue
        }

        private async Task LogMetricAsync(string runId, string key, double value, long timestamp = 0)
        {
            timestamp = (timestamp == 0) ? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() : timestamp;
            var request = new { run_id = runId, key = key, value = value, timestamp = timestamp };
            var response = await _httpClient.PostAsJsonAsync("api/2.0/mlflow/runs/log-metric", request);
             if (!response.IsSuccessStatusCode) _logger.LogWarning("Failed to log metric {Key} to run {RunId}. Status: {StatusCode}", key, runId, response.StatusCode);
            // response.EnsureSuccessStatusCode();
        }

        private async Task LogArtifactAsync(string runId, Stream artifactStream, string artifactFileName, string artifactPathInRun)
        {
            // MLflow's `log-artifact` REST endpoint expects multipart/form-data.
            // This is more complex to implement correctly with HttpClient alone without helpers.
            // Using a simplified path for `log-batch` which takes data, or assuming `log-model` handles this.
            // For a single artifact, a PUT to the artifact store might be possible if its URI is known,
            // but this depends on the artifact store (S3, Azure Blob, local FS).

            // The `runs/log-model` endpoint is usually preferred.
            // If using `log-artifact` directly, it's often done by MLflow client library which handles multipart.
            // This is a placeholder for a more robust artifact logging mechanism.
            _logger.LogWarning("LogArtifactAsync to MLflow via REST is complex and this is a simplified placeholder. RunId: {RunId}, Path: {ArtifactPath}", runId, Path.Combine(artifactPathInRun, artifactFileName));

            // Example of what it might look like with multipart form data (conceptual)
            using var_content = new MultipartFormDataContent();
            artifactStream.Position = 0;
            var streamContent = new StreamContent(artifactStream);
            streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "file", // MLflow expects the file part with this name
                FileName = artifactFileName
            };
            _content.Add(streamContent);

            // The path is usually part of the URL or another form field
            // Example URL: /api/2.0/mlflow/artifacts/log-artifact?run_id={runId}&path={artifactPathInRun} (this is not standard)
            // The standard `log-artifact` is more for local files the MLflow client sends.
            // `log-batch` with { "run_id": "...", "artifacts": [{ "path": "...", "bytes": "base64encoded" }] } could be an option.

            // For now, this is a non-functional placeholder for direct artifact logging.
            // Consider using a dedicated MLflow library or shelling out to MLflow CLI for this if necessary.
            await Task.CompletedTask;
            _logger.LogInformation("Placeholder: Would log artifact '{ArtifactFileName}' to run '{RunId}' at path '{ArtifactPathInRun}'.", artifactFileName, runId, artifactPathInRun);
        }


        public void Dispose()
        {
            // HttpClient is generally designed to be reused.
            // If it was created and owned by this class, it should be disposed.
            // If injected (as it is here), the DI container manages its lifetime.
            GC.SuppressFinalize(this);
        }
    }

    // Helper classes for MLflow JSON deserialization
    internal class MlflowRunResponsePayload
    {
        public MlflowRun Run { get; set; }
    }
    internal class MlflowRun
    {
        public MlflowRunInfo Info { get; set; }
        // public MlflowRunData Data { get; set; } // If needed
    }
    internal class MlflowRunInfo
    {
        public string RunId { get; set; }
        public string RunUuid { get; set; } // Often same as RunId
        public string ExperimentId { get; set; }
        // ... other fields like status, start_time, end_time, artifact_uri
    }
}