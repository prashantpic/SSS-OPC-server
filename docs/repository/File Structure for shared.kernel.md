# Specification

# 1. Files

- **Path:** src/shared.kernel/SharedKernel.csproj  
**Description:** The .NET project file for the Shared Kernel library. Defines the project as a .NET 8 Standard Library, specifies dependencies, and sets project-level properties.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** SharedKernel.csproj  
**Type:** Configuration  
**Relative Path:** .  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Definition
    
**Requirement Ids:**
    
    
**Purpose:** To define the build and dependency settings for the Shared Kernel library, ensuring it can be referenced by all other service repositories in the solution.  
**Logic Description:** This file will be configured as a .NET 8 Standard Library. It will not have dependencies on other projects within the solution but will be a dependency for others. It should list any third-party libraries if required, though for a kernel, these should be minimal.  
**Documentation:**
    
    - **Summary:** Defines the fundamental configuration for the shared.kernel project, including its target framework and package references. It is the entry point for the compiler to build this reusable library.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/shared.kernel/Contracts/Grpc/management.proto  
**Description:** Protocol Buffers definition file for the Management Service's gRPC API. Defines services, RPC methods, and message structures for managing OPC client instances and their configurations.  
**Template:** Protobuf Contract  
**Dependency Level:** 0  
**Name:** management.proto  
**Type:** Contract  
**Relative Path:** Contracts/Grpc  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - gRPC API Contract Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To provide a strongly-typed, language-agnostic contract for all communication with the Management Service, ensuring consistency between the server and its clients.  
**Logic Description:** This file will use proto3 syntax. It will define a 'ManagementService' with RPCs like 'GetClientConfiguration', 'UpdateClientConfiguration', and 'GetClientStatus'. It will also define the corresponding request and response message types, such as 'ClientConfigurationRequest', 'ClientConfigurationResponse', and 'ClientStatus'.  
**Documentation:**
    
    - **Summary:** This file serves as the single source of truth for the Management Service API, enabling auto-generation of client and server code in various languages.
    
**Namespace:** Contracts.Grpc  
**Metadata:**
    
    - **Category:** APIContract
    
- **Path:** src/shared.kernel/Contracts/Grpc/ai_processing.proto  
**Description:** Protocol Buffers definition file for the AI Processing Service's gRPC API. Defines services, RPCs, and messages for interacting with AI models, such as getting predictions or submitting feedback.  
**Template:** Protobuf Contract  
**Dependency Level:** 0  
**Name:** ai_processing.proto  
**Type:** Contract  
**Relative Path:** Contracts/Grpc  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - gRPC API Contract Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To establish the formal communication contract for the AI Processing Service, detailing how other services can request AI-driven insights.  
**Logic Description:** Using proto3 syntax, this file will define an 'AiProcessingService' with RPCs like 'GetMaintenancePrediction' and 'DetectAnomalies'. Message types such as 'MaintenancePredictionRequest', 'MaintenancePredictionResponse', and 'AnomalyDetectionRequest' will be defined to carry the necessary data payloads.  
**Documentation:**
    
    - **Summary:** Defines the service contract for AI-related operations, ensuring type safety and consistency for all consumers of the AI Processing Service.
    
**Namespace:** Contracts.Grpc  
**Metadata:**
    
    - **Category:** APIContract
    
- **Path:** src/shared.kernel/Contracts/Messages/Events/OpcDataReceivedEvent.cs  
**Description:** A C# class representing the event payload for when a batch of OPC data is received and queued for processing. This is a Data Transfer Object (DTO) for the message bus.  
**Template:** C# DTO  
**Dependency Level:** 1  
**Name:** OpcDataReceivedEvent  
**Type:** DTO  
**Relative Path:** Contracts/Messages/Events  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DTO
    - Messaging
    
**Members:**
    
    - **Name:** ClientId  
**Type:** Guid  
**Attributes:** public|init  
    - **Name:** DataPoints  
**Type:** IReadOnlyList<OpcDataPointDto>  
**Attributes:** public|init  
    - **Name:** EventTimestamp  
**Type:** DateTimeOffset  
**Attributes:** public|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Message Schema Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To define a structured, immutable message contract for publishing OPC data point information onto a message queue for consumption by backend services.  
**Logic Description:** This is a plain C# record or class with init-only properties to ensure immutability. It will contain a list of OpcDataPointDto objects, each representing a single tag value change, along with metadata about the source client and when the event was generated.  
**Documentation:**
    
    - **Summary:** This class defines the schema for OPC data events published to the message bus. It is used by both the Core OPC Client (as a producer) and backend services (as consumers).
    
**Namespace:** SharedKernel.Contracts.Messages.Events  
**Metadata:**
    
    - **Category:** DataContract
    
- **Path:** src/shared.kernel/Contracts/Messages/Events/AlarmTriggeredEvent.cs  
**Description:** A C# DTO representing the event payload for a new or updated alarm from an OPC A&C server. This object is used for asynchronous communication via the message bus.  
**Template:** C# DTO  
**Dependency Level:** 1  
**Name:** AlarmTriggeredEvent  
**Type:** DTO  
**Relative Path:** Contracts/Messages/Events  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DTO
    - Messaging
    
