# Specification

# 1. Files

- **Path:** services/core-opc-client/services.core-opc-client.csproj  
**Description:** Defines the .NET project, specifying the target framework (.NET 8), and enumerating all NuGet package dependencies such as OPCFoundation.NetStandard.Opc.Ua, Grpc.Net.Client, RabbitMQ.Client, Microsoft.ML.OnnxRuntime, and Serilog.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** services.core-opc-client  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Dependency Management
    
**Requirement Ids:**
    
    - REQ-SAP-001
    
**Purpose:** To configure the project's build properties, framework version, and external library dependencies, enabling cross-platform compilation.  
**Logic Description:** This XML file will contain ItemGroup sections for PackageReference entries, listing all required third-party libraries. The TargetFramework property will be set to 'net8.0'. OutputType will be 'Exe' as this is a standalone service.  
**Documentation:**
    
    - **Summary:** The project file is the cornerstone for the build system, defining all necessary dependencies and settings for creating the cross-platform OPC client executable.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/core-opc-client/appsettings.json  
**Description:** JSON configuration file for the OPC client service. Contains connection strings for message queues, endpoint URLs for the server-side gRPC services, logging levels, and a list of OPC servers to connect to with their specific configurations.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - ExternalConfigurationStore
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Externalized Configuration
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-8-001
    
**Purpose:** To provide a flexible and environment-agnostic way to configure the service's operational parameters without requiring code changes.  
**Logic Description:** This file will have a structured JSON format. Top-level keys will include 'Serilog', 'MessageBus', 'RemoteServices', and 'OpcConnections'. The 'OpcConnections' key will be an array of objects, each defining a server's name, endpoint, protocol, security settings, and tags to monitor.  
**Documentation:**
    
    - **Summary:** Provides runtime configuration for the application, including service endpoints, connection details, and operational settings.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** services/core-opc-client/src/Program.cs  
**Description:** The main entry point of the application. Responsible for building the application host, configuring dependency injection for all services, setting up logging with Serilog, reading configuration from appsettings.json, and starting the main OpcClientHost hosted service.  
**Template:** C# Program  
**Dependency Level:** 3  
**Name:** Program  
**Type:** Application Entry Point  
**Relative Path:** Program  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** Task  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - Application Initialization
    - Dependency Injection Setup
    
**Requirement Ids:**
    
    - REQ-SAP-001
    
**Purpose:** To bootstrap the entire application, wire up all dependencies, and initiate the long-running service.  
**Logic Description:** Uses the WebApplication.CreateBuilder pattern to configure services. It will register all interfaces from the Domain layer with their concrete implementations from the Infrastructure layer (e.g., services.AddSingleton<IMessageBusPublisher, RabbitMqPublisher>). It also registers the OpcClientHost as a hosted service and then runs the application.  
**Documentation:**
    
    - **Summary:** Initializes and runs the core OPC client service, setting up all necessary configurations and service dependencies.
    
**Namespace:** services.opc.client  
**Metadata:**
    
    - **Category:** ApplicationHost
    
- **Path:** services/core-opc-client/src/Domain/Abstractions/IOpcProtocolClient.cs  
**Description:** Defines a common contract for all OPC protocol-specific clients (UA, DA, HDA, A&C). This abstraction allows the application layer to interact with different OPC servers in a protocol-agnostic manner for common operations.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IOpcProtocolClient  
**Type:** Interface  
**Relative Path:** Domain/Abstractions/IOpcProtocolClient  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - StrategyPattern
    - InterfaceSegregation
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ConnectAsync  
**Parameters:**
    
    - OpcConnectionSettings settings
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** DisconnectAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** ReadAsync  
**Parameters:**
    
    - IEnumerable<TagIdentifier> tags
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<DataPoint>>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-CSVC-003
    - REQ-CSVC-011
    - REQ-CSVC-017
    
**Purpose:** To establish a standardized interface for OPC communication, promoting loose coupling and enabling the use of different protocol strategies.  
**Logic Description:** This interface will define methods for connecting, disconnecting, reading data, writing data, browsing namespaces, and handling protocol-specific features like subscriptions or historical queries. Each method will be asynchronous.  
**Documentation:**
    
    - **Summary:** Provides a contract for interacting with an OPC server, abstracting the underlying communication protocol.
    
**Namespace:** services.opc.client.Domain.Abstractions  
**Metadata:**
    
    - **Category:** DomainLogic
    
