# Software Design Specification (SDS): Core OPC Client Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Core OPC Client Service** (`services.core-opc-client`). This service is a foundational component of the industrial data platform, acting as the primary bridge between the Operational Technology (OT) environment (OPC servers) and the Information Technology (IT) environment (server-side application). It is designed as a .NET 8 cross-platform, standalone executable capable of running on various operating systems (Windows, Linux, macOS) and edge devices.

### 1.2. Scope
The scope of this document is limited to the design of the `services.core-opc-client` repository. It covers the internal architecture, component design, data models, communication protocols, and logic required to fulfill its responsibilities, which include:
- Multi-protocol communication with OPC servers (UA, DA, HDA, A&C).
- Real-time and historical data acquisition.
- Alarm and event monitoring.
- Secure, resilient communication with the server-side application.
- (Optional) Local execution of AI models for edge processing.

This document will serve as the technical blueprint for the development of this service.

## 2. System Overview & Architecture

The Core OPC Client Service employs a layered architecture internally, promoting separation of concerns, testability, and maintainability. It functions as a client in two contexts:
1.  **As a client to OPC Servers:** It initiates connections to industrial hardware controllers and historians.
2.  **As a client to the Server-Side Application:** It pushes collected data and events via a message bus and pulls configuration updates via gRPC.

### 2.1. Architectural Layers
The service is structured into the following layers:

-   **Domain Layer:** Contains business logic, entities, and abstractions (interfaces) that are independent of any specific technology or framework. It defines the core contracts of the application.
-   **Application Layer:** Orchestrates the domain logic. It contains the primary application services, hosted services, and use case implementations. It depends on the Domain layer but not on the Infrastructure layer.
-   **Infrastructure Layer:** Contains concrete implementations of the domain abstractions. This layer handles all external concerns, such as communication with OPC servers, message buses, AI runtimes, and file systems. It depends on the Domain and Application layers.

### 2.2. Component Diagram

mermaid
graph TD
    subgraph Core OPC Client Service
        subgraph Application Layer
            A1[OpcClientHost] --> A2[ConnectionManager]
            A2 --> A3[DataFlowOrchestrator]
            A2 --> F1[OpcClientFactory]
        end

        subgraph Domain Layer
            D1[Interfaces: IOpcProtocolClient, IMessageBusPublisher, IEdgeAiRuntime, IDataBuffer]
            D2[Entities: DataPoint, AlarmEvent, OpcConnectionSettings]
        end

        subgraph Infrastructure Layer
            I1[OPC UA Client] -- implements --> D1
            I2[OPC HDA Client] -- implements --> D1
            I3[OPC A&C Client] -- implements --> D1
            I4[RabbitMqPublisher] -- implements --> D1
            I5[OnnxAiRuntime] -- implements --> D1
            I6[FileBasedDataBuffer] -- implements --> D1
            F1[OpcClientFactory]
        end
    end

    subgraph External Systems
        OPC_UA[OPC UA Server]
        OPC_HDA[OPC HDA Server]
        OPC_AC[OPC A&C Server]
        RabbitMQ[Message Bus]
        AI_Model[ONNX Model File]
        Backend[Backend gRPC Service]
    end

    A1 -- Manages --> Application Layer
    Application Layer -- Depends on --> Domain Layer
    Infrastructure Layer -- Depends on --> Domain Layer

    A3 -- uses --> I4
    I4 -- sends to --> RabbitMQ
    A3 -- uses --> I6

    I1 -- communicates with --> OPC_UA
    I2 -- communicates with --> OPC_HDA
    I3 -- communicates with --> OPC_AC
    I5 -- loads --> AI_Model
    A2 -- calls --> Backend


## 3. Core Components Design

### 3.1. Application Layer

