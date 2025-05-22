using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Represents an active OPC UA subscription, holding its configuration, 
/// list of monitored items, and status information.
/// </summary>
public class Subscription
{
    public required string Id { get; init; } // Unique identifier for this subscription instance
    public required string ServerId { get; init; } // Identifier for the OPC UA Server
    public required UaSubscriptionConfigDto Config { get; set; }
    
    // List of internal TagIds that are part of this subscription
    public List<string> MonitoredInternalTagIds { get; } = []; 

    public string Status { get; set; } = "Initializing"; // e.g., "Initializing", "Active", "Error", "Disconnected"
    public DateTime LastDataChangeTimestamp { get; set; }
    public string? LastErrorMessage { get; set; }
    
    // Could hold the OPC UA SDK's subscription object if needed for direct interaction,
    // but keeping it abstracted is generally better for domain model purity.
    // public object? SdkSubscriptionObject { get; set; }

    public Subscription() { } // For frameworks or manual instantiation
}