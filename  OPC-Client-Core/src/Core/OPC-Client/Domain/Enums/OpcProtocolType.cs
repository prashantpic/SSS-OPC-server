namespace OPC.Client.Core.Domain.Enums
{
    /// <summary>
    /// Enum defining the supported OPC protocol types.
    /// Provides a strongly-typed enumeration for different OPC protocols supported by the library.
    /// Implements REQ-CSVC-001.
    /// </summary>
    public enum OpcProtocolType
    {
        /// <summary>
        /// Unknown or unspecified protocol.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// OPC Data Access (Classic).
        /// </summary>
        DA = 1,

        /// <summary>
        /// OPC Unified Architecture.
        /// </summary>
        UA = 2,

        /// <summary>
        /// OPC XML Data Access.
        /// </summary>
        XmlDA = 3,

        /// <summary>
        /// OPC Historical Data Access (Classic).
        /// </summary>
        HDA = 4,

        /// <summary>
        /// OPC Alarms and Conditions (Classic).
        /// </summary>
        AC = 5
    }
}