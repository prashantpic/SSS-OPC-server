# Specification

# 1. Files

- **Path:** src/Opc.System.Shared.Kernel/Opc.System.Shared.Kernel.csproj  
**Description:** The .NET 8 project file defining the Shared Kernel library. It specifies the target framework as .NET Standard for maximum compatibility, lists NuGet package dependencies like Google.Protobuf and Grpc.Tools for gRPC contract compilation, and configures the build process for the .proto files.  
**Template:** C# .NET Standard Library  
**Dependency Level:** 0  
**Name:** Opc.System.Shared.Kernel  
**Type:** Project  
**Relative Path:** Opc.System.Shared.Kernel.csproj  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Definition
    - Dependency Management
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To define the project structure, dependencies, and build configurations for the shared kernel library.  
**Logic Description:** This file should be configured as a .NET Standard 2.1 library to ensure wide compatibility with both .NET 6+ (for Core OPC Client) and the .NET 8 server-side application. It must include ItemGroup entries for Protobuf files, specifying them as 'Proto' items and enabling the gRPC C# code generator. Dependencies on Google.Protobuf, Grpc.Tools, and Grpc.Core.Api should be added.  
**Documentation:**
    
    - **Summary:** Defines the Opc.System.Shared.Kernel project, which serves as a common library for data models, interfaces, and communication contracts across the system.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Opc.System.Shared.Kernel/Protos/common.proto  
**Description:** Defines common, reusable message types for gRPC services to avoid duplication across different .proto files. Includes standard wrapper types and structures like paged results.  
**Template:** gRPC Protocol Buffer  
**Dependency Level:** 1  
**Name:** common  
**Type:** Contract  
**Relative Path:** Protos/common.proto  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Common gRPC Message Types
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To establish a set of standard, reusable message definitions for use in other gRPC service contracts throughout the system.  
**Logic Description:** Define messages for common data patterns. Include 'PagedRequest' with page number and size. Define 'StatusResponse' with a boolean success flag and an error message string. Define wrappers for primitive types like 'GuidValue' to handle cases where 'string' is not ideal. Set the C# namespace option to 'Opc.System.Shared.Kernel.Protos.Common'.  
**Documentation:**
    
    - **Summary:** This file contains shared Protocol Buffer message definitions used by multiple gRPC services, promoting consistency in API contracts.
    
**Namespace:** opc.system.shared.kernel.protos  
**Metadata:**
    
    - **Category:** ApiContract
    
- **Path:** src/Opc.System.Shared.Kernel/Protos/management_api.proto  
**Description:** The gRPC contract definition for the Management Service. It defines the RPC methods for managing OPC client instances, their configurations, and health status, along with the request and response message structures.  
**Template:** gRPC Protocol Buffer  
**Dependency Level:** 2  
**Name:** management_api  
**Type:** Contract  
**Relative Path:** Protos/management_api.proto  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Client Management API Contract
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To define the strict, versioned API contract for all communication related to the centralized management of OPC client instances.  
**Logic Description:** Import 'common.proto'. Define a 'ManagementApiService' service. Include RPCs like 'GetClientStatus' and 'UpdateClientConfiguration'. Define message types like 'ClientStatusResponse' (containing health KPIs) and 'ClientConfigurationRequest' (containing connection details, tag lists, etc.). Set the C# namespace option to 'Opc.System.Shared.Kernel.Protos.Management'.  
**Documentation:**
    
    - **Summary:** This file specifies the gRPC service contract for the Management Service, detailing methods and data structures for remote client administration.
    
**Namespace:** opc.system.shared.kernel.protos  
**Metadata:**
    
    - **Category:** ApiContract
    
- **Path:** src/Opc.System.Shared.Kernel/Messaging/IIntegrationEvent.cs  
**Description:** A marker interface for all integration events that are published to the message bus. This helps in creating generic handlers, publishers, and subscribers for event-driven communication.  
**Template:** C# Interface  
**Dependency Level:** 0  
**Name:** IIntegrationEvent  
**Type:** Interface  
**Relative Path:** Messaging/IIntegrationEvent.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    - **Name:** EventId  
**Type:** Guid  
**Attributes:** public|get  
    - **Name:** CreationDate  