#### 3.1.1. `OpcClientHost` (Hosted Service)
-   **Purpose:** The main long-running background service that controls the application's lifecycle.
-   **Responsibilities:**
    -   Integrates with the .NET `IHostedService` framework.
    -   On start (`StartAsync`), it triggers the `ConnectionManager` to initialize all configured OPC connections.
    -   On stop (`StopAsync`), it ensures a graceful shutdown of all connections and dependent services.
-   **Key Methods:**
    -   `Task StartAsync(CancellationToken cancellationToken)`: Fetches initial configuration and calls `ConnectionManager.InitializeAllAsync()`.
    -   `Task StopAsync(CancellationToken cancellationToken)`: Calls `ConnectionManager.DisconnectAllAsync()` and disposes resources.

#### 3.1.2. `ConnectionManager`
-   **Purpose:** To manage the lifecycle of all configured OPC server connections.
-   **Responsibilities:**
    -   Reads OPC connection settings from configuration.
    -   Uses an `OpcClientFactory` to instantiate the appropriate `IOpcProtocolClient` for each connection.
    -   Initiates, maintains, and monitors the health of all connections.
    -   Implements retry logic for failed connection attempts.
-   **Key Methods:**
    -   `Task InitializeAllAsync()`: Iterates through configured connections, creates clients via the factory, and calls their `ConnectAsync` methods.
    -   `Task DisconnectAllAsync()`: Gracefully disconnects all active clients.

#### 3.1.3. `DataFlowOrchestrator`
-   **Purpose:** A central service to process and route data received from OPC clients.
-   **Responsibilities:**
    -   Subscribes to data events raised by the various `IOpcProtocolClient` implementations.
    -   If Edge AI is enabled, routes relevant data to the `IEdgeAiRuntime` for inference.
    -   Publishes real-time data, alarms, and AI results to the message bus via `IMessageBusPublisher`.
-   **Key Methods:**
    -   `void OnDataReceived(DataPoint data)`: Event handler that receives data, potentially runs it through the AI runtime, and then publishes it.
    -   `void OnAlarmReceived(AlarmEvent alarm)`: Event handler that receives alarms and publishes them.

### 3.2. Infrastructure Layer

#### 3.2.1. `OpcUaProtocolClient`
-   **Purpose:** Handles all communication with OPC UA servers.
-   **Implements:** `IOpcProtocolClient`
-   **Dependencies:** `OPCFoundation.NetStandard.Opc.Ua`, `SubscriptionManager`
-   **Responsibilities:**
    -   Manages the OPC UA `Session` lifecycle.
    -   Handles UA security, including certificate management and endpoint selection based on security policy. (REQ-3-001)
    -   Performs synchronous reads and writes. (REQ-CSVC-003, REQ-CSVC-004)
    -   Delegates all subscription-based data collection to the `SubscriptionManager`.
-   **Key Methods:**
    -   `Task ConnectAsync(OpcConnectionSettings settings, CancellationToken ct)`: Establishes a secure session with the UA server.
    -   `Task<IEnumerable<DataPoint>> ReadAsync(...)`: Executes a batch read operation.

#### 3.2.2. `SubscriptionManager`
-   **Purpose:** Encapsulates the logic for managing OPC UA subscriptions.
-   **Responsibilities:**
    -   Creates and manages `Subscription` objects on the OPC UA session.
    -   Adds `MonitoredItem`s for configured tags.
    -   Handles data change notifications, keep-alives, and status changes. (REQ-CSVC-023)
    -   Implements logic to detect subscription loss and attempts re-establishment. (REQ-CSVC-026)
    -   Transforms incoming OPC UA notifications into domain `DataPoint` objects and raises an event for the `DataFlowOrchestrator`.
-   **Key Methods:**
    -   `void CreateSubscription(SubscriptionSettings settings)`: Creates a new subscription on the server.
    -   `void OnNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)`: Private event handler for data changes. This is a critical path.

