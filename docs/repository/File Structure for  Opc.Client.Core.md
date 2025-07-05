# Specification

# 1. Files

- **Path:** src/Opc.Client.Core/Opc.Client.Core.csproj  
**Description:** The .NET 8 project file for the Core OPC Client library. It defines the target framework, dependencies like OPCFoundation.NetStandard.Opc.Ua, Grpc.Net.Client, RabbitMQ.Client, Serilog, and ONNX Runtime.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** Opc.Client.Core  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Definition
    
**Requirement Ids:**
    
    - REQ-SAP-001
    
**Purpose:** Defines the project structure, dependencies, and build settings for the core OPC client service.  
**Logic Description:** This file will list all necessary NuGet package references for OPC communication, gRPC, message queues, logging, and AI model execution. It will also specify the target framework as net8.0 and enable properties for generating a cross-platform executable or library.  
**Documentation:**
    
    - **Summary:** Specifies project-level configurations and external library dependencies required to build the OPC Client Core service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Opc.Client.Core/Domain/Aggregates/ServerConnection.cs  
**Description:** Represents a connection to a single OPC Server. This aggregate root manages its own state (e.g., Connected, Disconnected), configuration, and holds collections of associated tags and subscriptions.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** ServerConnection  
**Type:** AggregateRoot  
**Relative Path:** Domain/Aggregates  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Aggregate
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public|readonly  
    - **Name:** Configuration  
**Type:** ServerConfiguration  
**Attributes:** public|readonly  
    - **Name:** Status  
**Type:** ServerStatus  
**Attributes:** public|private set  
    - **Name:** _tags  
**Type:** IReadOnlyDictionary<NodeId, Tag>  
**Attributes:** private  
    - **Name:** _subscriptions  
**Type:** IReadOnlyDictionary<Guid, Subscription>  
**Attributes:** private  
    
**Methods:**
    
    - **Name:** Connect  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Disconnect  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** AddSubscription  
**Parameters:**
    
    - Subscription subscription
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UpdateStatus  
**Parameters:**
    
    - ServerStatus newStatus
    - string reason
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Connection State Management
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-CSVC-026
    
**Purpose:** To encapsulate the state and behavior of a connection to an OPC server, ensuring all operations are consistent with the current connection state.  
**Logic Description:** Contains methods to initiate connection and disconnection, which raise domain events. Manages collections of associated tags and subscriptions, ensuring their lifecycle is tied to the server connection. The UpdateStatus method will be central to failover and reconnect logic, raising events for status changes.  
**Documentation:**
    
    - **Summary:** The ServerConnection aggregate root. It models an OPC server connection, its configuration, and its lifecycle.
    
**Namespace:** Opc.Client.Core.Domain.Aggregates  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Domain/ValueObjects/TagValue.cs  
**Description:** An immutable value object representing the value, quality, and timestamp of an OPC tag reading.  
**Template:** C# Record  
**Dependency Level:** 0  
**Name:** TagValue  
**Type:** ValueObject  
**Relative Path:** Domain/ValueObjects  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - ValueObject
    
**Members:**
    
    - **Name:** Value  
**Type:** object  
**Attributes:** public|init  
    - **Name:** Quality  
**Type:** OpcQuality  
**Attributes:** public|init  
    - **Name:** Timestamp  
**Type:** DateTimeOffset  
**Attributes:** public|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Data Value Representation
    
**Requirement Ids:**
    
    - REQ-CSVC-003
    
**Purpose:** To provide a strongly-typed, immutable representation of a data point from an OPC server, preventing primitive obsession and ensuring data consistency.  
**Logic Description:** Implemented as a C# record for value-based equality and immutability out of the box. This ensures that two TagValue objects are considered equal if their value, quality, and timestamp are the same. No business logic is needed here.  
**Documentation:**
    
    - **Summary:** Represents a snapshot of an OPC tag's data, including its value, quality, and the time of measurement.
    
**Namespace:** Opc.Client.Core.Domain.ValueObjects  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Application/Interfaces/IOpcProtocolClient.cs  
**Description:** Defines a common interface for interacting with different OPC protocol specifications (UA, DA, HDA, etc.), abstracting the specific implementation details.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IOpcProtocolClient  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Strategy
    - Adapter
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ConnectAsync  
**Parameters:**
    
    - ServerConfiguration config
    - CancellationToken cancellationToken
    
