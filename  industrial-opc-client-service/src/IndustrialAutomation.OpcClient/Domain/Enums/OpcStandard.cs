namespace IndustrialAutomation.OpcClient.Domain.Enums;

/// <summary>
/// Categorizes the different OPC communication standards supported by the client, 
/// used for selecting appropriate handlers or configurations.
/// </summary>
public enum OpcStandard
{
    Unknown = 0,
    Da = 1,      // OPC Data Access
    Ua = 2,      // OPC Unified Architecture
    XmlDa = 3,   // OPC XML-DA
    Hda = 4,     // OPC Historical Data Access
    Ac = 5       // OPC Alarms & Conditions
}