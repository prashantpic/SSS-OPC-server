# Software Design Specification (SDS) for Integration Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the **Integration Service** (`services.integration-service`). This microservice acts as a dedicated bridge between the core industrial data platform and various external and emerging technology systems. Its primary purpose is to encapsulate the complexity of third-party protocols and APIs, providing a standardized set of interfaces for the core platform to interact with IoT platforms, Augmented Reality (AR) devices, Blockchain ledgers, and Digital Twins.

### 1.2. Scope
The scope of this service includes:
- Establishing and managing secure connections to external systems.
- Implementing protocol-specific communication (e.g., MQTT, AMQP, WebSockets, JSON-RPC).
- Handling data mapping, transformation, and validation for bi-directional data flow.
- Providing robust error handling, retries, and circuit-breaking for external integrations.
- Exposing internal interfaces for use by other platform services and being triggered primarily by asynchronous messages or internal API calls.

### 1.3. Acronyms and Abbreviations
- **AR**: Augmented Reality
- **AAS**: Asset Administration Shell
- **DTDL**: Digital Twin Definition Language
- **DLT**: Distributed Ledger Technology (Blockchain)
- **DI**: Dependency Injection
- **IoT**: Internet of Things
- **MQTT**: Message Queuing Telemetry Transport
- **SDS**: Software Design Specification
- **API**: Application Programming Interface
- **RPC**: Remote Procedure Call

## 2. System Architecture

The Integration Service is an ASP.NET Core 8 microservice designed following **Clean Architecture** principles. This separates the code into distinct layers with a clear direction of dependencies, ensuring high cohesion, low coupling, and testability.

- **Domain Layer**: Contains the core business logic and entities, such as the `IntegrationEndpoint` aggregate. It has no dependencies on other layers.
- **Application Layer**: Orchestrates the use cases of the service. It defines interfaces for infrastructure concerns (`IIotPlatformConnector`, `IBlockchainAdapter`, etc.) and contains command/query handlers. It depends only on the Domain layer.
- **Infrastructure Layer**: Provides concrete implementations for the interfaces defined in the Application layer. This is where all external communication logic resides (e.g., MQTT clients, HTTP clients, blockchain libraries). It depends on the Application and Domain layers.
- **Presentation (API) Layer**: Exposes the service's functionality to the outside world. This includes minimal RESTful APIs for configuration and the SignalR Hub for AR streaming. It depends on the Application layer.

The service will heavily utilize the **Strategy Pattern**. A collection of connector/adapter implementations (e.g., `IEnumerable<IIotPlatformConnector>`) will be injected via DI. A factory or handler will then select the appropriate implementation at runtime based on the `EndpointType` of the target `IntegrationEndpoint`.

## 3. Detailed Component Design

### 3.1. Domain Layer

#### 3.1.1. `IntegrationEndpoint` Aggregate
This is the aggregate root representing a single configured external system.
- **File:** `src/Services/Integration/Domain/Aggregates/IntegrationEndpoint.cs`
- **Description:** Encapsulates all configuration, data mapping rules, and state for one integration point (e.g., a connection to a specific Azure IoT Hub or an Ethereum network).
- **Key Logic:**
    - The constructor will be private. A static factory method `Create(...)` will enforce validation rules upon creation (e.g., name is not null, address is valid for the type).
    - Methods like `UpdateConfiguration`, `AddDataMappingRule`, `Enable`, and `Disable` will contain business logic to ensure the aggregate remains in a consistent state.
    - It will not contain any infrastructure-specific code.

#### 3.1.2. `EndpointType` Value Object
A smart enum to provide a strongly-typed classification of integration types.
- **File:** `src/Services/Integration/Domain/ValueObjects/EndpointType.cs`
- **Description:** Defines the supported integration types.
- **Values:** `AzureIot`, `AwsIot`, `AugmentedReality`, `Blockchain`, `DigitalTwin`.

### 3.2. Application Layer

This layer defines the "what" of the service's capabilities through interfaces.

#### 3.2.1. `IIotPlatformConnector` Interface
- **File:** `src/Services/Integration/Application/Interfaces/IIotPlatformConnector.cs`
- **Purpose:** Abstracts sending data to any supported IoT platform.
- **Methods:**
    - `bool Supports(EndpointType type)`: Used by the Strategy pattern to select the correct implementation.
    - `Task<bool> SendDataAsync(IntegrationEndpoint endpoint, string payload, CancellationToken cancellationToken)`: Sends the given JSON payload to the specified endpoint.