**Type:** DateTimeOffset  
**Attributes:** public|get  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Event Contract Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To provide a common contract for all integration events, ensuring they have essential metadata like an ID and creation timestamp.  
**Logic Description:** This file will define a public C# interface named IIntegrationEvent. It will contain two read-only properties: a Guid 'EventId' and a DateTimeOffset 'CreationDate'. This interface will be implemented by all specific event classes to enforce a standard event structure.  
**Documentation:**
    
    - **Summary:** Defines a base interface for integration events, ensuring a consistent structure for messages published across different microservices.
    
**Namespace:** Opc.System.Shared.Kernel.Messaging  
**Metadata:**
    
    - **Category:** Messaging
    
- **Path:** src/Opc.System.Shared.Kernel/Messaging/Events/OpcDataReceivedEvent.cs  
**Description:** Defines the data contract for a message published when a batch of real-time OPC data is received from a client instance. This is a key schema for streaming data to the backend.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** OpcDataReceivedEvent  
**Type:** DataContract  
**Relative Path:** Messaging/Events/OpcDataReceivedEvent.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    - **Name:** ClientId  
**Type:** Guid  
**Attributes:** public|get|init  
    - **Name:** ServerId  
**Type:** Guid  
**Attributes:** public|get|init  
    - **Name:** DataPoints  
**Type:** IReadOnlyList<DataPointDto>  
**Attributes:** public|get|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Real-time Data Message Schema
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To define a structured, versionable schema for real-time OPC data points being transmitted over the message bus for asynchronous processing.  
**Logic Description:** This class will implement the 'IIntegrationEvent' interface. It will contain properties for ClientId, ServerId, and a list of data points. A nested 'DataPointDto' class will be defined within, containing properties for TagIdentifier, Value, Quality, and Timestamp. The class should be a record or have init-only setters for immutability.  
**Documentation:**
    
    - **Summary:** Represents the message schema for a batch of OPC tag data points received from a client instance, used for event-driven data ingestion.
    
**Namespace:** Opc.System.Shared.Kernel.Messaging.Events  
**Metadata:**
    
    - **Category:** Messaging
    
- **Path:** src/Opc.System.Shared.Kernel/Messaging/Events/AlarmTriggeredEvent.cs  
**Description:** Defines the data contract for an alarm or condition event published by an OPC client instance. This schema is used to communicate A&E events asynchronously to backend services.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** AlarmTriggeredEvent  
**Type:** DataContract  
**Relative Path:** Messaging/Events/AlarmTriggeredEvent.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    - **Name:** ClientId  
**Type:** Guid  
**Attributes:** public|get|init  
    - **Name:** SourceNode  
**Type:** string  
**Attributes:** public|get|init  
    - **Name:** Message  
**Type:** string  
**Attributes:** public|get|init  
    - **Name:** Severity  
**Type:** int  
**Attributes:** public|get|init  
    - **Name:** Acknowledged  
**Type:** bool  
**Attributes:** public|get|init  
    - **Name:** OccurrenceTime  
**Type:** DateTimeOffset  
**Attributes:** public|get|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Alarm/Event Message Schema
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To provide a standardized message structure for alarm and condition events, enabling consistent processing and storage by backend services.  
**Logic Description:** This class will implement 'IIntegrationEvent'. It will include properties for all key fields of an alarm record as required by the system, such as SourceNode, Message, Severity, OccurrenceTime, and Acknowledged state. The properties should be immutable after creation.  
**Documentation:**
    
    - **Summary:** Defines the message contract for OPC A&E events, used to decouple the OPC client from the backend alarm processing and storage services.
    
**Namespace:** Opc.System.Shared.Kernel.Messaging.Events  
**Metadata:**
    
    - **Category:** Messaging
    
- **Path:** src/Opc.System.Shared.Kernel/Contracts/Common/PagedResultDto.cs  
**Description:** A generic Data Transfer Object for returning paginated data from APIs. It contains the list of items for the current page and metadata about the pagination, such as total count and page size.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** PagedResultDto  
**Type:** DTO  
**Relative Path:** Contracts/Common/PagedResultDto.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** Items  
**Type:** IReadOnlyList<T>  
**Attributes:** public|get  
    - **Name:** TotalCount  
