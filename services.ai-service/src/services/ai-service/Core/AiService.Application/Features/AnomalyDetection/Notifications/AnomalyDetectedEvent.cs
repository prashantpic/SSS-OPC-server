using AiService.Application.Dtos;
using MediatR;

namespace AiService.Application.Features.AnomalyDetection.Notifications;

/// <summary>
/// Represents a notification that is published when an anomaly is detected.
/// This can be consumed by other handlers to perform actions like sending alerts.
/// </summary>
/// <param name="DetectedAnomaly">The details of the detected anomaly.</param>
public record AnomalyDetectedEvent(AnomalyDto DetectedAnomaly) : INotification;