- **Path:** services/core-opc-client/src/Domain/Abstractions/IMessageBusPublisher.cs  
**Description:** Defines the contract for publishing messages to the central message bus (e.g., RabbitMQ). This decouples the core application logic from the specific message queue technology being used.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IMessageBusPublisher  
**Type:** Interface  
**Relative Path:** Domain/Abstractions/IMessageBusPublisher  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** PublishDataPointAsync  
**Parameters:**
    
    - DataPoint dataPoint
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** PublishAlarmAsync  
**Parameters:**
    
    - AlarmEvent alarmEvent
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To provide a generic interface for sending data and events to the server-side application asynchronously.  
**Logic Description:** This interface will include generic methods for publishing different types of payloads, such as real-time data, alarms, or health status updates. Each method will accept a domain object and handle serialization internally.  
**Documentation:**
    
    - **Summary:** A contract for publishing messages to a message bus, ensuring that the application logic is independent of the messaging infrastructure.
    
**Namespace:** services.opc.client.Domain.Abstractions  
**Metadata:**
    
    - **Category:** Integration
    
- **Path:** services/core-opc-client/src/Domain/Abstractions/IEdgeAiRuntime.cs  
**Description:** Defines the contract for the AI model execution engine. This allows the application to run inference on models (e.g., ONNX) without being tightly coupled to a specific AI runtime library.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IEdgeAiRuntime  
**Type:** Interface  
**Relative Path:** Domain/Abstractions/IEdgeAiRuntime  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LoadModelAsync  
**Parameters:**
    
    - string modelPath
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** RunInferenceAsync  
**Parameters:**
    
    - ModelInput input
    
**Return Type:** Task<ModelOutput>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-8-001
    
**Purpose:** To abstract the functionality of loading and executing lightweight AI models for edge-based processing.  
**Logic Description:** This interface will define methods to load a model from a specified path and to run inference using that model. Input and output will be handled through generic or strongly-typed model classes.  
**Documentation:**
    
    - **Summary:** Provides a contract for running AI model inference, decoupling the application from the specifics of the ONNX runtime or other edge AI frameworks.
    
**Namespace:** services.opc.client.Domain.Abstractions  
**Metadata:**
    
    - **Category:** AI
    
- **Path:** services/core-opc-client/src/Infrastructure/OpcProtocolClients/Ua/OpcUaProtocolClient.cs  
**Description:** Concrete implementation of IOpcProtocolClient for OPC Unified Architecture (UA). It uses the OPCFoundation.NetStandard.Opc.Ua library to handle session management, security, subscriptions, and data access for OPC UA servers.  
**Template:** C# Service Template  
**Dependency Level:** 2  
**Name:** OpcUaProtocolClient  
**Type:** Service  
**Relative Path:** Infrastructure/OpcProtocolClients/Ua/OpcUaProtocolClient  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    - **Name:** _session  
**Type:** Session  
**Attributes:** private  
    - **Name:** _subscriptionManager  
**Type:** SubscriptionManager  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ConnectAsync  
**Parameters:**
    
    - OpcConnectionSettings settings
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public|override  
    - **Name:** ReadAsync  
**Parameters:**
    
    - IEnumerable<TagIdentifier> tags
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<DataPoint>>  
**Attributes:** public|override  
    - **Name:** CreateSubscriptionAsync  
**Parameters:**
    
    - SubscriptionSettings settings
    
**Return Type:** Task<string>  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC UA Connection
    - OPC UA Read/Write
    - OPC UA Subscriptions
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-CSVC-003
    - REQ-CSVC-023
    
**Purpose:** To handle all communication with OPC UA servers, including secure session establishment and data exchange.  
**Logic Description:** This class will implement the IOpcProtocolClient interface. The ConnectAsync method will use the Opc.Ua.Client.Session.Create method to establish a connection. ReadAsync will use the session's Read method. It will also instantiate and manage the SubscriptionManager for handling real-time data updates.  
**Documentation:**
    
    - **Summary:** Implements the client-side logic for communicating with OPC UA servers, managing sessions, subscriptions, and data operations as per the OPC UA specification.
    
**Namespace:** services.opc.client.Infrastructure.OpcProtocolClients.Ua  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/core-opc-client/src/Infrastructure/OpcProtocolClients/Ua/SubscriptionManager.cs  
**Description:** Manages OPC UA subscriptions for a given session. It handles the creation of subscriptions, adding monitored items, and processing data change notifications from the server. It deals with keep-alives and re-establishment logic.  
**Template:** C# Service Template  
**Dependency Level:** 2  
**Name:** SubscriptionManager  
**Type:** Service  
**Relative Path:** Infrastructure/OpcProtocolClients/Ua/SubscriptionManager  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - ObserverPattern
    