#### 3.2.3. `OpcHdaProtocolClient` & `OpcAcProtocolClient`
-   **Purpose:** To provide connectivity to legacy OPC Classic HDA and A&C servers.
-   **Implements:** `IOpcProtocolClient` (or a specialized variant for historical/alarm data)
-   **Dependencies:** A third-party OPC Classic .NET wrapper library.
-   **Responsibilities:**
    -   Abstract the complexities of DCOM-based OPC Classic communication.
    -   Implement methods for querying historical data (HDA). (REQ-CSVC-011)
    -   Implement methods for subscribing to and acknowledging alarms (A&C). (REQ-CSVC-017, REQ-CSVC-018)
    -   Translate data and events to and from the application's domain models.

#### 3.2.4. `OnnxAiRuntime`
-   **Purpose:** To execute AI models locally on the edge device.
-   **Implements:** `IEdgeAiRuntime`
-   **Dependencies:** `Microsoft.ML.OnnxRuntime`
-   **Responsibilities:**
    -   Load a specified `.onnx` model file into an `InferenceSession`. (REQ-7-001, REQ-8-001)
    -   Pre-process input data into the required tensor format.
    -   Execute the model inference.
    -   Post-process the model's output tensors into a structured domain object.
-   **Key Methods:**
    -   `Task LoadModelAsync(string modelPath)`: Initializes the `InferenceSession`.
    -   `Task<ModelOutput> RunInferenceAsync(ModelInput input)`: The core inference execution logic.

#### 3.2.5. `RabbitMqPublisher`
-   **Purpose:** To publish data and events to the RabbitMQ message bus.
-   **Implements:** `IMessageBusPublisher`
-   **Dependencies:** `RabbitMQ.Client`, `IDataBuffer`
-   **Responsibilities:**
    -   Manage connection and channel to the RabbitMQ server.
    -   Implement connection retry and resilience logic.
    -   Serialize domain objects (e.g., `DataPoint`, `AlarmEvent`) to JSON.
    -   Publish messages to the appropriate exchanges and routing keys.
    -   If publishing fails due to network issues, it will delegate the message to the `IDataBuffer` service for later delivery.
-   **Key Methods:**
    -   `Task PublishDataPointsAsync(IEnumerable<DataPoint> dataPoints)`: Publishes a batch of data points.
    -   `Task PublishAlarmAsync(AlarmEvent alarmEvent)`: Publishes a single alarm event.

#### 3.2.6. `FileBasedDataBuffer`
-   **Purpose:** To provide a durable, temporary store for data that cannot be sent to the message bus during network outages. (REQ-CSVC-026)
-   **Implements:** `IDataBuffer`
-   **Responsibilities:**
    -   Accepts data payloads (e.g., serialized `DataPoint` or `AlarmEvent` objects).
    -   Writes payloads to a local file-based queue in a FIFO manner.
    -   Runs a background task to periodically read from the queue and attempt to re-publish the data via `IMessageBusPublisher`.
    -   Removes data from the queue only after successful publication.
    -   Manages queue size to prevent unbounded disk usage.
-   **Key Methods:**
    -   `Task EnqueueAsync(BufferedMessage message)`: Adds a message to the durable queue.
    -   `Task<BufferedMessage> DequeueAsync()`: Retrieves the next message for publishing.
    -   `Task FlushBufferAsync()`: The main loop of the background worker that attempts to send buffered data.

## 4. Data Models (Domain Entities)

Key domain models will be defined as C# records for immutability.

csharp
// In namespace services.opc.client.Domain.Models

// Represents a single data point from any OPC server
public record DataPoint(
    string TagId,       // Unique identifier for the tag
    object Value,       // The value of the tag
    long SourceTimestamp, // Timestamp from the originating device
    int StatusCode,     // OPC status code (e.g., Good, Bad)
    long CollectionTimestamp // Timestamp when collected by this service
);

// Represents an alarm or event
public record AlarmEvent(
    string SourceNode,
    string ConditionName,
    string Message,
    int Severity,
    DateTime OccurrenceTime,
    bool IsAcknowledged
);