**Members:**
    
    - **Name:** ClientId  
**Type:** Guid  
**Attributes:** public|init  
    - **Name:** SourceNode  
**Type:** string  
**Attributes:** public|init  
    - **Name:** EventType  
**Type:** string  
**Attributes:** public|init  
    - **Name:** Severity  
**Type:** int  
**Attributes:** public|init  
    - **Name:** Message  
**Type:** string  
**Attributes:** public|init  
    - **Name:** Timestamp  
**Type:** DateTimeOffset  
**Attributes:** public|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Message Schema Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To define a consistent, structured format for publishing alarm and event information to the message bus for further processing, logging, and notification.  
**Logic Description:** A simple, immutable C# record or class containing all the required fields for an alarm event as specified in the system requirements. This ensures that any service consuming alarm events receives the data in a predictable format.  
**Documentation:**
    
    - **Summary:** Defines the data contract for alarm events. It's used by the Core OPC Client to publish alarms and by backend services to consume and act upon them.
    
**Namespace:** SharedKernel.Contracts.Messages.Events  
**Metadata:**
    
    - **Category:** DataContract
    
- **Path:** src/shared.kernel/Contracts/Dtos/OpcDataPointDto.cs  
**Description:** A Data Transfer Object representing a single OPC tag's value at a specific point in time. Used within other contracts, such as OpcDataReceivedEvent.  
**Template:** C# DTO  
**Dependency Level:** 0  
**Name:** OpcDataPointDto  
**Type:** DTO  
**Relative Path:** Contracts/Dtos  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DTO
    
**Members:**
    
    - **Name:** TagIdentifier  
**Type:** string  
**Attributes:** public|init  
    - **Name:** Value  
**Type:** string  
**Attributes:** public|init  
    - **Name:** Quality  
**Type:** string  
**Attributes:** public|init  
    - **Name:** Timestamp  
**Type:** DateTimeOffset  
**Attributes:** public|init  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Data Model Definition
    
**Requirement Ids:**
    
    - REQ-SAP-005
    
**Purpose:** To provide a standardized, serializable representation of an OPC tag's data for use in message queues and API calls across the distributed system.  
**Logic Description:** This will be an immutable C# record with properties for the tag's identifier, its value (serialized as a string for universal compatibility), its quality, and the timestamp of the reading. It acts as a common data structure for real-time data.  
**Documentation:**
    
    - **Summary:** A fundamental DTO that encapsulates a single data point from an OPC server. It is a core building block for many inter-service communication payloads.
    
**Namespace:** SharedKernel.Contracts.Dtos  
**Metadata:**
    
    - **Category:** DataContract
    
- **Path:** src/shared.kernel/SeedWork/Entity.cs  
**Description:** An abstract base class for domain entities. Provides a common implementation for identity and equality comparison, encapsulating core DDD entity concepts.  
**Template:** C# Base Class  
**Dependency Level:** 0  
**Name:** Entity  
**Type:** BaseClass  
**Relative Path:** SeedWork  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DDD-Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public|protected set  
    
**Methods:**
    
    - **Name:** Equals  
**Parameters:**
    
    - object obj
    
**Return Type:** bool  
**Attributes:** public override  
    - **Name:** GetHashCode  
**Parameters:**
    
    
**Return Type:** int  
**Attributes:** public override  
    
**Implemented Features:**
    
    - Domain Entity Base
    
**Requirement Ids:**
    
    
**Purpose:** To provide a consistent, reusable base for all domain entities across all microservices, ensuring they are identified by their ID rather than their properties.  
**Logic Description:** This abstract class will have a protected constructor and a public 'Id' property of type Guid. It will override 'Equals' and 'GetHashCode' to perform comparisons based on the 'Id' property. It also implements '==' and '!=' operators for the same purpose.  
**Documentation:**
    
    - **Summary:** This class is a foundational building block for Domain-Driven Design, representing an object with a distinct identity that persists over time.
    
**Namespace:** SharedKernel.SeedWork  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/shared.kernel/SeedWork/AggregateRoot.cs  
**Description:** An abstract base class for aggregate roots, which are the primary entry points to a domain aggregate. Extends the Entity base class and adds domain event management.  
**Template:** C# Base Class  
**Dependency Level:** 1  
**Name:** AggregateRoot  
**Type:** BaseClass  
**Relative Path:** SeedWork  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DDD-Aggregate
    
**Members:**
    
    - **Name:** _domainEvents  
**Type:** List<IDomainEvent>  
**Attributes:** private readonly  
    - **Name:** DomainEvents  
**Type:** IReadOnlyCollection<IDomainEvent>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** AddDomainEvent  
**Parameters:**
    
    - IDomainEvent domainEvent
    
**Return Type:** void  
**Attributes:** protected  
    - **Name:** ClearDomainEvents  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Aggregate Root Base
    - Domain Event Management
    