**Members:**
    
    - **Name:** _session  
**Type:** Session  
**Attributes:** private|readonly  
    - **Name:** _subscriptions  
**Type:** List<Subscription>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** CreateSubscription  
**Parameters:**
    
    - SubscriptionSettings settings
    
**Return Type:** Subscription  
**Attributes:** public  
    - **Name:** AddMonitoredItem  
**Parameters:**
    
    - Subscription subscription
    - NodeId nodeId
    
**Return Type:** MonitoredItem  
**Attributes:** public  
    - **Name:** OnNotification  
**Parameters:**
    
    - MonitoredItem monitoredItem
    - MonitoredItemNotificationEventArgs e
    
**Return Type:** void  
**Attributes:** private  
    
**Implemented Features:**
    
    - OPC UA Subscription Management
    
**Requirement Ids:**
    
    - REQ-CSVC-023
    
**Purpose:** To encapsulate the complexity of OPC UA's publish-subscribe model, providing a stable stream of real-time data.  
**Logic Description:** This class will create Subscription objects from the OPC UA stack. It will subscribe to the MonitoredItem's Notification event. When a notification is received, it will transform the event arguments into a domain `DataPoint` object and pass it to the `DataFlowOrchestrator` via an event or a direct call.  
**Documentation:**
    
    - **Summary:** Handles the lifecycle of OPC UA subscriptions, including creating subscriptions, adding items to monitor, and processing incoming data change notifications.
    
**Namespace:** services.opc.client.Infrastructure.OpcProtocolClients.Ua  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/core-opc-client/src/Infrastructure/OpcProtocolClients/Hda/OpcHdaProtocolClient.cs  
**Description:** Concrete implementation for OPC Historical Data Access (HDA). This class is responsible for connecting to OPC HDA servers and executing historical data queries, including raw and aggregated data retrieval.  
**Template:** C# Service Template  
**Dependency Level:** 2  
**Name:** OpcHdaProtocolClient  
**Type:** Service  
**Relative Path:** Infrastructure/OpcProtocolClients/Hda/OpcHdaProtocolClient  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** QueryHistoricalDataAsync  
**Parameters:**
    
    - HistoricalDataRequest request
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<DataPoint>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC HDA Integration
    
**Requirement Ids:**
    
    - REQ-CSVC-011
    
**Purpose:** To enable the retrieval of historical process data from compliant OPC HDA servers.  
**Logic Description:** This class will implement a specific interface for historical access. It will use a suitable OPC Classic library that supports HDA or a bridge to communicate with HDA servers. The `QueryHistoricalDataAsync` method will construct the query parameters (time range, tags, aggregation) and execute the read on the server.  
**Documentation:**
    
    - **Summary:** Implements the client-side logic for connecting to and querying data from OPC HDA servers.
    
**Namespace:** services.opc.client.Infrastructure.OpcProtocolClients.Hda  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/core-opc-client/src/Infrastructure/OpcProtocolClients/Ac/OpcAcProtocolClient.cs  
**Description:** Concrete implementation for OPC Alarms & Conditions (A&C). This class connects to OPC A&C servers, subscribes to alarm and event notifications, and provides methods for acknowledging alarms.  
**Template:** C# Service Template  
**Dependency Level:** 2  
**Name:** OpcAcProtocolClient  
**Type:** Service  
**Relative Path:** Infrastructure/OpcProtocolClients/Ac/OpcAcProtocolClient  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SubscribeToAlarmsAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** AcknowledgeAlarmAsync  
**Parameters:**
    
    - AlarmAcknowledgeRequest request
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC A&C Integration
    
**Requirement Ids:**
    
    - REQ-CSVC-017
    
**Purpose:** To provide real-time alarm and event monitoring capabilities by integrating with OPC A&C servers.  
**Logic Description:** This class will use a suitable OPC Classic library with A&C support. It will establish a connection, create an event subscription, and handle incoming alarm events by converting them to the domain's `AlarmEvent` model and publishing them via the `IMessageBusPublisher`.  
**Documentation:**
    
    - **Summary:** Implements the client-side logic for receiving, processing, and acknowledging alarms from OPC A&C servers.
    
**Namespace:** services.opc.client.Infrastructure.OpcProtocolClients.Ac  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/core-opc-client/src/Infrastructure/EdgeProcessing/OnnxAiRuntime.cs  
**Description:** Concrete implementation of IEdgeAiRuntime. This class uses the Microsoft.ML.OnnxRuntime library to load and execute lightweight AI models in the ONNX format directly within the client service process, enabling low-latency edge AI.  
**Template:** C# Service Template  
**Dependency Level:** 2  
**Name:** OnnxAiRuntime  
**Type:** Service  
**Relative Path:** Infrastructure/EdgeProcessing/OnnxAiRuntime  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _inferenceSession  
**Type:** InferenceSession  
**Attributes:** private  
    