**Return Type:** Task<ServerStatus>  
**Attributes:** public  
    - **Name:** DisconnectAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** ReadAsync  
**Parameters:**
    
    - IEnumerable<NodeId> nodeIds
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<TagValue>>  
**Attributes:** public  
    - **Name:** WriteAsync  
**Parameters:**
    
    - IDictionary<NodeId, object> values
    - CancellationToken cancellationToken
    
**Return Type:** Task<WriteResult>  
**Attributes:** public  
    - **Name:** BrowseAsync  
**Parameters:**
    
    - NodeId nodeId
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<NodeInfo>>  
**Attributes:** public  
    - **Name:** CreateSubscriptionAsync  
**Parameters:**
    
    - SubscriptionParameters parameters
    - Action<DataChangeNotification> onNotification
    - CancellationToken cancellationToken
    
**Return Type:** Task<Guid>  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC Communication Abstraction
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-CSVC-002
    - REQ-CSVC-003
    - REQ-CSVC-004
    - REQ-CSVC-023
    
**Purpose:** To decouple the application logic from the concrete OPC SDKs and protocol implementations, allowing for easier testing and maintenance.  
**Logic Description:** This interface defines all the fundamental operations required for OPC communication, such as connect, disconnect, read, write, browse, and subscribe. Each method will be asynchronous to support non-blocking I/O operations. Implementations for UA, DA, HDA, etc., will adhere to this contract.  
**Documentation:**
    
    - **Summary:** Provides a unified contract for all OPC protocol clients, abstracting away the specific details of each standard.
    
**Namespace:** Opc.Client.Core.Application.Interfaces  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Application/Interfaces/IServerEventPublisher.cs  
**Description:** Defines the contract for publishing events (e.g., data changes, alarms, health status) to the centralized server-side application via a message queue.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IServerEventPublisher  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** PublishDataChangeAsync  
**Parameters:**
    
    - DataChangeNotification notification
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** PublishAlarmAsync  
**Parameters:**
    
    - AlarmEventNotification notification
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** PublishHealthStatusAsync  
**Parameters:**
    
    - ClientHealthStatus status
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** PublishCriticalWriteLogAsync  
**Parameters:**
    
    - CriticalWriteLog log
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Central Server Communication
    
**Requirement Ids:**
    
    - REQ-SAP-003
    - REQ-CSVC-009
    - REQ-CSVC-018
    - REQ-CSVC-028
    
**Purpose:** To abstract the mechanism of sending asynchronous events to the backend, decoupling the core logic from specific message broker technologies like RabbitMQ or Kafka.  
**Logic Description:** This interface defines methods for each type of event the OPC client needs to send to the server. Implementations will handle message serialization (e.g., to JSON) and the specifics of publishing to the configured message broker topic or exchange.  
**Documentation:**
    
    - **Summary:** Provides a contract for publishing events from the OPC client to the centralized server application.
    
**Namespace:** Opc.Client.Core.Application.Interfaces  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Application/Interfaces/IAiModelRunner.cs  
**Description:** Defines the contract for executing a lightweight AI model on the edge device.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IAiModelRunner  
**Type:** Interface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LoadModelAsync  
**Parameters:**
    
    - string modelPath
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** RunInferenceAsync  
**Parameters:**
    
    - ModelInputData inputData
    
**Return Type:** Task<ModelOutputData>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Edge AI Execution Abstraction
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-8-001
    
**Purpose:** To abstract the specifics of the AI model execution runtime (e.g., ONNX Runtime), allowing the application to interact with a simple, high-level interface for running inferences.  
**Logic Description:** The interface provides methods to load a model from a given path and to run an inference by passing structured input data. This decouples the core application logic from the underlying complexities of tensor manipulation and session management in AI runtimes.  
**Documentation:**
    
    - **Summary:** Provides a contract for loading and executing AI models on an edge device.
    
