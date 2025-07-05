# Specification

# 1. Files

- **Path:** src/Services/Integration/Integration.sln  
**Description:** Visual Studio Solution file for the Integration Service, linking all its projects.  
**Template:** .NET Solution  
**Dependency Level:** 0  
**Name:** Integration  
**Type:** Solution  
**Relative Path:**   
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the projects that constitute the Integration microservice.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** The root solution file that groups the API, Application, Domain, and Infrastructure projects for the Integration service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Integration/Integration.Domain/Integration.Domain.csproj  
**Description:** .NET project file for the Domain layer, containing core business logic and models. It has no dependencies on other layers.  
**Template:** .NET Class Library  
**Dependency Level:** 0  
**Name:** Integration.Domain  
**Type:** Project  
**Relative Path:** Integration.Domain  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DomainModel
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To define the core, technology-agnostic business entities, value objects, and rules for external system integrations.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Contains the project definition for the Domain layer, listing its dependencies and build settings.
    
**Namespace:** Opc.System.Services.Integration.Domain  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Integration/Integration.Domain/Aggregates/IntegrationConnection.cs  
**Description:** Domain entity representing a configured connection to an external system like an IoT platform, blockchain, or digital twin.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** IntegrationConnection  
**Type:** Entity  
**Relative Path:** Integration.Domain/Aggregates  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - AggregateRoot
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** ConnectionType  
**Type:** ConnectionType  
**Attributes:** public  
    - **Name:** Endpoint  
**Type:** string  
**Attributes:** public  
    - **Name:** IsEnabled  
**Type:** bool  
**Attributes:** public  
    - **Name:** SecurityConfiguration  
**Type:** JsonDocument  
**Attributes:** public  
    - **Name:** DataMapId  
**Type:** Guid?  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** Enable  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Disable  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UpdateConfiguration  
**Parameters:**
    
    - string name
    - string endpoint
    - JsonDocument securityConfig
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - External Connection Management
    
**Requirement Ids:**
    
    - REQ-8-004
    - REQ-8-006
    - REQ-8-007
    - REQ-8-010
    
**Purpose:** Encapsulates the state and behavior of a single, configured integration endpoint.  
**Logic Description:** Contains properties for connection details. Implements methods to manage the connection's lifecycle, such as enabling, disabling, and updating its configuration, ensuring business rules are enforced.  
**Documentation:**
    
    - **Summary:** This class is the aggregate root for an integration connection, managing its properties and state transitions.
    
**Namespace:** Opc.System.Services.Integration.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Integration/Integration.Domain/Aggregates/DataMap.cs  
**Description:** Domain entity representing a data transformation map between the internal OPC data model and an external system's schema.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** DataMap  
**Type:** Entity  
**Relative Path:** Integration.Domain/Aggregates  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - AggregateRoot
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** SourceModelDefinition  
**Type:** JsonDocument  
**Attributes:** public  
    - **Name:** TargetModelDefinition  
**Type:** JsonDocument  
**Attributes:** public  
    - **Name:** TransformationRules  
**Type:** JsonDocument  
**Attributes:** public  
    - **Name:** Version  
**Type:** int  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Data Transformation Rule Management
    
**Requirement Ids:**
    
    - REQ-8-005
    - REQ-8-010
    
**Purpose:** Defines the rules and schema for transforming data for a specific integration.  
**Logic Description:** This class holds the schema definitions and transformation logic as JSON, allowing for flexible and configurable data mapping without requiring code changes for new or updated maps.  
**Documentation:**
    
    - **Summary:** Represents a data mapping configuration, including source and target schemas and the transformation rules between them.
    
**Namespace:** Opc.System.Services.Integration.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Integration/Integration.Domain/Enums/ConnectionType.cs  
**Description:** Enumeration for the different types of supported external integrations.  
**Template:** C# Enum  
**Dependency Level:** 1  
**Name:** ConnectionType  
**Type:** Enum  
**Relative Path:** Integration.Domain/Enums  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** AzureIotHub  
**Type:** enum  
**Attributes:**   
    - **Name:** AwsIotCore  
**Type:** enum  
**Attributes:**   
    - **Name:** GoogleCloudIot  
**Type:** enum  
**Attributes:**   
    - **Name:** AugmentedRealityStream  
**Type:** enum  
**Attributes:**   
    - **Name:** BlockchainLedger  
**Type:** enum  
**Attributes:**   
    - **Name:** DigitalTwin  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-004
    - REQ-8-006
    - REQ-8-007
    - REQ-8-010
    
