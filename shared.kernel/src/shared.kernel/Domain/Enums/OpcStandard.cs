namespace SharedKernel.Domain.Enums;

/// <summary>
/// Defines the set of supported OPC communication standards.
/// </summary>
public enum OpcStandard
{
    /// <summary>
    /// OPC Data Access (Classic)
    /// </summary>
    DA,
    
    /// <summary>
    /// OPC Unified Architecture
    /// </summary>
    UA,
    
    /// <summary>
    /// OPC XML-DA
    /// </summary>
    XmlDa,
    
    /// <summary>
    /// OPC Historical Data Access
    /// </summary>
    Hda,
    
    /// <summary>
    /// OPC Alarms & Conditions
    /// </summary>
    Ac
}