**Methods:**
    
    - **Name:** LoadModelAsync  
**Parameters:**
    
    - string modelPath
    
**Return Type:** Task  
**Attributes:** public|override  
    - **Name:** RunInferenceAsync  
**Parameters:**
    
    - ModelInput input
    
**Return Type:** Task<ModelOutput>  
**Attributes:** public|override  
    
**Implemented Features:**
    
    - Edge AI Model Execution
    - ONNX Runtime Integration
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-8-001
    
**Purpose:** To provide local, low-latency AI inference capabilities for tasks like predictive maintenance and anomaly detection.  
**Logic Description:** Implements the IEdgeAiRuntime interface. The LoadModelAsync method creates an InferenceSession from a .onnx file. The RunInferenceAsync method takes input data, converts it to the format expected by the model (e.g., Tensors), runs the session, and processes the output Tensors back into a structured result.  
**Documentation:**
    
    - **Summary:** Handles the loading and execution of ONNX-formatted AI models for real-time inference on edge devices.
    
**Namespace:** services.opc.client.Infrastructure.EdgeProcessing  
**Metadata:**
    
    - **Category:** AI
    
- **Path:** services/core-opc-client/src/Application/Orchestration/ConnectionManager.cs  
**Description:** A core service responsible for managing the lifecycle of all configured OPC server connections. It reads connection settings, creates the appropriate protocol client using a factory, and handles connection/disconnection and health monitoring.  
**Template:** C# Service Template  
**Dependency Level:** 3  
**Name:** ConnectionManager  
**Type:** Service  
**Relative Path:** Application/Orchestration/ConnectionManager  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - FactoryPattern
    - Manager
    
**Members:**
    
    - **Name:** _connections  
**Type:** Dictionary<string, IOpcProtocolClient>  
**Attributes:** private|readonly  
    - **Name:** _clientFactory  
**Type:** OpcClientFactory  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** InitializeConnectionsAsync  
**Parameters:**
    
    - IEnumerable<OpcConnectionSettings> settings
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** MonitorConnectionHealth  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** private  
    
**Implemented Features:**
    
    - OPC Connection Lifecycle Management
    
**Requirement Ids:**
    
    - REQ-SAP-001
    - REQ-SAP-015
    
**Purpose:** To act as the central coordinator for all outbound OPC server connections, ensuring they are established and maintained correctly.  
**Logic Description:** This service iterates through connection settings provided by the configuration. For each setting, it uses a factory to get the correct IOpcProtocolClient implementation and calls its ConnectAsync method. It maintains a dictionary of active connections and periodically checks their status.  
**Documentation:**
    
    - **Summary:** Manages the lifecycle of OPC server connections, including creation, monitoring, and termination based on application configuration.
    
**Namespace:** services.opc.client.Application.Orchestration  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/core-opc-client/src/Application/HostedServices/OpcClientHost.cs  
**Description:** The main IHostedService implementation. Its StartAsync method triggers the ConnectionManager to initialize all OPC connections, and its StopAsync method ensures a graceful shutdown of all connections and services.  
**Template:** C# Hosted Service  
**Dependency Level:** 4  
**Name:** OpcClientHost  
**Type:** Service  
**Relative Path:** Application/HostedServices/OpcClientHost  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _connectionManager  
**Type:** ConnectionManager  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** StartAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** StopAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Application Lifecycle Management
    
**Requirement Ids:**
    
    - REQ-SAP-001
    
**Purpose:** To manage the top-level start and stop logic for the entire OPC client service, integrating it with the .NET generic host.  
**Logic Description:** Implements the IHostedService interface. In StartAsync, it will fetch the initial configuration and pass it to the ConnectionManager to start all connections. In StopAsync, it will call a corresponding shutdown method on the ConnectionManager.  
**Documentation:**
    
    - **Summary:** Acts as the primary background service that controls the startup and shutdown of the OPC client's operations.
    
**Namespace:** services.opc.client.Application.HostedServices  
**Metadata:**
    
    - **Category:** ApplicationHost
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableEdgeAI
  - EnableDataBuffering
  - EnableUaProtocol
  - EnableDaProtocol
  - EnableHdaProtocol
  - EnableAcProtocol
  
- **Database Configs:**
  
  


---