**Purpose:** Provides a strongly-typed identifier for the kind of external system a connection points to.  
**Logic Description:** A simple enum to categorize integration connections, facilitating logic branching and configuration.  
**Documentation:**
    
    - **Summary:** Defines the possible types for an IntegrationConnection.
    
**Namespace:** Opc.System.Services.Integration.Domain.Enums  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Integration/Integration.Application/Integration.Application.csproj  
**Description:** .NET project file for the Application layer. It orchestrates domain logic to fulfill use cases.  
**Template:** .NET Class Library  
**Dependency Level:** 1  
**Name:** Integration.Application  
**Type:** Project  
**Relative Path:** Integration.Application  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Contains the business use cases of the microservice, decoupled from UI and infrastructure concerns.  
**Logic Description:** References the Domain project. Defines commands, queries, and their handlers. Defines interfaces for infrastructure dependencies.  
**Documentation:**
    
    - **Summary:** The project file for the application layer, which connects the API layer to the domain.
    
**Namespace:** Opc.System.Services.Integration.Application  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Contracts/External/IIotPlatformConnector.cs  
**Description:** Defines the contract for connecting and sending data to any supported IoT platform.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IIotPlatformConnector  
**Type:** Interface  
**Relative Path:** Integration.Application/Contracts/External  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SendDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string payload
    
**Return Type:** Task<bool>  
**Attributes:**   
    - **Name:** ReceiveDataAsync  
**Parameters:**
    
    - Guid connectionId
    - Func<string, Task> onMessageReceived
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-004
    - REQ-8-005
    
**Purpose:** To abstract the specific communication protocol (MQTT, AMQP) of an IoT platform from the application logic.  
**Logic Description:** The interface provides generic methods for sending and receiving data, allowing the application layer to remain ignorant of the underlying technology.  
**Documentation:**
    
    - **Summary:** An interface for IoT platform communication, enabling bi-directional data flow.
    
**Namespace:** Opc.System.Services.Integration.Application.Contracts.External  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Contracts/External/IBlockchainAdapter.cs  
**Description:** Defines the contract for interacting with a private permissioned blockchain.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IBlockchainAdapter  
**Type:** Interface  
**Relative Path:** Integration.Application/Contracts/External  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogCriticalDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string dataPayload
    
**Return Type:** Task<string>  
**Attributes:**   
    - **Name:** VerifyTransactionAsync  
**Parameters:**
    
    - Guid connectionId
    - string transactionId
    
**Return Type:** Task<bool>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-007
    - REQ-8-008
    
**Purpose:** To abstract the specifics of blockchain interaction, such as smart contract calls and transaction verification.  
**Logic Description:** Provides methods to log critical data, which will internally hash the data and call a smart contract, and to verify an existing transaction.  
**Documentation:**
    
    - **Summary:** An interface for logging data to and verifying data from a blockchain ledger.
    
**Namespace:** Opc.System.Services.Integration.Application.Contracts.External  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Contracts/External/IDigitalTwinAdapter.cs  
**Description:** Defines the contract for interacting with a digital twin platform.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IDigitalTwinAdapter  
**Type:** Interface  
**Relative Path:** Integration.Application/Contracts/External  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SendDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string twinId
    - string payload
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** ReceiveCommandAsync  
**Parameters:**
    
    - Guid connectionId
    - string twinId
    
**Return Type:** Task<string>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-010
    - REQ-8-011
    
**Purpose:** To abstract the protocol (AAS, DTDL) used for bi-directional communication with a digital twin.  
**Logic Description:** Provides methods to send data to a specific twin and receive commands from it, hiding the complexity of the underlying digital twin SDK.  
**Documentation:**
    
    - **Summary:** An interface for bi-directional data exchange with digital twin platforms.
    
**Namespace:** Opc.System.Services.Integration.Application.Contracts.External  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Contracts/External/IArDataStreamer.cs  
**Description:** Defines the contract for streaming real-time data to connected AR devices.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IArDataStreamer  
**Type:** Interface  
**Relative Path:** Integration.Application/Contracts/External  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** StreamDataToDeviceAsync  
**Parameters:**
    
    - string deviceId
    - string payload
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-006
    
**Purpose:** To abstract the real-time streaming mechanism, likely WebSockets, for sending data to AR applications.  
**Logic Description:** A simple interface with one method to push a data payload to a specific connected AR device.  
**Documentation:**
    
    - **Summary:** An interface for streaming data to Augmented Reality devices.
    
