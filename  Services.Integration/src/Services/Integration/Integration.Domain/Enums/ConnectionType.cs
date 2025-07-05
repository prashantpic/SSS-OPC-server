namespace Opc.System.Services.Integration.Domain.Enums;

/// <summary>
/// Provides a strongly-typed identifier for the different types of supported external integrations.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// Integration with Microsoft Azure IoT Hub.
    /// </summary>
    AzureIotHub,

    /// <summary>
    /// Integration with Amazon Web Services IoT Core.
    /// </summary>
    AwsIotCore,

    /// <summary>
    /// Integration with Google Cloud IoT Platform.
    /// </summary>
    GoogleCloudIot,

    /// <summary>
    /// A real-time data stream for Augmented Reality devices.
    /// </summary>
    AugmentedRealityStream,

    /// <summary>
    /// A connection to a permissioned blockchain ledger for data logging.
    /// </summary>
    BlockchainLedger,

    /// <summary>
    /// Integration with a Digital Twin platform (e.g., Azure Digital Twins).
    /// </summary>
    DigitalTwin
}