**Namespace:** Opc.Client.Core.Application.Interfaces  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Infrastructure/Opc/Protocols/OpcUaClient.cs  
**Description:** Implements the IOpcProtocolClient interface for the OPC Unified Architecture (UA) specification. Handles secure channel establishment, session management, subscriptions, and data access using the OPC Foundation .NET Standard stack.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** OpcUaClient  
**Type:** Client  
**Relative Path:** Infrastructure/Opc/Protocols  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    - **Name:** _session  
**Type:** Session  
**Attributes:** private  
    - **Name:** _subscriptionManager  
**Type:** UaSubscriptionManager  
**Attributes:** private|readonly  
    - **Name:** _securityHandler  
**Type:** UaSecurityHandler  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ConnectAsync  
**Parameters:**
    
    - ServerConfiguration config
    - CancellationToken cancellationToken
    
**Return Type:** Task<ServerStatus>  
**Attributes:** public  
    - **Name:** WriteAsync  
**Parameters:**
    
    - IDictionary<NodeId, object> values
    - CancellationToken cancellationToken
    
**Return Type:** Task<WriteResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC UA Communication
    - OPC UA Security
    - OPC UA Subscriptions
    
**Requirement Ids:**
    
    - REQ-CSVC-001
    - REQ-3-001
    - REQ-CSVC-023
    - REQ-DLP-014
    
**Purpose:** To provide a concrete implementation for all OPC UA communications, encapsulating the complexity of the underlying OPC Foundation SDK.  
**Logic Description:** This class will manage the lifecycle of an OPC UA Session. The ConnectAsync method will handle certificate validation (REQ-3-001) and session creation. Read/Write methods will translate domain objects to/from the SDK's DataValue types. It will delegate subscription management to a dedicated UaSubscriptionManager class, which in turn will use the SubscriptionBuffer for reconnect logic (REQ-CSVC-026). Will leverage OPC UA binary encoding for compression (REQ-DLP-014).  
**Documentation:**
    
    - **Summary:** A concrete client for interacting with OPC UA servers. Implements the common IOpcProtocolClient interface.
    
**Namespace:** Opc.Client.Core.Infrastructure.Opc.Protocols  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Opc.Client.Core/Infrastructure/Opc/Protocols/OpcHdaClient.cs  
**Description:** Implements the necessary parts of an OPC Historical Data Access client. This might be part of the OpcUaClient for UA-based HDA or a separate class for Classic HDA.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** OpcHdaClient  
**Type:** Client  
**Relative Path:** Infrastructure/Opc/Protocols  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    
**Methods:**
    
    - **Name:** QueryHistoricalDataAsync  
**Parameters:**
    
    - HistoricalDataQuery query
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<TagValue>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC HDA Communication
    
**Requirement Ids:**
    
    - REQ-CSVC-011
    - REQ-CSVC-012
    - REQ-CSVC-014
    - REQ-CSVC-015
    
**Purpose:** To provide a concrete implementation for querying historical data from OPC HDA compliant servers.  
**Logic Description:** This class will translate a domain-specific historical query object into the appropriate calls for the underlying OPC HDA specification (either via UA services or a Classic HDA wrapper). It will handle requesting raw data, as well as processed/aggregated data (e.g., Average, Min, Max), and manage error responses from the server.  
**Documentation:**
    
    - **Summary:** A concrete client for querying historical data from OPC HDA servers.
    
**Namespace:** Opc.Client.Core.Infrastructure.Opc.Protocols  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Opc.Client.Core/Infrastructure/Opc/Protocols/OpcAcClient.cs  
**Description:** Implements the client-side logic for OPC Alarms & Conditions (A&C). This class will handle subscriptions to alarm events and the acknowledgement of alarms.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** OpcAcClient  
**Type:** Client  
**Relative Path:** Infrastructure/Opc/Protocols  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SubscribeToAlarmsAsync  
**Parameters:**
    
    - Action<AlarmEvent> onAlarm
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** AcknowledgeAlarmAsync  
**Parameters:**
    
    - AcknowledgeInfo ackInfo
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - OPC A&C Communication
    
**Requirement Ids:**
    
    - REQ-CSVC-017
    - REQ-CSVC-018
    - REQ-CSVC-020
    