**Namespace:** Opc.System.Services.Integration.Application.Contracts.External  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Contracts/Infrastructure/IDataTransformer.cs  
**Description:** Defines the contract for the data transformation service.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IDataTransformer  
**Type:** Interface  
**Relative Path:** Integration.Application/Contracts/Infrastructure  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** TransformAsync  
**Parameters:**
    
    - Guid dataMapId
    - string sourcePayload
    
**Return Type:** Task<string>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-8-005
    
**Purpose:** To provide a generic interface for transforming a source data payload into a target format based on a specified data map.  
**Logic Description:** This interface decouples the application layer from the concrete implementation of the data transformation engine.  
**Documentation:**
    
    - **Summary:** An interface for performing data transformations using a predefined DataMap.
    
**Namespace:** Opc.System.Services.Integration.Application.Contracts.Infrastructure  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Application/Features/BlockchainLogging/Commands/LogCriticalDataCommandHandler.cs  
**Description:** Handles the command to log critical data asynchronously to the blockchain.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** LogCriticalDataCommandHandler  
**Type:** Handler  
**Relative Path:** Integration.Application/Features/BlockchainLogging/Commands  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _blockchainAdapter  
**Type:** IBlockchainAdapter  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - LogCriticalDataCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<string>  
**Attributes:** public|override  
    
**Implemented Features:**
    
    - Asynchronous Blockchain Logging
    
**Requirement Ids:**
    
    - REQ-8-007
    
**Purpose:** Orchestrates the process of logging a critical data exchange to the blockchain.  
**Logic Description:** This handler receives a command with the data to log. It uses the IBlockchainAdapter to perform the actual interaction with the blockchain network. It is designed to be called from a background worker to ensure the primary operation is not blocked.  
**Documentation:**
    
    - **Summary:** Implements the logic for the LogCriticalDataCommand, interacting with the blockchain infrastructure via an adapter.
    
**Namespace:** Opc.System.Services.Integration.Application.Features.BlockchainLogging.Commands  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Integration/Integration.Infrastructure/Integration.Infrastructure.csproj  
**Description:** .NET project file for the Infrastructure layer. Contains implementations for external concerns like database access and external service clients.  
**Template:** .NET Class Library  
**Dependency Level:** 2  
**Name:** Integration.Infrastructure  
**Type:** Project  
**Relative Path:** Integration.Infrastructure  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To provide concrete implementations for interfaces defined in the Application layer.  
**Logic Description:** References the Application project. Contains EF Core repositories, MQTTnet/Nethereum clients, and other infrastructure code.  
**Documentation:**
    
    - **Summary:** Project file for the infrastructure layer, containing dependencies like EF Core, MQTTnet, and Nethereum.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/External/Iot/MqttIotPlatformConnector.cs  
**Description:** Implementation of the IIotPlatformConnector using the MQTT protocol.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** MqttIotPlatformConnector  
**Type:** Client  
**Relative Path:** Integration.Infrastructure/External/Iot  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    - **Name:** _mqttFactory  
**Type:** MqttFactory  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string payload
    
**Return Type:** Task<bool>  
**Attributes:** public  
    
**Implemented Features:**
    
    - IoT Data Publishing via MQTT
    
**Requirement Ids:**
    
    - REQ-8-004
    - REQ-8-005
    
**Purpose:** To connect to IoT platforms like AWS IoT Core or Azure IoT Hub using MQTT and publish data.  
**Logic Description:** Uses the MQTTnet library to establish a secure connection based on the IntegrationConnection configuration. Implements logic to publish messages to the appropriate topic. Includes Polly policies for connection retries.  
**Documentation:**
    
    - **Summary:** Provides a concrete implementation for sending data to IoT platforms over MQTT.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.External.Iot  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/External/Blockchain/NethereumBlockchainAdapter.cs  
**Description:** Implementation of the IBlockchainAdapter using the Nethereum library for Ethereum-based blockchains.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** NethereumBlockchainAdapter  
**Type:** Adapter  
**Relative Path:** Integration.Infrastructure/External/Blockchain  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogCriticalDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string dataPayload
    
**Return Type:** Task<string>  
**Attributes:** public  
    - **Name:** VerifyTransactionAsync  
**Parameters:**
    
    - Guid connectionId
    - string transactionId
    
**Return Type:** Task<bool>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Blockchain Transaction Logging
    - Blockchain Transaction Verification
    
**Requirement Ids:**
    
    - REQ-8-007
    - REQ-8-008
    