#### 3.2.2. `IBlockchainAdapter` Interface
- **File:** `src/Services/Integration/Application/Interfaces/IBlockchainAdapter.cs`
- **Purpose:** Abstracts logging critical data to a blockchain.
- **Methods:**
    - `Task<string> LogTransactionAsync(string dataHash, string transactionDetails, CancellationToken cancellationToken)`: Logs a transaction and returns the blockchain transaction ID/hash.

#### 3.2.3. `IArDataStreamer` Interface
- **File:** `src/Services/Integration/Application/Interfaces/IArDataStreamer.cs`
- **Purpose:** Abstracts broadcasting real-time data to AR clients.
- **Methods:**
    - `Task StreamDataToAllAsync(object data)`: Sends data to all connected AR clients.
    - `Task StreamDataToGroupAsync(string groupName, object data)`: Sends data to a specific group of AR clients.

#### 3.2.4. `IDigitalTwinAdapter` Interface
- **File:** `src/Services/Integration/Application/Interfaces/IDigitalTwinAdapter.cs`
- **Purpose:** Abstracts bi-directional communication with Digital Twins.
- **Methods:**
    - `Task SendDataToTwinAsync(IntegrationEndpoint endpoint, object data)`: Sends data to the twin.
    - `Task<object> ReceiveDataFromTwinAsync(IntegrationEndpoint endpoint)`: Receives data from the twin.

#### 3.2.5. Use Cases (e.g., Command Handlers)
- **File:** `src/Services/Integration/Application/UseCases/Iot/SendDataToIotPlatformHandler.cs`
- **Purpose:** To orchestrate a specific business operation.
- **Logic:**
    1.  Receive a command (e.g., `SendDataToIotPlatformCommand`).
    2.  Use a repository to fetch the `IntegrationEndpoint` aggregate by its ID.
    3.  Use the injected `IEnumerable<IIotPlatformConnector>` to find the correct connector: `_connectors.Single(c => c.Supports(endpoint.Type))`.
    4.  Invoke the connector's `SendDataAsync` method.
    5.  Handle exceptions and log the outcome.

### 3.3. Infrastructure Layer

This layer provides the concrete "how" for the interfaces defined in the Application layer.

#### 3.3.1. `AzureIotConnector`
- **File:** `src/Services/Integration/Infrastructure/Connectors/Iot/AzureIotConnector.cs`
- **Library:** `MQTTnet`
- **Logic:**
    - Implements `IIotPlatformConnector`.
    - `Supports` method returns `true` for `EndpointType.AzureIot`.
    - `SendDataAsync` will:
        - Securely retrieve the connection string or SAS token for the given endpoint from configuration/key vault.
        - Build an MQTT client using `MqttFactory`.
        - Configure the client with the correct hostname, port, TLS, and authentication details for Azure IoT Hub.
        - Connect the client.
        - Publish the payload to the appropriate MQTT topic (e.g., `devices/{deviceId}/messages/events/`).
        - Disconnect and log the result.

#### 3.3.2. `EthereumAdapter`
- **File:** `src/Services/Integration/Infrastructure/Adapters/Blockchain/EthereumAdapter.cs`
- **Library:** `Nethereum.Web3`
- **Logic:**
    - Implements `IBlockchainAdapter`.
    - The constructor will receive Ethereum node configuration (URL, private key for signing account) from `IOptions<EthereumSettings>`.
    - `LogTransactionAsync` will:
        - Create a `Web3` instance with the signing account.
        - Get a handle to the pre-deployed smart contract using its address and ABI.
        - Get a handle to the target function (e.g., `logData`).
        - Call `SendTransactionAndWaitForReceiptAsync` to execute the function with the `dataHash` and `transactionDetails`.
        - Return the `TransactionHash` from the receipt.

#### 3.3.3. `ArDataStreamer`
- **File:** `src/Services/Integration/Infrastructure/Streaming/ArDataStreamer.cs`
- **Library:** `Microsoft.AspNetCore.SignalR`
- **Logic:**
    - Implements `IArDataStreamer`.
    - The constructor is injected with `IHubContext<ArDataStreamerHub>`.
    - `StreamDataToAllAsync` calls `_hubContext.Clients.All.SendAsync("ReceiveData", data)`.
    - `StreamDataToGroupAsync` calls `_hubContext.Clients.Group(groupName).SendAsync("ReceiveData", data)`.