**Purpose:** To provide a concrete implementation for receiving and interacting with alarms from OPC A&C compliant servers.  
**Logic Description:** This class will manage subscriptions to alarm and event notifiers on an OPC UA server. When an alarm event is received, it will be mapped to a domain `AlarmEvent` object and passed to a callback. The `AcknowledgeAlarmAsync` method will execute the `Acknowledge` method call on the server for a given alarm condition.  
**Documentation:**
    
    - **Summary:** A concrete client for receiving and acknowledging alarms and conditions from OPC A&C servers.
    
**Namespace:** Opc.Client.Core.Infrastructure.Opc.Protocols  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Opc.Client.Core/Infrastructure/Opc/SubscriptionBuffer.cs  
**Description:** Implements the local data buffering logic required for OPC UA subscriptions. It queues data during short network interruptions and ensures it's delivered upon successful reconnection.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** SubscriptionBuffer  
**Type:** Service  
**Relative Path:** Infrastructure/Opc  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Buffer
    
**Members:**
    
    - **Name:** _buffer  
**Type:** ConcurrentQueue<DataChangeNotification>  
**Attributes:** private|readonly  
    - **Name:** _bufferSizeLimit  
**Type:** int  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Enqueue  
**Parameters:**
    
    - DataChangeNotification notification
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** DrainAndPublishAsync  
**Parameters:**
    
    - IServerEventPublisher publisher
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** Clear  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Subscription Data Buffering
    - Data Loss Prevention
    
**Requirement Ids:**
    
    - REQ-CSVC-026
    
**Purpose:** To prevent data loss from OPC UA subscriptions during brief periods of disconnection from the central server application.  
**Logic Description:** This class will use a thread-safe queue to store incoming `DataChangeNotification` objects when the client is unable to publish them to the central server. It will have a configurable size limit to prevent unbounded memory growth. Upon reconnection, the `DrainAndPublishAsync` method will be called to send all buffered data in order.  
**Documentation:**
    
    - **Summary:** Provides a temporary, in-memory buffer for OPC UA subscription data changes during network outages.
    
**Namespace:** Opc.Client.Core.Infrastructure.Opc  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Opc.Client.Core/Infrastructure/ServerComms/ServerAppRabbitMqProducer.cs  
**Description:** Implements the IServerEventPublisher interface using RabbitMQ. It handles connecting to the RabbitMQ broker, declaring exchanges/queues, and publishing serialized messages.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** ServerAppRabbitMqProducer  
**Type:** Client  
**Relative Path:** Infrastructure/ServerComms  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Producer
    
**Members:**
    
    - **Name:** _connection  
**Type:** IConnection  
**Attributes:** private  
    - **Name:** _channel  
**Type:** IModel  
**Attributes:** private  
    
**Methods:**
    
    - **Name:** PublishDataChangeAsync  
**Parameters:**
    
    - DataChangeNotification notification
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Asynchronous Event Publishing
    
**Requirement Ids:**
    
    - REQ-SAP-003
    
**Purpose:** To send event data asynchronously from the OPC client to the central server application using RabbitMQ.  
**Logic Description:** This class will use the RabbitMQ.Client library to establish a connection to the message broker. Each `Publish...Async` method will serialize the corresponding DTO to JSON, then publish it to a pre-configured exchange with a specific routing key. It will implement connection and channel management, including retry logic for resilience.  
**Documentation:**
    
    - **Summary:** A concrete implementation of IServerEventPublisher that sends messages to a RabbitMQ message broker.
    
**Namespace:** Opc.Client.Core.Infrastructure.ServerComms  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Opc.Client.Core/Infrastructure/Ai/OnnxAiModelRunner.cs  
**Description:** Implements the IAiModelRunner interface using the ONNX Runtime. This class is responsible for loading an ONNX model file and running inference sessions.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** OnnxAiModelRunner  
**Type:** Service  
**Relative Path:** Infrastructure/Ai  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    - **Name:** _inferenceSession  
**Type:** InferenceSession  
**Attributes:** private  
    
**Methods:**
    
    - **Name:** LoadModelAsync  
