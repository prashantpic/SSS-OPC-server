using System;

namespace IndustrialAutomation.OpcClient.Domain.Exceptions;

/// <summary>
/// Specific exception type for errors occurring during OPC interactions, 
/// allowing for more granular error handling.
/// </summary>
public class OpcCommunicationException : Exception
{
    public string? ServerId { get; }
    public string? Operation { get; }
    public string? OpcStatusCode { get; } // OPC specific status code, if applicable

    public OpcCommunicationException(string message, string? serverId = null, string? operation = null, string? opcStatusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ServerId = serverId;
        Operation = operation;
        OpcStatusCode = opcStatusCode;
    }

    public OpcCommunicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public OpcCommunicationException(string message)
        : base(message)
    {
    }
}