**Purpose:** To abstract interactions with an Ethereum-compatible blockchain.  
**Logic Description:** Uses Nethereum to connect to a configured node. The LogCriticalDataAsync method will hash the payload, then call a pre-deployed smart contract function to record the hash and metadata. VerifyTransactionAsync will retrieve transaction details to confirm its integrity.  
**Documentation:**
    
    - **Summary:** A concrete implementation for interacting with a permissioned blockchain using Nethereum.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.External.Blockchain  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/External/DigitalTwin/AzureDigitalTwinAdapter.cs  
**Description:** Implementation of the IDigitalTwinAdapter for Azure Digital Twins.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** AzureDigitalTwinAdapter  
**Type:** Adapter  
**Relative Path:** Integration.Infrastructure/External/DigitalTwin  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Adapter
    
**Members:**
    
    - **Name:** _digitalTwinsClient  
**Type:** DigitalTwinsClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** SendDataAsync  
**Parameters:**
    
    - Guid connectionId
    - string twinId
    - string payload
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Digital Twin Data Synchronization
    
**Requirement Ids:**
    
    - REQ-8-010
    - REQ-8-011
    
**Purpose:** To facilitate bi-directional communication with the Azure Digital Twins service.  
**Logic Description:** Uses the Azure.DigitalTwins.Core SDK to connect to an Azure Digital Twins instance. Implements logic to update twin properties based on incoming OPC data and potentially to subscribe to twin events to send commands back to the OPC client.  
**Documentation:**
    
    - **Summary:** A concrete implementation for interacting with Azure Digital Twins.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.External.DigitalTwin  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/External/AR/WebSocketArDataStreamer.cs  
**Description:** Implementation of the IArDataStreamer using WebSockets.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** WebSocketArDataStreamer  
**Type:** Service  
**Relative Path:** Integration.Infrastructure/External/AR  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Singleton
    
**Members:**
    
    
**Methods:**
    
    - **Name:** StreamDataToDeviceAsync  
**Parameters:**
    
    - string deviceId
    - string payload
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - AR Data Streaming
    
**Requirement Ids:**
    
    - REQ-8-006
    
**Purpose:** To manage WebSocket connections and stream data to connected AR clients.  
**Logic Description:** Maintains a registry of connected WebSocket clients (identified by a device ID). The StreamDataToDeviceAsync method finds the appropriate client connection and sends the data payload. This service would be exposed via a WebSocket endpoint in the API layer.  
**Documentation:**
    
    - **Summary:** A service that uses WebSockets to push real-time data to AR applications.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.External.AR  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/Services/JsonDataTransformer.cs  
**Description:** Implements the IDataTransformer for JSON-based transformations.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** JsonDataTransformer  
**Type:** Service  
**Relative Path:** Integration.Infrastructure/Services  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** TransformAsync  
**Parameters:**
    
    - Guid dataMapId
    - string sourcePayload
    
**Return Type:** Task<string>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Data Transformation Logic
    
**Requirement Ids:**
    
    - REQ-8-005
    
**Purpose:** To perform data transformations based on configured DataMap rules.  
**Logic Description:** Retrieves the specified DataMap from the repository. Parses the source JSON payload and applies the transformation rules (e.g., using Jolt.NET or a custom transformation engine) to produce the target JSON payload.  
**Documentation:**
    
    - **Summary:** A concrete implementation of the data transformation logic for JSON data.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.Services  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.Infrastructure/Resilience/IntegrationResiliencePolicies.cs  
**Description:** Defines resilience policies using Polly for external service calls.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** IntegrationResiliencePolicies  
**Type:** Configuration  
**Relative Path:** Integration.Infrastructure/Resilience  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - CircuitBreaker
    - Retry
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetDefaultRetryPolicy  
**Parameters:**
    
    
**Return Type:** IAsyncPolicy<HttpResponseMessage>  
**Attributes:** public|static  
    - **Name:** GetDefaultCircuitBreakerPolicy  
**Parameters:**
    
    
**Return Type:** IAsyncPolicy<HttpResponseMessage>  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - Resilient Service Communication
    
**Requirement Ids:**
    
    - REQ-8-005
    
**Purpose:** To centralize the definition of retry and circuit breaker policies for all external communications.  
**Logic Description:** Uses the Polly library to define policies for handling transient faults. For example, a retry policy that attempts an operation 3 times with exponential backoff, and a circuit breaker policy that stops calls to a failing service for a short period.  
**Documentation:**
    
    - **Summary:** Defines shared Polly resilience policies for HTTP clients and other external service integrations.
    
