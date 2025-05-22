using IndustrialAutomation.OpcClient.Domain.Enums;
using System;

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Core representation of an OPC tag within the client's domain, 
/// encapsulating its identity, addressing information, configuration, and last known state.
/// </summary>
public class OpcTag
{
    public required string Id { get; init; }
    public required string OpcAddress { get; init; } // NodeId for UA, ItemId for DA/XML-DA
    public required OpcStandard Standard { get; init; }
    public required string ServerId { get; init; } // Identifier for the OPC Server this tag belongs to
    public string? DataType { get; set; } // Expected data type string (e.g., "Int32", "Double")
    public double ScaleFactor { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public bool IsActive { get; set; } = true;
    public bool IsWritable { get; set; } = false;

    // State information
    public object? LastKnownValue { get; set; }
    public DateTime? LastKnownTimestamp { get; set; }
    public string? LastKnownQuality { get; set; }

    public OpcTag() { } // Parameterless constructor for frameworks or manual instantiation
}