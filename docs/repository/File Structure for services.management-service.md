# Specification

# 1. Files

- **Path:** src/ManagementService/ManagementService.csproj  
**Description:** The C# project file for the Management Service, defining its dependencies, target framework (.NET 8), and other build configurations.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** ManagementService  
**Type:** ProjectFile  
**Relative Path:**   
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the project structure and dependencies for the .NET build system.  
**Logic Description:** This file will list NuGet package references such as ASP.NET Core, gRPC, MediatR, and any clients for messaging or data services.  
**Documentation:**
    
    - **Summary:** Specifies the project's metadata and build instructions for MSBuild.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/ManagementService/Program.cs  
**Description:** The main entry point for the Management Service. This file configures and starts the ASP.NET Core web host, registers services for dependency injection, and sets up middleware pipelines for HTTP and gRPC.  
**Template:** C# Main EntryPoint  
**Dependency Level:** 4  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:**   
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - ServiceBootstrap
    - DependencyInjectionSetup
    - MiddlewareConfiguration
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** Initializes the microservice, configures all services and the HTTP request pipeline, and runs the application.  
**Logic Description:** The logic will build a WebApplication. It will register application services (MediatR), repository implementations, API controllers, and gRPC services. It will also configure logging, authentication, authorization, and map the REST and gRPC endpoints. Health checks will be configured here.  
**Documentation:**
    
    - **Summary:** Bootstraps the entire Management Service application.
    
**Namespace:** ManagementService  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/ManagementService/appsettings.json  
**Description:** Configuration file for the Management Service, containing settings for service URLs, connection strings for messaging queues, logging levels, and other operational parameters.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides runtime configuration for the service.  
**Logic Description:** This JSON file will contain sections for Logging, service discovery URLs (e.g., DataService), RabbitMQ/Kafka connection details, and any feature toggles.  
**Documentation:**
    
    - **Summary:** External configuration source for the Management Service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/ManagementService/Domain/Aggregates/OpcClientInstance.cs  
**Description:** Represents the OpcClientInstance aggregate root. This is the core domain entity, encapsulating the state and behavior of a managed OPC client, including its identity, configuration, and health status.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** OpcClientInstance  
**Type:** AggregateRoot  
**Relative Path:** Domain/Aggregates  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Aggregate
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public|readonly  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** Site  
**Type:** string  
**Attributes:** public  
    - **Name:** LastSeen  
**Type:** DateTimeOffset  
**Attributes:** public  
    - **Name:** HealthStatus  
**Type:** HealthStatus  
**Attributes:** public  
    - **Name:** Configuration  
**Type:** ClientConfiguration  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** UpdateConfiguration  
**Parameters:**
    
    - ClientConfiguration newConfiguration
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** ReportHealth  
**Parameters:**
    
    - HealthStatus newStatus
    - DateTimeOffset reportTime
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Register  
**Parameters:**
    
    - Guid id
    - string name
    - string site
    
**Return Type:** OpcClientInstance  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - ClientStateManagement
    - ConfigurationUpdateLogic
    - HealthStatusUpdateLogic
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To model a managed OPC client instance and enforce business rules related to its lifecycle and state.  
**Logic Description:** This class will contain the core properties of a client instance. Methods will enforce invariants, like validating a new configuration before applying it or updating the LastSeen timestamp when health is reported. It will raise domain events (e.g., ClientConfigurationUpdated) upon state changes.  
**Documentation:**
    
    - **Summary:** The central domain model for the Management Service, representing a single OPC client instance.
    
**Namespace:** ManagementService.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/ManagementService/Domain/ValueObjects/ClientConfiguration.cs  
**Description:** A value object representing the configuration settings for an OPC client instance. This is an immutable object.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** ClientConfiguration  
**Type:** ValueObject  
**Relative Path:** Domain/ValueObjects  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - ValueObject
    
**Members:**
    
    - **Name:** PollingIntervalSeconds  
**Type:** int  
**Attributes:** public|readonly  
    - **Name:** TagConfigurations  
**Type:** IReadOnlyList<TagConfig>  
**Attributes:** public|readonly  
    
**Methods:**
    
    
**Implemented Features:**
    
    - ClientConfigurationState
    
**Requirement Ids:**
    
    - REQ-6-001
    
**Purpose:** To encapsulate the set of configuration parameters for a client in a consistent and immutable way.  
**Logic Description:** This will be a record or class that overrides Equals and GetHashCode to provide value-based equality. It will contain properties representing various settings that can be pushed to a client.  
**Documentation:**
    
    - **Summary:** Defines the immutable configuration parameters for an OpcClientInstance.
    
**Namespace:** ManagementService.Domain.ValueObjects  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/ManagementService/Domain/ValueObjects/HealthStatus.cs  
**Description:** A value object representing the health status reported by a client, including KPIs like connection status and throughput.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** HealthStatus  
**Type:** ValueObject  
**Relative Path:** Domain/ValueObjects  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - ValueObject
    