**Type:** long  
**Attributes:** public|get  
    - **Name:** PageNumber  
**Type:** int  
**Attributes:** public|get  
    - **Name:** PageSize  
**Type:** int  
**Attributes:** public|get  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Pagination Data Contract
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To standardize the structure of responses for all list-based API endpoints that support pagination, ensuring a consistent client-side experience.  
**Logic Description:** Define a public generic class 'PagedResultDto<T>'. It will have properties for the list of items of type T, and pagination metadata (TotalCount, PageNumber, PageSize). The constructor will take these values to ensure the object is created in a valid state.  
**Documentation:**
    
    - **Summary:** A generic DTO used to encapsulate a paginated list of items returned from an API, providing both the data for the current page and overall pagination details.
    
**Namespace:** Opc.System.Shared.Kernel.Contracts.Common  
**Metadata:**
    
    - **Category:** ApiContract
    
- **Path:** src/Opc.System.Shared.Kernel/Domain/SeedWork/ValueObject.cs  
**Description:** An abstract base class for creating Value Objects in the domain model. It provides built-in equality comparison based on the object's components, simplifying the implementation of custom value objects.  
**Template:** C# Abstract Class  
**Dependency Level:** 0  
**Name:** ValueObject  
**Type:** BaseClass  
**Relative Path:** Domain/SeedWork/ValueObject.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DDD-ValueObject
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetEqualityComponents  
**Parameters:**
    
    
**Return Type:** IEnumerable<object>  
**Attributes:** protected|abstract  
    - **Name:** Equals  
**Parameters:**
    
    - object obj
    
**Return Type:** bool  
**Attributes:** public|override  
    - **Name:** GetHashCode  
**Parameters:**
    
    
**Return Type:** int  
**Attributes:** public|override  
    
**Implemented Features:**
    
    - Value Object Pattern
    
**Requirement Ids:**
    
    
**Purpose:** To enforce the semantics of Value Objects (immutability and structural equality) and reduce boilerplate code in domain model implementations.  
**Logic Description:** This abstract class will override the 'Equals' and 'GetHashCode' methods. The 'Equals' logic will iterate over the components returned by the abstract 'GetEqualityComponents' method and compare them. 'GetHashCode' will compute a hash based on these components. It will also overload the '==' and '!=' operators.  
**Documentation:**
    
    - **Summary:** Provides the base implementation for the Value Object pattern, handling structural equality comparison automatically.
    
**Namespace:** Opc.System.Shared.Kernel.Domain.SeedWork  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Opc.System.Shared.Kernel/Infrastructure/Abstractions/IEventBus.cs  
**Description:** Defines the contract for the system's event bus, abstracting the specific message queue technology (e.g., RabbitMQ, Kafka). It provides methods for publishing events and subscribing to them.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IEventBus  
**Type:** Interface  
**Relative Path:** Infrastructure/Abstractions/IEventBus.cs  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - EventDrivenArchitecture
    
**Members:**
    
    
**Methods:**
    
    - **Name:** PublishAsync  
**Parameters:**
    
    - IIntegrationEvent integrationEvent
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** SubscribeAsync<T, TH>  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Event Bus Abstraction
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To decouple application and domain layers from the concrete messaging infrastructure, allowing for easier testing and technology swapping.  
**Logic Description:** This interface defines the core operations of a message bus. 'PublishAsync' takes an 'IIntegrationEvent' and sends it to the bus. 'SubscribeAsync' is a generic method that registers a handler 'TH' for a specific event type 'T'. This allows services to declare their interest in certain events without knowing the publisher.  
**Documentation:**
    
    - **Summary:** Provides a high-level abstraction for publishing and subscribing to integration events across the microservices architecture.
    
**Namespace:** Opc.System.Shared.Kernel.Infrastructure.Abstractions  
**Metadata:**
    
    - **Category:** Infrastructure
    


---

# 2. Configuration

- **Feature Toggles:**
  
  
- **Database Configs:**
  
  


---

