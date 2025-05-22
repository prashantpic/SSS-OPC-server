using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class DataTransmissionService : IDataTransmissionService
    {
        private readonly ILogger<DataTransmissionService> _logger;
        private readonly IServerAppGrpcClient? _grpcClient;
        private readonly IServerAppMessageProducer? _messageProducer;
        private readonly IDataBufferer _dataBufferer;
        private readonly IClientSideDataValidator _clientSideValidator;
        private readonly ServerBoundPayloadMapper _payloadMapper;
        private readonly RetryPolicyProvider _retryPolicyProvider;
        private readonly string _communicationMethod;
        private readonly string _clientId;

        public DataTransmissionService(
            ILogger<DataTransmissionService> logger,
            IConfiguration configuration,
            IDataBufferer dataBufferer,
            IClientSideDataValidator clientSideValidator,
            ServerBoundPayloadMapper payloadMapper,
            RetryPolicyProvider retryPolicyProvider,
            IServiceProvider serviceProvider) // To resolve gRPC client or MQ producer based on config
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataBufferer = dataBufferer ?? throw new ArgumentNullException(nameof(dataBufferer));
            _clientSideValidator = clientSideValidator ?? throw new ArgumentNullException(nameof(clientSideValidator));
            _payloadMapper = payloadMapper ?? throw new ArgumentNullException(nameof(payloadMapper));
            _retryPolicyProvider = retryPolicyProvider ?? throw new ArgumentNullException(nameof(retryPolicyProvider));

            _clientId = configuration["OpcClient:ClientId"] ?? throw new InvalidOperationException("ClientId is not configured.");
            _communicationMethod = configuration["ServerApp:CommunicationMethod"] ?? "MessageQueue"; // Default to MQ

            if (_communicationMethod.Equals("Grpc", StringComparison.OrdinalIgnoreCase))
            {
                _grpcClient = serviceProvider.GetService(typeof(IServerAppGrpcClient)) as IServerAppGrpcClient;
                if (_grpcClient == null) _logger.LogWarning("gRPC client requested but not registered.");
            }
            else // Default or explicit MQ
            {
                _messageProducer = serviceProvider.GetService(typeof(IServerAppMessageProducer)) as IServerAppMessageProducer;
                 if (_messageProducer == null) _logger.LogWarning("Message producer requested but not registered.");
            }
        }

        private async Task TransmitAsync<T>(T payload, Func<T, Task> grpcAction, Func<T, Task> mqAction, string dataType)
        {
            var validationRules = new List<ValidationRule>(); // Load these from config or service
            var bufferedItem = new BufferedDataItem { DataType = dataType, Payload = payload!, Timestamp = DateTime.UtcNow }; // Assuming payload is not null

            if (!_clientSideValidator.Validate(new OpcPointDto(), validationRules).IsValid) // Simplified validation call for the batch itself
            {
                _logger.LogWarning("Validation failed for {DataType} payload. Item will not be sent or buffered.", dataType);
                // Potentially log specific validation errors
                return;
            }
            
            var retryPolicy = _retryPolicyProvider.GetServerCommsPolicy();

            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    if (_communicationMethod.Equals("Grpc", StringComparison.OrdinalIgnoreCase) && _grpcClient != null)
                    {
                        await grpcAction(payload);
                    }
                    else if (_messageProducer != null)
                    {
                        await mqAction(payload);
                    }
                    else
                    {
                        _logger.LogWarning("No valid transmission client (gRPC or MQ) configured or registered for {DataType}. Buffering data.", dataType);
                        throw new InvalidOperationException("No valid transmission client configured."); // Force buffering
                    }
                });
                _logger.LogInformation("{DataType} batch transmitted successfully.", dataType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transmit {DataType} batch after retries. Buffering data.", dataType);
                _dataBufferer.Add(bufferedItem);
            }
        }

        public async Task SendRealtimeDataBatchAsync(RealtimeDataBatchDto batch)
        {
            if (batch == null || batch.DataPoints == null || !batch.DataPoints.Any()) return;
            batch.ClientId = _clientId; // Ensure ClientId is set

            _logger.LogDebug("Preparing to send realtime data batch with {Count} points.", batch.DataPoints.Count);
            await TransmitAsync(batch,
                async (b) => { if (_grpcClient != null) await _grpcClient.SendRealtimeDataAsync(b); },
                async (b) => { if (_messageProducer != null) await _messageProducer.PublishRealtimeDataAsync(b); },
                "RealtimeData");
        }

        public async Task SendHistoricalDataBatchAsync(HistoricalDataBatchDto batch)
        {
            if (batch == null || batch.HistoricalDataPoints == null || !batch.HistoricalDataPoints.Any()) return;
            batch.ClientId = _clientId;

            _logger.LogDebug("Preparing to send historical data batch with {Count} points.", batch.HistoricalDataPoints.Count);
            await TransmitAsync(batch,
                async (b) => { if (_grpcClient != null) await _grpcClient.SendHistoricalDataAsync(b); }, // Assuming method exists
                async (b) => { if (_messageProducer != null) await _messageProducer.PublishHistoricalDataAsync(b); },
                "HistoricalData");
        }

        public async Task SendAlarmEventBatchAsync(AlarmEventBatchDto batch)
        {
            if (batch == null || batch.AlarmEvents == null || !batch.AlarmEvents.Any()) return;
            batch.ClientId = _clientId;

            _logger.LogDebug("Preparing to send alarm event batch with {Count} events.", batch.AlarmEvents.Count);
            await TransmitAsync(batch,
                async (b) => { if (_grpcClient != null) await _grpcClient.SendAlarmEventDataAsync(b); }, // Assuming method exists
                async (b) => { if (_messageProducer != null) await _messageProducer.PublishAlarmEventAsync(b); },
                "AlarmEventData");
        }

        public async Task SendClientHealthStatusAsync(ClientHealthStatusDto status)
        {
            if (status == null) return;
            status.ClientId = _clientId;

            _logger.LogDebug("Preparing to send client health status: {OverallStatus}", status.OverallStatus);
             // Health status typically goes via gRPC if available, or less frequently via MQ
            var retryPolicy = _retryPolicyProvider.GetServerCommsPolicy();
            try
            {
                 await retryPolicy.ExecuteAsync(async () =>
                 {
                    if (_grpcClient != null)
                    {
                        await _grpcClient.SendClientStatusAsync(status);
                    }
                    else if (_messageProducer != null) // Less common for status, but possible
                    {
                        _logger.LogInformation("Sending Client Health status via Message Queue (Uncommon).");
                        // await _messageProducer.PublishClientHealthStatusAsync(status); // Requires specific method
                    }
                    else
                    {
                         _logger.LogWarning("No valid transmission client for ClientHealthStatus.");
                    }
                 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send client health status after retries.");
                // Buffering status is less common, but could be done.
            }
        }

        public async Task SendSubscriptionStatusAsync(SubscriptionStatusDto status)
        {
            if (status == null) return;
            status.ClientId = _clientId;

            _logger.LogDebug("Preparing to send subscription status for {SubscriptionId}: {Status}", status.SubscriptionId, status.Status);
            await TransmitAsync(status,
                async (s) => { if (_grpcClient != null) await _grpcClient.SendSubscriptionStatusAsync(s); }, // Assuming method exists
                async (s) => { if (_messageProducer != null) await _messageProducer.PublishSubscriptionStatusAsync(s); },
                "SubscriptionStatus");
        }

        public async Task SendCriticalWriteLogAsync(CriticalWriteLogDto logEntry)
        {
            if (logEntry == null) return;
            logEntry.ClientId = _clientId;

            _logger.LogDebug("Preparing to send critical write log for TagId: {TagId}", logEntry.TagId);
            await TransmitAsync(logEntry,
                async (l) => { if (_grpcClient != null) await _grpcClient.SendCriticalWriteLogAsync(l); }, // Assuming method exists
                async (l) => { if (_messageProducer != null) await _messageProducer.PublishCriticalWriteLogAsync(l); },
                "CriticalWriteLog");
        }

        public async Task SendAuditEventAsync(AuditEventDto auditEvent)
        {
            if (auditEvent == null) return;
            auditEvent.ClientId = _clientId;

            _logger.LogDebug("Preparing to send audit event: {EventType}", auditEvent.EventType);
            await TransmitAsync(auditEvent,
                async (a) => { if (_grpcClient != null) await _grpcClient.SendAuditEventAsync(a); }, // Assuming method exists
                async (a) => { if (_messageProducer != null) await _messageProducer.PublishAuditEventAsync(a); },
                "AuditEvent");
        }

        public async Task SendEdgeAiOutputAsync(EdgeModelOutputDto output)
        {
            if (output == null) return;
            // output.ClientId = _clientId; // Assuming EdgeModelOutputDto might have a ClientId or it's wrapped

            _logger.LogDebug("Preparing to send Edge AI output for Model: {ModelName}", output.ModelName);

            // Create a wrapper if EdgeModelOutputDto itself doesn't have ClientId for the generic TransmitAsync
            var payload = new { ClientId = _clientId, Output = output };

            await TransmitAsync(payload,
                async (p) => { if (_grpcClient != null) await _grpcClient.SendEdgeAiOutputAsync(p.Output); }, // Assuming method takes EdgeModelOutputDto
                async (p) => { if (_messageProducer != null) await _messageProducer.PublishEdgeAiOutputAsync(p.Output); }, // Assuming method takes EdgeModelOutputDto
                "EdgeAiOutput");
        }


        // Method to periodically check buffer and send data
        public async Task ProcessBufferedDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting buffered data processor task.");
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_dataBufferer.Count() > 0)
                {
                    if (_dataBufferer.TryTake(out BufferedDataItem? item) && item != null)
                    {
                        _logger.LogInformation("Attempting to send buffered item of type: {DataType}", item.DataType);
                        try
                        {
                            // Resend logic based on item.DataType and item.Payload
                            // This needs to be more robust, mapping item.DataType to specific Send*Async methods
                            if (item.Payload is RealtimeDataBatchDto rtBatch) await SendRealtimeDataBatchAsync(rtBatch);
                            else if (item.Payload is HistoricalDataBatchDto histBatch) await SendHistoricalDataBatchAsync(histBatch);
                            else if (item.Payload is AlarmEventBatchDto alarmBatch) await SendAlarmEventBatchAsync(alarmBatch);
                            else if (item.Payload is CriticalWriteLogDto cwLog) await SendCriticalWriteLogAsync(cwLog);
                            else if (item.Payload is AuditEventDto audit) await SendAuditEventAsync(audit);
                            else if (item.Payload is SubscriptionStatusDto subStatus) await SendSubscriptionStatusAsync(subStatus);
                            // else if (item.Payload is EdgeModelOutputDto aiOut) await SendEdgeAiOutputAsync(aiOut); // If EdgeModelOutputDto is directly buffered
                            else if (item.Payload is { ClientId: var _, Output: EdgeModelOutputDto aiOutput }) // For the anonymous type used in SendEdgeAiOutputAsync
                            {
                                await SendEdgeAiOutputAsync(aiOutput);
                            }

                            _logger.LogInformation("Successfully sent buffered item of type: {DataType}", item.DataType);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send buffered item of type {DataType}. Re-queueing if attempts < max.", item.DataType);
                            item.RetryCount++;
                            if (item.RetryCount < 5) // Example max retries
                            {
                                _dataBufferer.Add(item); // Add back to buffer
                            }
                            else
                            {
                                _logger.LogError("Max retries reached for buffered item of type {DataType}. Item discarded. Payload: {@Payload}", item.DataType, item.Payload);
                            }
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // Configurable delay
            }
            _logger.LogInformation("Buffered data processor task stopped.");
        }
    }
}