**Members:**
    
    - **Name:** IsConnected  
**Type:** bool  
**Attributes:** public|readonly  
    - **Name:** DataThroughput  
**Type:** double  
**Attributes:** public|readonly  
    - **Name:** CpuUsagePercent  
**Type:** double  
**Attributes:** public|readonly  
    
**Methods:**
    
    
**Implemented Features:**
    
    - ClientHealthState
    
**Requirement Ids:**
    
    - REQ-6-002
    
**Purpose:** Encapsulates the set of health metrics reported by a client at a point in time.  
**Logic Description:** An immutable record or class with properties for various KPIs. It will use value-based equality.  
**Documentation:**
    
    - **Summary:** Defines the immutable health and KPI metrics for an OpcClientInstance.
    
**Namespace:** ManagementService.Domain.ValueObjects  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/ManagementService/Application/Contracts/Persistence/IOpcClientInstanceRepository.cs  
**Description:** The repository interface defining the contract for data persistence operations for the OpcClientInstance aggregate.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IOpcClientInstanceRepository  
**Type:** RepositoryInterface  
**Relative Path:** Application/Contracts/Persistence  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - RepositoryPattern
    - DependencyInversionPrinciple
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<OpcClientInstance>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** GetAllAsync  
**Parameters:**
    
    
**Return Type:** Task<IReadOnlyList<OpcClientInstance>>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To abstract the data storage mechanism for OpcClientInstance aggregates from the application logic.  
**Logic Description:** This interface defines standard CRUD-like operations for managing OpcClientInstance entities. It is defined in the Application layer and implemented in the Infrastructure layer.  
**Documentation:**
    
    - **Summary:** Contract for persistence operations related to OpcClientInstance.
    
**Namespace:** ManagementService.Application.Contracts.Persistence  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/ManagementService/Application/Features/ClientMonitoring/GetClientDetailsQuery.cs  
**Description:** A query object used to request detailed information for a specific OPC client instance.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** GetClientDetailsQuery  
**Type:** Query  
**Relative Path:** Application/Features/ClientMonitoring  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** ClientId  
**Type:** Guid  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-6-001
    
**Purpose:** Represents a request to fetch the full details of a single managed client.  
**Logic Description:** This is a simple data carrier class, likely a record, that implements IRequest<ClientDetailsResponse> from MediatR. It holds the ID of the client to be queried.  
**Documentation:**
    
    - **Summary:** A CQRS query to retrieve details for one OpcClientInstance.
    
**Namespace:** ManagementService.Application.Features.ClientMonitoring  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/ManagementService/Application/Features/ClientMonitoring/GetClientDetailsQueryHandler.cs  
**Description:** The handler for the GetClientDetailsQuery. It retrieves the client instance from the repository and maps it to a response DTO.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** GetClientDetailsQueryHandler  
**Type:** QueryHandler  
**Relative Path:** Application/Features/ClientMonitoring  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _clientRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - GetClientDetailsQuery request
    - CancellationToken cancellationToken
    
**Return Type:** Task<ClientDetailsResponse>  
**Attributes:** public  
    
**Implemented Features:**
    
    - ClientDataRetrieval
    
**Requirement Ids:**
    
    - REQ-6-001
    
**Purpose:** To process the GetClientDetailsQuery, fetch data, and return it in a structured response.  
**Logic Description:** This handler will use the injected IOpcClientInstanceRepository to fetch the OpcClientInstance by its ID. It will then use a mapper to convert the domain entity into the ClientDetailsResponse DTO. It handles cases where the client is not found.  
**Documentation:**
    
    - **Summary:** Implements the logic for handling a GetClientDetailsQuery.
    
**Namespace:** ManagementService.Application.Features.ClientMonitoring  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/ManagementService/Application/Features/BulkOperations/ExecuteBulkConfigurationCommandHandler.cs  
**Description:** The handler for executing a configuration update across multiple client instances.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** ExecuteBulkConfigurationCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/Features/BulkOperations  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _clientRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    - **Name:** _configPublisher  
**Type:** IConfigurationUpdatePublisher  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ExecuteBulkConfigurationCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - BulkConfigurationUpdate
    
**Requirement Ids:**
    
    - REQ-6-002
    
**Purpose:** To orchestrate the application of a single configuration to multiple specified clients.  
**Logic Description:** The handler will iterate through the list of client IDs in the command. For each client, it will fetch the domain object, apply the configuration update logic, save the updated state via the repository, and then publish a configuration update event to a message queue for the respective client to consume. This ensures the operation is resilient.  
**Documentation:**
    
    - **Summary:** Implements the business logic for handling bulk configuration updates.
    