**Parameters:**
    
    - string modelPath
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** RunInferenceAsync  
**Parameters:**
    
    - ModelInputData inputData
    
**Return Type:** Task<ModelOutputData>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Edge AI Inference
    
**Requirement Ids:**
    
    - REQ-7-001
    - REQ-8-001
    
**Purpose:** To execute pre-trained AI models in the ONNX format locally on the edge device where the OPC client is running.  
**Logic Description:** This class will use the Microsoft.ML.OnnxRuntime library. `LoadModelAsync` will create an `InferenceSession` from the given model file path. `RunInferenceAsync` will take domain-specific input data, convert it into the required `NamedOnnxValue` tensor format, execute the session, and map the output tensors back into a domain-specific output object.  
**Documentation:**
    
    - **Summary:** A concrete implementation of IAiModelRunner that uses the ONNX Runtime for model execution.
    
**Namespace:** Opc.Client.Core.Infrastructure.Ai  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Opc.Client.Core/Application/Features/Data/WriteTagsCommandHandler.cs  
**Description:** Handles the command to write values to OPC tags. This is the central point for orchestrating write operations, including validation, rate limiting, and critical write logging.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** WriteTagsCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/Features/Data  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - CQRS
    - Decorator
    
**Members:**
    
    - **Name:** _clientManager  
**Type:** IOpcClientManager  
**Attributes:** private|readonly  
    - **Name:** _validator  
**Type:** IWriteValidator  
**Attributes:** private|readonly  
    - **Name:** _rateLimiter  
**Type:** IWriteRateLimiter  
**Attributes:** private|readonly  
    - **Name:** _criticalLogger  
**Type:** ICriticalWriteAuditor  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - WriteTagsCommand command
    - CancellationToken cancellationToken
    
**Return Type:** Task<WriteResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Tag Writing
    - Client-Side Validation
    - Write Rate Limiting
    - Critical Write Auditing
    
**Requirement Ids:**
    
    - REQ-CSVC-004
    - REQ-CSVC-005
    - REQ-CSVC-006
    - REQ-CSVC-007
    - REQ-CSVC-009
    - REQ-CSVC-010
    
**Purpose:** To provide a robust, secure, and controlled mechanism for writing data to OPC servers, enforcing business rules before communication.  
**Logic Description:** The handler will first check if the write operation is permitted by the rate limiter. Then, it will validate the data against configured rules (e.g., range checks). If the tag is marked as critical, it will pre-fetch the old value for logging. It will then delegate the actual write to the appropriate IOpcProtocolClient via the manager. Finally, it will log the critical write details asynchronously.  
**Documentation:**
    
    - **Summary:** Orchestrates the entire process of writing tag values, including validation, limiting, and auditing.
    
**Namespace:** Opc.Client.Core.Application.Features.Data  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Opc.Client.Core/Application/Features/Configuration/ImportTagsCommand.cs  
**Description:** A command to import tag configurations from a file (e.g., CSV, XML).  
**Template:** C# Record  
**Dependency Level:** 2  
**Name:** ImportTagsCommand  
**Type:** Command  
**Relative Path:** Application/Features/Configuration  
**Repository Id:** REPO-SAP-001  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** FilePath  
**Type:** string  
**Attributes:** public|init  
    - **Name:** FileFormat  
**Type:** FileFormat  
**Attributes:** public|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Tag Configuration Import
    
**Requirement Ids:**
    
    - REQ-CSVC-008
    
**Purpose:** To encapsulate the request to import a list of OPC tags from an external file, triggering the import process.  
**Logic Description:** This is a simple data-carrying record. The handler for this command will use a factory to get the correct file parser based on the `FileFormat` enum, parse the file, validate the contents, and then update the client's tag configuration.  
**Documentation:**
    
    - **Summary:** Represents a use case for importing tag configurations from a file.
    
**Namespace:** Opc.Client.Core.Application.Features.Configuration  
**Metadata:**
    
    - **Category:** BusinessLogic
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableEdgeAiProcessing
  - EnableClassicOpcDaSupport
  - EnableRabbitMqPublishing
  - EnableGrpcComms
  
- **Database Configs:**
  
  


---

