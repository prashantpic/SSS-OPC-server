# Software Design Specification: Services.Integration

## 1. Introduction

### 1.1 Purpose
This document provides a detailed software design for the **Services.Integration** microservice. This service acts as a central hub for integrating the core OPC System with various external ecosystems, including cloud IoT platforms, Augmented Reality (AR) devices, blockchain ledgers, and Digital Twin platforms. It is responsible for protocol translation, data mapping, and secure communication with these external systems.

### 1.2 Scope
The scope of this document is limited to the design of the `Services.Integration` microservice. This includes its internal architecture, components, interfaces, data models, and interaction patterns. It covers the implementation details required to fulfill the following user requirements: `REQ-8-004`, `REQ-8-005`, `REQ-8-006`, `REQ-8-007`, `REQ-8-008`, `REQ-8-010`, and `REQ-8-011`.

## 2. System Overview & Architecture

The `Services.Integration` microservice is a key component of the server-side application. It follows a **Clean Architecture** pattern, separating concerns into distinct layers: Domain, Application, Infrastructure, and API. This design ensures modularity, testability, and maintainability.

- **Domain Layer:** Contains the core business logic and entities, independent of any technology.
- **Application Layer:** Orchestrates the domain logic to fulfill specific use cases (e.g., sending data to IoT Hub). It defines interfaces for infrastructure dependencies.
- **Infrastructure Layer:** Provides concrete implementations for the interfaces defined in the application layer, interacting with external systems and databases (e.g., MQTT clients, blockchain nodes, data transformation engines).
- **API Layer:** Exposes the service's functionality through REST endpoints, gRPC services, and WebSocket hubs. It also hosts background workers for asynchronous tasks.

## 3. Domain Layer Design (`Integration.Domain`)

This layer contains the core business models and rules, with no dependencies on external frameworks.

### 3.1 Aggregates

#### 3.1.1 `IntegrationConnection` (Aggregate Root)
Represents a configured connection to a single external system.

csharp
namespace Opc.System.Services.Integration.Domain.Aggregates;

public class IntegrationConnection
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public ConnectionType ConnectionType { get; private set; }
    public string Endpoint { get; private set; } // e.g., MQTT broker hostname, RPC endpoint URL
    public bool IsEnabled { get; private set; }
    // SecurityConfiguration will store credentials, keys, and certificates.
    // This JSON will be encrypted before being persisted.
    public JsonDocument SecurityConfiguration { get; private set; }
    public Guid? DataMapId { get; private set; } // Optional link to a DataMap

    // Methods to enforce business rules
    public void Enable() { IsEnabled = true; }
    public void Disable() { IsEnabled = false; }
    public void UpdateConfiguration(string name, string endpoint, JsonDocument securityConfig) { /* ... validation ... */ }
    public void AssignDataMap(Guid dataMapId) { DataMapId = dataMapId; }
}

**`SecurityConfiguration` JSON Structure Examples:**
- **MQTT:** `{ "ClientId": "...", "Username": "...", "Password": "...", "UseTls": true }`
- **Blockchain (Nethereum):** `{ "NodeUrl": "...", "PrivateKey": "...", "SmartContractAddress": "...", "ChainId": 12345 }`
- **Azure Digital Twins:** `{ "AdtInstanceUrl": "...", "TenantId": "...", "ClientId": "...", "ClientSecret": "..." }`

#### 3.1.2 `DataMap` (Aggregate Root)
Represents a data transformation configuration.

csharp
namespace Opc.System.Services.Integration.Domain.Aggregates;

public class DataMap
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    // Defines the expected structure of the source data.
    public JsonDocument SourceModelDefinition { get; private set; }
    // Defines the structure of the target data.
    public JsonDocument TargetModelDefinition { get; private set; }
    // TransformationRules will define the mapping logic, e.g., using a simple JSON Path-based mapping.
    public JsonDocument TransformationRules { get; private set; }
    public int Version { get; private set; }
}

**`TransformationRules` JSON Structure Example:**
json
[
  { "sourcePath": "$.tagId", "targetPath": "$.deviceId" },
  { "sourcePath": "$.value", "targetPath": "$.telemetry.temperature" },
  { "sourcePath": "$.timestamp", "targetPath": "$.timestamp", "format": "ISO8601" }
]


### 3.2 Enums

#### 3.2.1 `ConnectionType.cs`
Provides a strongly-typed identifier for the integration type.

csharp
namespace Opc.System.Services.Integration.Domain.Enums;

public enum ConnectionType
{
    AzureIotHub,
    AwsIotCore,
    GoogleCloudIot,
    AugmentedRealityStream,
    BlockchainLedger,
    DigitalTwin
}