**Requirement Ids:**
    
    
**Purpose:** To enforce transactional consistency for a group of related domain objects (the aggregate) and to serve as a central point for publishing domain events.  
**Logic Description:** This abstract class inherits from 'Entity'. It contains a private list of domain events and a public read-only collection to expose them. A protected 'AddDomainEvent' method allows derived classes to raise events, and a public 'ClearDomainEvents' method is used by the infrastructure layer after dispatching them.  
**Documentation:**
    
    - **Summary:** A core DDD pattern representing a cluster of domain objects that can be treated as a single unit. It is the root of a consistency boundary.
    
**Namespace:** SharedKernel.SeedWork  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/shared.kernel/SeedWork/IDomainEvent.cs  
**Description:** A marker interface for domain events. This interface does not define any members but is used to identify classes that represent significant occurrences within the domain.  
**Template:** C# Interface  
**Dependency Level:** 0  
**Name:** IDomainEvent  
**Type:** Interface  
**Relative Path:** SeedWork  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    - DDD-DomainEvent
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Domain Event Contract
    
**Requirement Ids:**
    
    
**Purpose:** To establish a common type for all domain events across the system, enabling generic handling mechanisms such as event dispatchers and handlers.  
**Logic Description:** This is an empty interface. Its sole purpose is to act as a constraint and a common identifier for domain event classes.  
**Documentation:**
    
    - **Summary:** A marker interface that signifies that a class is a domain event, representing something that has happened in the past within the domain.
    
**Namespace:** SharedKernel.SeedWork  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/shared.kernel/Exceptions/DomainException.cs  
**Description:** A custom base exception class for all exceptions thrown from the domain layer. This allows for specific catch blocks for domain-related errors.  
**Template:** C# Exception  
**Dependency Level:** 0  
**Name:** DomainException  
**Type:** Exception  
**Relative Path:** Exceptions  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** DomainException  
**Parameters:**
    
    - string message
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** DomainException  
**Parameters:**
    
    - string message
    - Exception innerException
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Custom Exception Handling
    
**Requirement Ids:**
    
    
**Purpose:** To create a specific exception type that represents a violation of domain logic, distinguishing it from application or infrastructure errors.  
**Logic Description:** A simple class that inherits from the base 'System.Exception' class. It provides standard constructors for setting the exception message and an optional inner exception.  
**Documentation:**
    
    - **Summary:** The base exception for all domain-specific errors. Catching this type allows for handling any error that originates from a violation of business rules or domain invariants.
    
**Namespace:** SharedKernel.Exceptions  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/shared.kernel/Domain/Enums/OpcStandard.cs  
**Description:** An enumeration defining the different types of OPC standards supported by the system.  
**Template:** C# Enum  
**Dependency Level:** 0  
**Name:** OpcStandard  
**Type:** Enum  
**Relative Path:** Domain/Enums  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** DA  
**Type:** enum  
**Attributes:**   
    - **Name:** UA  
**Type:** enum  
**Attributes:**   
    - **Name:** XmlDa  
**Type:** enum  
**Attributes:**   
    - **Name:** Hda  
**Type:** enum  
**Attributes:**   
    - **Name:** Ac  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    - OPC Standard Type Definition
    
**Requirement Ids:**
    
    
**Purpose:** To provide a strongly-typed, consistent way to refer to OPC standards throughout the codebase, avoiding magic strings and improving readability.  
**Logic Description:** A standard C# enumeration with members for each supported OPC standard: DA, UA, XmlDa, Hda, and Ac.  
**Documentation:**
    
    - **Summary:** Represents the set of supported OPC communication standards within the system.
    
**Namespace:** SharedKernel.Domain.Enums  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/shared.kernel/Abstractions/ICurrentUser.cs  
**Description:** An interface that defines a contract for accessing information about the currently authenticated user who is making a request.  
**Template:** C# Interface  
**Dependency Level:** 0  
**Name:** ICurrentUser  
**Type:** Interface  
**Relative Path:** Abstractions  
**Repository Id:** REPO-SAP-012  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** UserId  
**Type:** Guid?  
**Attributes:** get  
    - **Name:** IsAuthenticated  
**Type:** bool  
**Attributes:** get  
    - **Name:** Roles  
**Type:** IReadOnlyList<string>  
**Attributes:** get  
    
**Methods:**
    
    
**Implemented Features:**
    
    - User Context Abstraction
    
**Requirement Ids:**
    
    
**Purpose:** To decouple business logic from the specific implementation of authentication (e.g., HTTP context), allowing for easier testing and greater flexibility.  
**Logic Description:** This interface defines properties to get the current user's ID, authentication status, and their assigned roles. The implementation will be provided by the infrastructure layer of each service.  
**Documentation:**
    
    - **Summary:** Provides a contract for a service that can resolve the current user's identity and permissions from the execution context.
    
**Namespace:** SharedKernel.Abstractions  
**Metadata:**
    
    - **Category:** Abstractions
    


---

# 2. Configuration

- **Feature Toggles:**
  
  
- **Database Configs:**
  
  


---