**Namespace:** ManagementService.Application.Features.BulkOperations  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/ManagementService/Api/Controllers/ClientsController.cs  
**Description:** The REST API controller for managing and monitoring OPC client instances. This is the primary interface for the centralized management dashboard.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** ClientsController  
**Type:** Controller  
**Relative Path:** Api/Controllers  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - REST
    
**Members:**
    
    - **Name:** _mediator  
**Type:** IMediator  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetAllClients  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetClientById  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** UpdateConfiguration  
**Parameters:**
    
    - Guid id
    - UpdateConfigurationRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** ExecuteBulkUpdate  
**Parameters:**
    
    - BulkUpdateRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetAggregatedKpis  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - ClientManagementApi
    - ClientMonitoringApi
    - BulkOperationApi
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To expose client management and monitoring functionalities over a RESTful HTTP API.  
**Logic Description:** This controller class will receive HTTP requests. It will not contain business logic. Instead, it will create appropriate Command or Query objects (e.g., GetClientDetailsQuery) and send them to the MediatR pipeline for processing. It then maps the results to HTTP responses (e.g., 200 OK, 404 Not Found).  
**Documentation:**
    
    - **Summary:** Provides HTTP endpoints for the centralized dashboard to interact with the Management Service.
    
**Namespace:** ManagementService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/ManagementService/Api/GrpcServices/ClientLifecycleService.cs  
**Description:** The gRPC service implementation for OPC clients to register themselves and report their health status.  
**Template:** C# gRPC Service  
**Dependency Level:** 4  
**Name:** ClientLifecycleService  
**Type:** GrpcService  
**Relative Path:** Api/GrpcServices  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - gRPC
    
**Members:**
    
    - **Name:** _mediator  
**Type:** IMediator  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** RegisterClient  
**Parameters:**
    
    - RegisterClientRequest request
    - ServerCallContext context
    
**Return Type:** Task<RegisterClientReply>  
**Attributes:** public|override  
    - **Name:** ReportHealth  
**Parameters:**
    
    - ReportHealthRequest request
    - ServerCallContext context
    
**Return Type:** Task<ReportHealthReply>  
**Attributes:** public|override  
    
**Implemented Features:**
    
    - ClientRegistrationGrpcEndpoint
    - ClientHealthReportingEndpoint
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To provide a high-performance, strongly-typed API for OPC clients to communicate with the management service.  
**Logic Description:** This class inherits from the gRPC-generated base class. Each method will translate the incoming gRPC request into a CQRS command (e.g., ReportHealthCommand) and dispatch it using MediatR. It will then formulate a gRPC reply based on the result of the command execution.  
**Documentation:**
    
    - **Summary:** Implements the gRPC service contract for client lifecycle management.
    
**Namespace:** ManagementService.Api.GrpcServices  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/ManagementService/Infrastructure/Persistence/OpcClientInstanceRepository.cs  
**Description:** Implementation of the IOpcClientInstanceRepository interface. This class communicates with the external Data Service to perform persistence operations.  
**Template:** C# Repository  
**Dependency Level:** 3  
**Name:** OpcClientInstanceRepository  
**Type:** Repository  
**Relative Path:** Infrastructure/Persistence  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _dataServiceClient  
**Type:** DataServiceClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<OpcClientInstance>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** GetAllAsync  
**Parameters:**
    
    
**Return Type:** Task<IReadOnlyList<OpcClientInstance>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - ClientDataPersistence
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To provide a concrete implementation for storing and retrieving OpcClientInstance data by delegating calls to the Data Service.  
**Logic Description:** Each method in this class will use the injected DataServiceClient (an HTTP or gRPC client) to call the corresponding endpoint on the Data Service. It will handle the mapping between the domain models of the Management Service and the DTOs required by the Data Service API.  
**Documentation:**
    
    - **Summary:** Implements the persistence contract for OpcClientInstance by communicating with the central Data Service.
    
**Namespace:** ManagementService.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** protos/management.proto  
**Description:** Protocol Buffers definition file for the gRPC services exposed by the Management Service.  
**Template:** Protobuf  
**Dependency Level:** 0  
**Name:** management  
**Type:** ApiDefinition  
**Relative Path:** ../../protos  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - gRPC
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - GrpcApiContract
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    
**Purpose:** To define the strongly-typed, cross-language contract for gRPC communication with this service.  
**Logic Description:** This file will define the `ClientLifecycleService` with its RPCs (`RegisterClient`, `ReportHealth`). It will also define the message types for requests and replies (e.g., `RegisterClientRequest`, `ReportHealthRequest`, etc.), specifying the data fields and their types.  
**Documentation:**
    
    - **Summary:** Defines the service and message contracts for the Management Service's gRPC endpoints.
    
**Namespace:** management.v1  
**Metadata:**
    
    - **Category:** API
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableBulkSoftwareUpdates
  - EnableDetailedKpiTracking
  
- **Database Configs:**
  
  


---