## 4. Application Layer Design (`Integration.Application`)

This layer contains the application logic and defines abstractions over infrastructure.

### 4.1 Contracts (Interfaces)

#### 4.1.1 `Contracts.External`
These interfaces abstract away the specific technologies used to communicate with external systems.
- **`IIotPlatformConnector.cs`**:
  csharp
  public interface IIotPlatformConnector
  {
      Task<bool> SendDataAsync(Guid connectionId, string payload);
      Task StartReceivingAsync(Guid connectionId, Func<string, Task> onMessageReceived);
  }
  
- **`IBlockchainAdapter.cs`**:
  csharp
  public interface IBlockchainAdapter
  {
      // Returns the transaction hash
      Task<string> LogDataHashAsync(Guid connectionId, string dataHash, string metadata);
      // Returns a boolean indicating validity and the logged metadata
      Task<(bool IsValid, string Metadata)> VerifyTransactionByHashAsync(Guid connectionId, string dataHash);
  }
  
- **`IDigitalTwinAdapter.cs`**:
  csharp
  public interface IDigitalTwinAdapter
  {
      Task SendDataAsync(Guid connectionId, string twinId, string payload);
      Task StartReceivingCommandsAsync(Guid connectionId, Func<string, Task> onCommandReceived);
  }
  
- **`IArDataStreamer.cs`**:
  csharp
  public interface IArDataStreamer
  {
      Task StreamDataToAllAsync(string payload);
      Task StreamDataToDeviceAsync(string deviceId, string payload);
      int GetConnectedDeviceCount();
  }
  
#### 4.1.2 `Contracts.Infrastructure`
- **`IDataTransformer.cs`**:
  csharp
  public interface IDataTransformer
  {
      Task<string> TransformAsync(Guid dataMapId, string sourcePayload);
  }
  

### 4.2 Features (CQRS Handlers)

#### 4.2.1 `Features.BlockchainLogging.Commands.LogCriticalDataCommandHandler.cs`
Handles the asynchronous logging of a critical event. This will be triggered by a message queue worker, not a direct API call.

csharp
// In the command
public record LogCriticalDataCommand(Guid ConnectionId, string DataPayload, string Metadata) : IRequest<string>;

// In the handler
public class LogCriticalDataCommandHandler : IRequestHandler<LogCriticalDataCommand, string>
{
    private readonly IBlockchainAdapter _blockchainAdapter;
    private readonly ILogger<LogCriticalDataCommandHandler> _logger;

    public LogCriticalDataCommandHandler(IBlockchainAdapter blockchainAdapter, ILogger<LogCriticalDataCommandHandler> logger)
    {
        _blockchainAdapter = blockchainAdapter;
        _logger = logger;
    }

    public async Task<string> Handle(LogCriticalDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Hash the incoming data payload to get a unique, fixed-size fingerprint.
        var dataHash = HashData(request.DataPayload);

        // 2. Call the blockchain adapter to log the hash and metadata.
        _logger.LogInformation("Logging hash {DataHash} to blockchain for connection {ConnectionId}", dataHash, request.ConnectionId);
        var transactionId = await _blockchainAdapter.LogDataHashAsync(request.ConnectionId, dataHash, request.Metadata);
        
        // Note: The full DataPayload is stored off-chain by another service.
        // This service is only responsible for the blockchain transaction itself.

        return transactionId;
    }

    private string HashData(string data) { /* Use SHA256 to compute hash */ }
}


## 5. Infrastructure Layer Design (`Integration.Infrastructure`)

This layer provides concrete implementations for the application contracts.

### 5.1 External Service Implementations

- **`External.Iot.MqttIotPlatformConnector.cs`**:
  - **Library:** `MQTTnet`
  - **Logic:** Manages a pool of `IMqttClient` instances. The `SendDataAsync` method will use the `IntegrationConnection` configuration to build `MqttClientOptions` (server, port, TLS, credentials) and publish the message. It will use Polly resilience policies for connecting and publishing. `StartReceivingAsync` will subscribe to a pre-configured command topic.
- **`External.Blockchain.NethereumBlockchainAdapter.cs`**:
  - **Library:** `Nethereum`
  - **Logic:** The `LogDataHashAsync` method will instantiate a `Web3` client using the node URL from the `IntegrationConnection` configuration. It will load an account with the private key, get a handle to the pre-deployed smart contract, and call a function like `logTransaction(bytes32 dataHash, string metadata)`.
