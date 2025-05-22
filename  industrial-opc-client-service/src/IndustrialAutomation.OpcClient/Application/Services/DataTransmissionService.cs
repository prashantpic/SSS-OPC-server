using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.Mappers;
using IndustrialAutomation.OpcClient.Domain.Models;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Messaging;
using IndustrialAutomation.OpcClient.CrossCuttingConcerns.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Polly;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class DataTransmissionService : IDataTransmissionService
    {
        private readonly ILogger<DataTransmissionService> _logger;
        private readonly IServerAppGrpcClient _grpcClient;
        private readonly IServerAppMessageProducer _messageProducer;
        private readonly IDataBufferer _dataBufferer;
        private readonly IClientSideDataValidator _validator;
        private readonly ServerBoundPayloadMapper _payloadMapper;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly string _communicationMethod;
        private readonly string _clientId;

        public DataTransmissionService(
            ILogger<DataTransmissionService> logger,
            IServerAppGrpcClient grpcClient,
            IServerAppMessageProducer messageProducer, // Or MessageProducerFactory
            IDataBufferer dataBufferer,
            IClientSideDataValidator validator,
            ServerBoundPayloadMapper payloadMapper,
            RetryPolicyProvider retryPolicyProvider,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _grpcClient = grpcClient; // Can be null if MQ is used
            _messageProducer = messageProducer; // Can be null if gRPC is used
            _dataBufferer = dataBufferer ?? throw new ArgumentNullException(nameof(dataBufferer));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _payloadMapper = payloadMapper ?? throw new ArgumentNullException(nameof(payloadMapper));
            _retryPolicy = retryPolicyProvider?.GetServerCommsPolicy() ?? Policy.NoOpAsync();
            _clientId = configuration["OpcClient:ClientId"] ?? "UnknownClient";

            _communicationMethod = configuration["ServerApp:CommunicationMethod"]?.ToLowerInvariant();
            if (string.IsNullOrEmpty(_communicationMethod) || (_communicationMethod == "grpc" && _grpcClient == null) || ((_communicationMethod == "rabbitmq" || _communicationMethod == "kafka") && _messageProducer == null))
            {
                _logger.LogWarning("Communication method not properly configured or corresponding client not available. Defaulting to MQ if producer exists, else gRPC if client exists, else logging only.");
                if (_messageProducer != null) _communicationMethod = "mq"; // Generic MQ if type unknown
                else if (_grpcClient != null) _communicationMethod = "grpc";
                else _logger.LogError("No valid communication client (gRPC or MQ) is configured and available.");
            }
        }

        private async Task SendAsync<T>(T payload, Func<T, Task> grpcAction, Func<T, Task> mqAction, string dataType) where T : class
        {
            // TODO: The IClientSideDataValidator's method signature in the prompt is Validate(OpcPointDto, List<ValidationRule>).
            // This service deals with batches (e.g., RealtimeDataBatchDto).
            // The validator needs to be adapted or this logic needs to change to validate individual points within the batch.
            // For now, skipping detailed validation here as the interface doesn't directly fit.
            // Example: if (!await _validator.ValidateDataForTransmissionAsync(new BufferedDataItem(...))) { log error; return; }

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    if (_communicationMethod == "grpc" && _grpcClient != null)
                    {
                        await grpcAction(payload);
                    }
                    else if ((_communicationMethod == "rabbitmq" || _communicationMethod == "kafka" || _communicationMethod == "mq") && _messageProducer != null)
                    {
                        await mqAction(payload);
                    }
                    else
                    {
                        _logger.LogError("No valid communication method configured to send {DataType}.", dataType);
                        throw new InvalidOperationException("No valid communication client configured.");
                    }
                });
                _logger.LogDebug("{DataType} sent successfully.", dataType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {DataType} after retries. Buffering.", dataType);
                var bufferedItem = new BufferedDataItem(dataType, payload, DateTime.UtcNow, 0);
                _dataBufferer.Add(bufferedItem);
            }
        }

        public async Task SendRealtimeDataBatchAsync(RealtimeDataBatchDto batch)
        {
            if (batch == null || batch.DataPoints == null || !batch.DataPoints.Any()) return;
            _logger.LogInformation("Sending realtime data batch with {Count} points.", batch.DataPoints.Count);
            await SendAsync(batch,
                async (b) => { /* await _grpcClient.SendRealtimeDataAsync(b); */ await Task.CompletedTask; _logger.LogWarning("gRPC RealtimeData Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (b) => await _messageProducer.PublishRealtimeDataAsync(b),
                "RealtimeDataBatch");
        }

        public async Task SendHistoricalDataBatchAsync(HistoricalDataBatchDto batch)
        {
            if (batch == null || batch.HistoricalDataPoints == null || !batch.HistoricalDataPoints.Any()) return;
            _logger.LogInformation("Sending historical data batch for query {QueryId}.", batch.QueryId);
            await SendAsync(batch,
                async (b) => { /* await _grpcClient.SendHistoricalDataAsync(b); */ await Task.CompletedTask; _logger.LogWarning("gRPC HistoricalData Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (b) => await _messageProducer.PublishHistoricalDataAsync(b),
                "HistoricalDataBatch");
        }

        public async Task SendAlarmEventBatchAsync(AlarmEventBatchDto batch)
        {
            if (batch == null || batch.AlarmEvents == null || !batch.AlarmEvents.Any()) return;
            _logger.LogInformation("Sending alarm event batch with {Count} events.", batch.AlarmEvents.Count);
            await SendAsync(batch,
                async (b) => { /* await _grpcClient.SendAlarmEventBatchAsync(b); */ await Task.CompletedTask; _logger.LogWarning("gRPC AlarmEventBatch Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (b) => await _messageProducer.PublishAlarmEventAsync(b),
                "AlarmEventBatch");
        }

        public async Task SendClientHealthStatusAsync(ClientHealthStatusDto status)
        {
            if (status == null) return;
            _logger.LogInformation("Sending client health status: {OverallStatus}", status.OverallStatus);
            await SendAsync(status,
                async (s) => await _grpcClient.SendClientStatusAsync(s),
                async (s) => { /* MQ health status publish not typically done, but can be if needed */ await Task.CompletedTask; _logger.LogWarning("ClientHealthStatus via MQ not standardly implemented."); },
                "ClientHealthStatus");
        }

        public async Task SendSubscriptionStatusAsync(SubscriptionStatusDto status)
        {
            if (status == null) return;
            _logger.LogInformation("Sending subscription status for {SubscriptionId}: {Status}", status.SubscriptionId, status.Status);
            await SendAsync(status,
                async (s) => { /* await _grpcClient.SendSubscriptionStatusAsync(s); */ await Task.CompletedTask; _logger.LogWarning("gRPC SubscriptionStatus Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (s) => await _messageProducer.PublishSubscriptionStatusAsync(s),
                "SubscriptionStatus");
        }

        public async Task SendCriticalWriteLogAsync(CriticalWriteLogDto logEntry)
        {
            if (logEntry == null) return;
            _logger.LogInformation("Sending critical write log for tag {TagId}", logEntry.TagId);
            await SendAsync(logEntry,
                 async (l) => { /* await _grpcClient.SendCriticalWriteLogAsync(l); */ await Task.CompletedTask; _logger.LogWarning("gRPC CriticalWriteLog Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (l) => await _messageProducer.PublishCriticalWriteLogAsync(l),
                "CriticalWriteLog");
        }

        public async Task SendAuditEventAsync(AuditEventDto auditEvent)
        {
            if (auditEvent == null) return;
            _logger.LogInformation("Sending audit event: {EventType} - {Description}", auditEvent.EventType, auditEvent.Description);
            await SendAsync(auditEvent,
                async (ae) => { /* await _grpcClient.SendAuditEventAsync(ae); */ await Task.CompletedTask; _logger.LogWarning("gRPC AuditEvent Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (ae) => await _messageProducer.PublishAuditEventAsync(ae),
                "AuditEvent");
        }

        public async Task SendEdgeAiOutputAsync(EdgeModelOutputDto output)
        {
            if (output == null) return;
            _logger.LogInformation("Sending Edge AI output for model {ModelName}", output.ModelName);

            // Example how validator MIGHT be used if it handled EdgeModelOutputDto
            // List<ValidationRule> rules = ... get rules for AI output ...
            // if (!_validator.Validate(output, rules).IsValid) // Fictional Validate signature
            // {
            //    _logger.LogError("Edge AI output validation failed for model {ModelName}. Not sending.", output.ModelName);
            //    return;
            // }

            await SendAsync(output,
                async (o) => { /* await _grpcClient.SendEdgeAiOutputAsync(o); */ await Task.CompletedTask; _logger.LogWarning("gRPC EdgeAiOutput Send not implemented in IServerAppGrpcClient example"); }, // Placeholder
                async (o) => { if (_messageProducer is IKafkaMessageProducer kafkaProducer) await kafkaProducer.PublishEdgeAiOutputAsync(o); else if (_messageProducer is IRabbitMqMessageProducer rabbitProducer) await rabbitProducer.PublishEdgeAiOutputAsync(o); else { _logger.LogWarning("No specific EdgeAI output method on generic producer."); await Task.CompletedTask;} },
                "EdgeAiOutput");
        }

        // Periodically try to send buffered data
        public async Task ProcessBufferedDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting buffered data processor task.");
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_dataBufferer.Count() > 0)
                {
                    if (_dataBufferer.TryTake(out var item))
                    {
                        _logger.LogInformation("Attempting to send buffered item of type {DataType}", item.DataType);
                        try
                        {
                            // Resend logic based on item.DataType and item.Payload
                            // This is a simplified example; a more robust solution would deserialize
                            // and call the appropriate Send method.
                            if (item.Payload is RealtimeDataBatchDto rtd) await SendRealtimeDataBatchAsync(rtd);
                            else if (item.Payload is HistoricalDataBatchDto hdd) await SendHistoricalDataBatchAsync(hdd);
                            else if (item.Payload is AlarmEventBatchDto aed) await SendAlarmEventBatchAsync(aed);
                            else if (item.Payload is CriticalWriteLogDto cwd) await SendCriticalWriteLogAsync(cwd);
                            else if (item.Payload is AuditEventDto aud) await SendAuditEventAsync(aud);
                            else if (item.Payload is SubscriptionStatusDto ssd) await SendSubscriptionStatusAsync(ssd);
                            else if (item.Payload is EdgeModelOutputDto eod) await SendEdgeAiOutputAsync(eod);
                            // ClientHealthStatusDto is usually not buffered as it's a point-in-time status.
                            else
                            {
                                _logger.LogWarning("Unknown payload type in buffer: {DataType}", item.DataType);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error resending buffered item of type {DataType}. Re-queuing if retry count not exceeded.", item.DataType);
                            item = item with { RetryCount = item.RetryCount + 1 };
                            if (item.RetryCount < 5) // Max retries for buffered items
                            {
                                _dataBufferer.Add(item); // Add back to buffer
                            }
                            else
                            {
                                _logger.LogError("Max retries exceeded for buffered item of type {DataType}. Item discarded.", item.DataType);
                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // Check buffer periodically
            }
            _logger.LogInformation("Buffered data processor task stopped.");
        }
    }
}