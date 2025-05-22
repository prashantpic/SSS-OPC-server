using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using IndustrialAutomation.OpcClient.Domain.Enums;

namespace IndustrialAutomation.OpcClient.Infrastructure.Configuration;

// Placeholder DTOs for configuration sections
// These would ideally be more detailed and match appsettings.json structure precisely.
public class OpcClientSettings
{
    public string ClientId { get; set; } = "DefaultClientId";
    public List<ServerConnectionConfigDto> ServerConnections { get; set; } = [];
    public TagImportConfigDto? TagImport { get; set; }
    public List<UaSubscriptionConfigDto> Subscriptions { get; set; } = [];
}

public class ServerAppSettings
{
    public string CommunicationMethod { get; set; } = "Grpc"; // Grpc, RabbitMq, Kafka
    public string? GrpcEndpoint { get; set; }
    public MessageQueueSettings? MessageQueue { get; set; }
}

public class MessageQueueSettings
{
    public string? ConnectionString { get; set; } // For RabbitMQ
    public string? BrokerList { get; set; }       // For Kafka
    public Dictionary<string, string> Topics { get; set; } = []; // e.g., "RealtimeData": "opc.realtime"
    public SecuritySettings? Security { get; set; }
}

public class SecuritySettings // Simplified
{
    public bool UseTls { get; set; } = false;
    public string? CaCertPath { get; set; }
    public string? ClientCertPath { get; set; }
    public string? ClientKeyPath { get; set; }
}


public class EdgeAiSettings
{
    public string ModelPath { get; set; } = "./models";
    public string? ActiveModelName { get; set; }
    public string? ActiveModelVersion { get; set; }
    public int PredictionIntervalMs { get; set; } = 5000;
}

public class DataHandlingSettings
{
    public int BufferCapacity { get; set; } = 10000;
    public int TransmissionBatchSize { get; set; } = 100;
    public List<ValidationRule> ValidationRules { get; set; } = [];
}


/// <summary>
/// Offers a strongly-typed way to access application configuration values 
/// defined in appsettings.json and environment variables.
/// </summary>
public class AppSettingsProvider
{
    private readonly IConfiguration _configuration;

    public AppSettingsProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public OpcClientSettings GetOpcClientSettings() =>
        _configuration.GetSection("OpcClient").Get<OpcClientSettings>() ?? new OpcClientSettings();

    public ServerAppSettings GetServerAppSettings() =>
        _configuration.GetSection("ServerApp").Get<ServerAppSettings>() ?? new ServerAppSettings();

    public EdgeAiSettings GetEdgeAiSettings() =>
        _configuration.GetSection("EdgeAI").Get<EdgeAiSettings>() ?? new EdgeAiSettings();
    
    public DataHandlingSettings GetDataHandlingSettings() =>
        _configuration.GetSection("DataHandling").Get<DataHandlingSettings>() ?? new DataHandlingSettings();

    public List<WriteLimitPolicy> GetWriteLimitPolicies() =>
        _configuration.GetSection("WriteLimits").Get<List<WriteLimitPolicy>>() ?? [];
    
    public string GetClientId() => GetOpcClientSettings().ClientId;
}