**Namespace:** Opc.System.Services.Integration.Infrastructure.Resilience  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Integration/Integration.API/Integration.API.csproj  
**Description:** .NET project file for the API layer. This is the executable entry point for the microservice.  
**Template:** ASP.NET Core Web API  
**Dependency Level:** 3  
**Name:** Integration.API  
**Type:** Project  
**Relative Path:** Integration.API  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To expose the service's functionality via REST and gRPC endpoints and host the application.  
**Logic Description:** References the Application and Infrastructure projects. Configures the ASP.NET Core pipeline, including controllers, gRPC services, DI, and authentication.  
**Documentation:**
    
    - **Summary:** The main project file for the Integration microservice's API and host.
    
**Namespace:** Opc.System.Services.Integration.API  
**Metadata:**
    
    - **Category:** API
    
- **Path:** src/Services/Integration/Integration.API/Program.cs  
**Description:** The main entry point for the Integration microservice.  
**Template:** C# Program  
**Dependency Level:** 4  
**Name:** Program  
**Type:** Main  
**Relative Path:** Integration.API  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - Application Bootstrap
    
**Requirement Ids:**
    
    
**Purpose:** To configure and run the ASP.NET Core web host.  
**Logic Description:** Sets up the dependency injection container, registering services from the Application and Infrastructure layers. Configures the HTTP request pipeline, including routing for controllers, gRPC services, WebSocket middleware, and exception handling.  
**Documentation:**
    
    - **Summary:** Bootstraps the Integration service, wires up all dependencies, and starts the web server.
    
**Namespace:** Opc.System.Services.Integration.API  
**Metadata:**
    
    - **Category:** API
    
- **Path:** src/Services/Integration/Integration.API/Controllers/ConnectionsController.cs  
**Description:** REST API controller for managing integration connections.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** ConnectionsController  
**Type:** Controller  
**Relative Path:** Integration.API/Controllers  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - REST
    
**Members:**
    
    
**Methods:**
    
    - **Name:** CreateConnection  
**Parameters:**
    
    - [FromBody] CreateConnectionCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetConnection  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Connection Configuration API
    
**Requirement Ids:**
    
    - REQ-8-005
    - REQ-8-008
    - REQ-8-010
    
**Purpose:** To provide HTTP endpoints for creating, retrieving, updating, and deleting integration connection configurations.  
**Logic Description:** This controller receives HTTP requests, maps them to application layer commands or queries (using MediatR), and returns the results as HTTP responses. It handles API-level concerns like routing, authorization, and model validation.  
**Documentation:**
    
    - **Summary:** Exposes CRUD operations for IntegrationConnection entities over a RESTful API.
    
**Namespace:** Opc.System.Services.Integration.API.Controllers  
**Metadata:**
    
    - **Category:** API
    
- **Path:** src/Services/Integration/Integration.API/Workers/BlockchainQueueWorker.cs  
**Description:** A background service that processes messages from a queue to log data to the blockchain.  
**Template:** C# BackgroundService  
**Dependency Level:** 4  
**Name:** BlockchainQueueWorker  
**Type:** Worker  
**Relative Path:** Integration.API/Workers  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - BackgroundJob
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ExecuteAsync  
**Parameters:**
    
    - CancellationToken stoppingToken
    
**Return Type:** Task  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Asynchronous Blockchain Logging
    
**Requirement Ids:**
    
    - REQ-8-007
    
**Purpose:** To decouple the time-consuming process of blockchain logging from the main request thread.  
**Logic Description:** This worker subscribes to a specific message queue (e.g., RabbitMQ). When a message arrives, it deserializes the content and uses MediatR to send a LogCriticalDataCommand to the application layer for processing. This ensures that the primary operation that triggered the logging is not delayed.  
**Documentation:**
    
    - **Summary:** A background worker that asynchronously processes and logs critical data transactions to the blockchain.
    
**Namespace:** Opc.System.Services.Integration.API.Workers  
**Metadata:**
    
    - **Category:** API
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableAwsIotConnector
  - EnableAzureIotConnector
  - EnableBlockchainLogging
  - EnableArStreaming
  
- **Database Configs:**
  
  - ConnectionStrings:DefaultConnection
  - ExternalServices:AzureIotHub:ConnectionString
  - ExternalServices:Blockchain:NodeUrl
  - ExternalServices:Blockchain:PrivateKey
  - ExternalServices:DigitalTwin:Endpoint
  


---