- **`External.DigitalTwin.AzureDigitalTwinAdapter.cs`**:
  - **Library:** `Azure.DigitalTwins.Core`, `Azure.Identity`
  - **Logic:** Uses `DefaultAzureCredential` or client secret from configuration to create a `DigitalTwinsClient`. `SendDataAsync` will use `client.UpdateDigitalTwinAsync` to patch the twin's properties. `StartReceivingCommandsAsync` will require setting up an Event Grid/Hub subscription and a listener to process incoming events from the twin graph.
- **`External.AR.WebSocketArDataStreamer.cs`**:
  - **Library:** `System.Net.WebSockets` (via ASP.NET Core)
  - **Logic:** Implemented as a singleton service. It will maintain a `ConcurrentDictionary<string, WebSocket>` to track connected devices. `StreamDataToDeviceAsync` will look up the WebSocket and send the payload. It will handle connection closures and removal from the dictionary.
- **`Services.JsonDataTransformer.cs`**:
  - **Logic:** Implements `IDataTransformer`. It fetches the `DataMap` entity by ID. It then iterates through the `TransformationRules`, using `System.Text.Json.JsonNode` to parse the source payload, extract values from `sourcePath`, and set them on a new `JsonNode` at the `targetPath`. This provides a flexible, configuration-driven transformation engine.

### 5.2 Resilience
- **`Resilience.IntegrationResiliencePolicies.cs`**:
  - **Library:** `Polly`
  - **Logic:** Defines static methods returning standard policies to be registered with `IHttpClientFactory` and used for other network clients (MQTT, Nethereum).
    - `GetDefaultRetryPolicy()`: Returns a policy that retries 3 times with exponential backoff on transient HTTP errors or `SocketException`.
    - `GetDefaultCircuitBreakerPolicy()`: Returns a policy that breaks the circuit for 30 seconds after 5 consecutive failures.

## 6. API Layer Design (`Integration.API`)

This is the entry point of the microservice, hosting endpoints and workers.

### 6.1 `Program.cs`
- **DI Configuration:** Registers all services, repositories, and handlers. It will use `services.AddHttpClient(...)` with Polly policies for resilient HTTP calls. It registers singleton instances for `IArDataStreamer` and the MQTT/Blockchain/DT adapters.
- **Middleware:** Configures routing, authentication (JWT Bearer), authorization, exception handling, and WebSocket middleware.
- **Hosted Services:** Registers `BlockchainQueueWorker` as a hosted service.

### 6.2 Controllers
- **`Controllers/ConnectionsController.cs`**: Exposes REST endpoints for CRUD operations on `IntegrationConnection` entities. Uses MediatR to dispatch commands and queries to the application layer. All endpoints are protected with `[Authorize(Roles = "Administrator")]`.

### 6.3 Workers
- **`Workers/BlockchainQueueWorker.cs`**:
  - **Type:** `BackgroundService`
  - **Logic:** Subscribes to a message bus (e.g., RabbitMQ) topic named `integration.critical_event.logged`. Upon receiving a message, it deserializes it into a `LogCriticalDataCommand` and sends it for processing using a scoped `IMediator` instance. This ensures total decoupling from the source of the critical event.

### 6.4 WebSocket Hub
- An endpoint like `/ws/ar` will be configured in `Program.cs` to accept WebSocket requests.
- The middleware will manage the WebSocket lifecycle, registering new connections with the `IArDataStreamer` service and handling disconnection.

## 7. Cross-Cutting Concerns

### 7.1 Configuration (`appsettings.json`)
json
{
  "ConnectionStrings": {
    "DefaultConnection": "..." // For storing IntegrationConnection, DataMap entities
  },
  "MessageBus": {
    "Hostname": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "FeatureToggles": {
    "EnableAwsIotConnector": true,
    "EnableAzureIotConnector": true,
    "EnableBlockchainLogging": true,
    "EnableArStreaming": true
  },
  "Serilog": { /* ... */ }
}


### 7.2 Security
- **API Security:** All API endpoints will be protected by JWT Bearer authentication. The token is validated against the public key of the `Services.Auth` microservice.
- **Credential Management:** Credentials for external systems (stored in `IntegrationConnection.SecurityConfiguration`) must be encrypted at rest in the database. The application will use a key managed by a secure vault (e.g., Azure Key Vault, HashiCorp Vault) to encrypt/decrypt this JSON blob. The service will never log these sensitive values.

### 7.3 Logging
- **Library:** `Serilog`
- **Configuration:** Configured to write structured JSON logs to the console, which can be easily collected by a log aggregator like Fluentd or Logstash.
- **Enrichment:** Logs will be enriched with a `SourceContext` (`IntegrationService`) and, where applicable, a `CorrelationId` to trace requests across services.