#### 3.3.4. `AasDigitalTwinAdapter`
- **File:** `src/Services/Integration/Infrastructure/Adapters/DigitalTwin/AasDigitalTwinAdapter.cs`
- **Library:** `System.Net.Http.IHttpClientFactory`
- **Logic:**
    - Implements `IDigitalTwinAdapter`.
    - Uses `IHttpClientFactory` to create `HttpClient` instances.
    - Methods will perform RESTful `GET`, `PUT`, `PATCH` requests to the AAS server endpoint defined in the `IntegrationEndpoint` configuration.
    - Logic will include serializing/deserializing data to/from the AAS JSON format.

### 3.4. Presentation (API) Layer

#### 3.4.1. `ArDataStreamerHub`
- **File:** `src/Services/Integration/Api/Hubs/ArDataStreamerHub.cs`
- **Framework:** ASP.NET Core SignalR
- **Logic:**
    - Inherits from `Microsoft.AspNetCore.SignalR.Hub`.
    - `OnConnectedAsync`: Logs a new client connection.
    - `OnDisconnectedAsync`: Logs a client disconnection.
    - `JoinGroup(string groupName)`: Allows a client to join a specific group for targeted data streams. `await Groups.AddToGroupAsync(Context.ConnectionId, groupName);`.

#### 3.4.2. Configuration API (Future consideration)
While not the primary interface, minimal REST endpoints for managing `IntegrationEndpoint` entities could be added here for administrative purposes.

### 3.5. Application Startup & Configuration

#### 3.5.1. `Program.cs`
- **Purpose:** Bootstraps the application.
- **Key Steps:**
    1.  Create `WebApplicationBuilder`.
    2.  Load configuration from `appsettings.json` and environment variables.
    3.  Configure `Serilog` for structured logging.
    4.  **Register Services (DI):**
        - Register all `IIotPlatformConnector` implementations (`.AddSingleton<IIotPlatformConnector, AzureIotConnector>()`).
        - Register other adapters and services (`.AddSingleton<IBlockchainAdapter, EthereumAdapter>()`, `.AddSingleton<IArDataStreamer, ArDataStreamer>()`, etc.).
        - Register repositories and use case handlers.
        - Register `AddSignalR()`.
    5.  Configure the HTTP pipeline.
    6.  Map SignalR Hubs: `app.MapHub<ArDataStreamerHub>("/hubs/arstreamer");`.
    7.  Map Health Checks.
    8.  Run the application.

#### 3.5.2. `appsettings.json`
Provides the configuration structure. Sensitive data must be stored in a secure vault.
json
{
  "Logging": { ... },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "IntegrationDb": "Reference to Key Vault"
  },
  "AzureIotSettings": {
    "DefaultConnectionString": "Reference to Key Vault"
  },
  "EthereumSettings": {
    "NodeUrl": "https://rinkeby.infura.io/v3/...",
    "PrivateKey": "Reference to Key Vault",
    "LoggingContractAddress": "0x..."
  }
}


## 4. Cross-Cutting Concerns

### 4.1. Error Handling and Resilience
- **Polly:** Use the Polly library for resilience. When calling external services (IoT Hubs, Blockchain nodes), wrap the calls in a Polly policy that combines:
    - **Retry:** A transient fault handling policy to retry a few times on network errors or timeouts.
    - **Circuit Breaker:** A policy to stop making calls to a service that is clearly down for a period, preventing cascading failures.
- **Global Exception Middleware:** A custom middleware will catch unhandled exceptions, log them, and return a standardized `application/problem+json` error response.

### 4.2. Logging and Monitoring
- **Structured Logging:** `Serilog` will be used to write structured JSON logs to the console, which can then be easily collected by log aggregation tools (ELK, Splunk, etc.).
- **Log Context:** Enrich logs with contextual information like `IntegrationEndpointId` or `TraceId` to facilitate diagnostics.
- **Health Checks:** Implement ASP.NET Core Health Checks for the service itself and its dependencies (e.g., database, message queue connectivity). Expose this at a `/health` endpoint.

### 4.3. Security
- **Credential Management:** All secrets (connection strings, API keys, private keys) must be loaded from a secure external store like Azure Key Vault or HashiCorp Vault. The .NET Configuration system will be used to bind these secrets at runtime.
- **Transport Security:** All external HTTP, MQTT, and WebSocket communication **must** use TLS.
- **API Security:** Any exposed administrative REST APIs must be protected by an authentication scheme (e.g., validating JWTs issued by a central authentication service).