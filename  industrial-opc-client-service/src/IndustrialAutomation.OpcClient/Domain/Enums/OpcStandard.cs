namespace IndustrialAutomation.OpcClient.Domain.Enums
{
    /// <summary>
    /// Categorizes the different OPC communication standards supported by the client, 
    /// used for selecting appropriate handlers or configurations.
    /// </summary>
    public enum OpcStandard
    {
        /// <summary>
        /// Unknown or unspecified OPC standard.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// OPC Data Access (Classic).
        /// </summary>
        Da = 1,

        /// <summary>
        /// OPC Unified Architecture.
        /// </summary>
        Ua = 2,

        /// <summary>
        /// OPC XML Data Access.
        /// </summary>
        XmlDa = 3,

        /// <summary>
        /// OPC Historical Data Access (Classic).
        /// </summary>
        Hda = 4,

        /// <summary>
        /// OPC Alarms & Conditions (Classic).
        /// </summary>
        Ac = 5
    }
}