// Represents the configuration for a single OPC Server connection
public record OpcConnectionSettings(
    string ServerId,    // A unique name/ID for this connection
    string Protocol,    // "UA", "HDA", "AC"
    string EndpointUrl,
    string SecurityPolicy, // For UA: e.g., "Basic256Sha256"
    string SecurityMode,   // For UA: e.g., "SignAndEncrypt"
    List<TagSettings> TagsToMonitor
);

// Represents AI model input
public record ModelInput(float[] Features);

// Represents AI model output
public record ModelOutput(float[] Predictions);


## 5. Configuration (`appsettings.json`)

The configuration will be structured to support all required settings.

json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/opc-client-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "MessageBus": {
    "Hostname": "rabbitmq.example.com",
    "Username": "user",
    "Password": "password",
    "DataExchange": "opc.data.exchange",
    "AlarmExchange": "opc.alarm.exchange"
  },
  "RemoteServices": {
    "ConfigurationServiceUrl": "http://management-service:5001"
  },
  "FeatureToggles": {
    "EnableEdgeAI": true,
    "EnableDataBuffering": true
  },
  "EdgeAi": {
    "ModelPath": "models/predictive_maintenance.onnx"
  },
  "DataBuffering": {
    "MaxQueueSizeMB": 1024,
    "FlushIntervalSeconds": 60
  },
  "OpcConnections": [
    {
      "ServerId": "Primary_Mixer_UA",
      "Protocol": "UA",
      "EndpointUrl": "opc.tcp://192.168.1.10:4840",
      "SecurityPolicy": "Basic256Sha256",
      "SecurityMode": "SignAndEncrypt",
      "Subscriptions": [
        {
          "PublishingInterval": 1000,
          "SamplingInterval": 500,
          "Tags": [ "ns=2;s=Mixer.Temperature", "ns=2;s=Mixer.Speed" ]
        }
      ]
    },
    {
      "ServerId": "History_Server_HDA",
      "Protocol": "HDA",
      "EndpointUrl": "opc.com://localhost/Matrikon.OPC.Simulation.1",
      "Tags": [ "Random.Int1", "Random.Real8" ]
    }
  ]
}


## 6. Error Handling and Resilience

-   **OPC Connection:** The `ConnectionManager` will implement an exponential backoff retry strategy for initial connection attempts. Active connections will be monitored, and if a connection drops, it will be automatically placed back into the retry loop.
-   **OPC UA Subscriptions:** The `SubscriptionManager` will monitor the state of each subscription. If a subscription enters an error state or a keep-alive fails, it will attempt to delete and recreate the subscription on the active session. If the session itself is lost, the `ConnectionManager`'s logic will take precedence.
-   **Message Bus Publishing:** The `RabbitMqPublisher` will wrap publish calls in a `try-catch` block. Upon failure (e.g., `BrokerUnreachableException`), the message payload will be passed to the `IDataBuffer` service. This ensures no data is lost during transient network partitions between the client and the message bus.
-   **Data Buffering:** The `FileBasedDataBuffer` will ensure durability by writing to disk. It will manage its own background worker to retry sending buffered messages, preventing the main data acquisition threads from being blocked. A cap on buffer size prevents uncontrolled disk growth.

## 7. Security
-   **OPC UA Communication:** The `OpcUaProtocolClient` will fully support the security policies defined in the configuration (e.g., `Basic256Sha256`). It will manage client certificates and trust server certificates based on standard validation procedures.
-   **Backend Communication:** All communication with the server-side application (gRPC, RabbitMQ) will be configured to use TLS to ensure data is encrypted in transit.
-   **Configuration:** Sensitive values in `appsettings.json` (e.g., passwords) will be managed using .NET's Secret Manager for development and environment variables or a secure vault (like Azure Key Vault) for production deployments.
-   **Edge Data:** Data buffered locally by the `FileBasedDataBuffer` will be stored in a protected directory with restricted file permissions. For high-security environments, application-level encryption of the buffer file could be added as a feature.