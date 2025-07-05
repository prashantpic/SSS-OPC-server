# Specification

# 1. Files

- **Path:** src/Services/Management/Management.Domain/Aggregates/OpcClientInstance.cs  
**Description:** The root aggregate representing a managed OPC Client instance. It encapsulates the client's identity, its comprehensive configuration, and its current operational state. It is the consistency boundary for all operations related to a single client.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** OpcClientInstance  
**Type:** AggregateRoot  
**Relative Path:** Domain/Aggregates  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Aggregate
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** ClientInstanceId  
**Attributes:** public|private set  
    - **Name:** Name  
**Type:** string  
**Attributes:** public|private set  
    - **Name:** Configuration  
**Type:** ClientConfiguration  
**Attributes:** public|private set  
    - **Name:** LastSeen  
**Type:** DateTimeOffset  
**Attributes:** public|private set  
    - **Name:** HealthStatus  
**Type:** HealthStatus  
**Attributes:** public|private set  
    - **Name:** IsActive  
**Type:** bool  
**Attributes:** public|private set  
    
**Methods:**
    
    - **Name:** UpdateConfiguration  
**Parameters:**
    
    - ClientConfiguration newConfiguration
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** ReportHeartbeat  
**Parameters:**
    
    - HealthStatus status
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Deactivate  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Create  
**Parameters:**
    
    - string name
    - ClientConfiguration initialConfiguration
    
**Return Type:** OpcClientInstance  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Client State Management
    - Configuration Update Logic
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-6-001
    
**Purpose:** To model a single, managed OPC client instance, ensuring its configuration and state are always consistent.  
**Logic Description:** Contains business logic for managing a client's lifecycle. The UpdateConfiguration method will validate the new configuration before applying it and may raise a domain event. ReportHeartbeat updates the LastSeen and HealthStatus. All state changes are internal to enforce aggregate consistency rules.  
**Documentation:**
    
    - **Summary:** This class is the core domain model for an OPC client instance. It acts as a transactional boundary for all configuration and status updates for a specific client.
    
**Namespace:** Opc.System.Services.Management.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Management/Management.Domain/Aggregates/ClientConfiguration.cs  
**Description:** Represents the complete configuration for an OpcClientInstance. This can be treated as a complex Value Object or a child Entity within the OpcClientInstance aggregate.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** ClientConfiguration  
**Type:** Entity  
**Relative Path:** Domain/Aggregates  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** TagConfigurations  
**Type:** IReadOnlyCollection<TagConfiguration>  
**Attributes:** public  
    - **Name:** ServerConnections  
**Type:** IReadOnlyCollection<ServerConnectionSetting>  
**Attributes:** public  
    - **Name:** PollingInterval  
**Type:** TimeSpan  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Client Configuration Data Model
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-6-001
    
**Purpose:** To hold all configurable parameters for an OPC client instance.  
**Logic Description:** This is primarily a data-holding class. It may include validation logic within its constructor or factory method to ensure a valid configuration state upon creation (e.g., no duplicate tag definitions).  
**Documentation:**
    
    - **Summary:** A data structure that encapsulates all settings for an OPC client, such as server connection details and tag lists.
    
**Namespace:** Opc.System.Services.Management.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Management/Management.Domain/Aggregates/MigrationStrategy.cs  
**Description:** Represents a strategy for migrating configurations from a legacy system. It includes mapping rules and validation procedures.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** MigrationStrategy  
**Type:** AggregateRoot  
**Relative Path:** Domain/Aggregates  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Aggregate
    
**Members:**
    
    - **Name:** Id  
**Type:** MigrationStrategyId  
**Attributes:** public|private set  
    - **Name:** SourceSystem  
**Type:** string  
**Attributes:** public|private set  
    - **Name:** MappingRules  
**Type:** string  
**Attributes:** public|private set  
    - **Name:** ValidationScript  
**Type:** string  
**Attributes:** public|private set  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Configuration Migration Plan
    
**Requirement Ids:**
    
    - REQ-SAP-009
    
**Purpose:** To model a reusable plan for migrating client configurations from legacy systems.  
**Logic Description:** This aggregate stores the definition of a migration process. The actual execution logic that uses this data resides in the application layer.  
**Documentation:**
    
    - **Summary:** Encapsulates the rules and procedures for migrating client configurations from a specific source system.
    
**Namespace:** Opc.System.Services.Management.Domain.Aggregates  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Management/Management.Domain/Repositories/IOpcClientInstanceRepository.cs  
**Description:** Defines the contract for data access operations related to the OpcClientInstance aggregate. This abstraction decouples the domain from specific data persistence technologies.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IOpcClientInstanceRepository  
**Type:** RepositoryInterface  
**Relative Path:** Domain/Repositories  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Repository
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - ClientInstanceId id
    - CancellationToken cancellationToken
    
**Return Type:** Task<OpcClientInstance>  
**Attributes:**   
    - **Name:** AddAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** UpdateAsync  
**Parameters:**
    
    - OpcClientInstance clientInstance
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** GetAllAsync  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<OpcClientInstance>>  
**Attributes:**   
    - **Name:** GetByIdsAsync  
**Parameters:**
    
    - IEnumerable<ClientInstanceId> ids
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<OpcClientInstance>>  
**Attributes:**   
    
**Implemented Features:**
    
    - Client Persistence Contract
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-6-001
    
**Purpose:** To provide an abstraction for storing and retrieving OpcClientInstance aggregates, isolating the domain model from data access concerns.  
**Logic Description:** This interface defines the essential persistence operations required by the application layer to manage OpcClientInstance aggregates. It includes methods for finding, adding, and updating instances, as well as bulk retrieval for bulk operations.  
**Documentation:**
    
    - **Summary:** An interface specifying the data access methods for OpcClientInstance aggregates. Implementations will handle the actual database interactions.
    
**Namespace:** Opc.System.Services.Management.Domain.Repositories  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Management/Management.Application/ClientInstances/Commands/UpdateClientConfiguration/UpdateClientConfigurationCommand.cs  
**Description:** Represents the command to update the configuration of a specific OPC client instance. It carries the necessary data to perform the action.  
**Template:** C# Record  
**Dependency Level:** 1  
**Name:** UpdateClientConfigurationCommand  
**Type:** Command  
**Relative Path:** Application/ClientInstances/Commands/UpdateClientConfiguration  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - Command
    
**Members:**
    
    - **Name:** ClientInstanceId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** NewConfiguration  
**Type:** ClientConfigurationDto  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Client Configuration Update Use Case
    
**Requirement Ids:**
    
    - REQ-6-001
    
**Purpose:** To encapsulate the intent and data required to update an OPC client's configuration.  
**Logic Description:** This is a simple data-carrying record. It will be created by the API layer from an incoming request and sent to the corresponding command handler for processing.  
**Documentation:**
    
    - **Summary:** A command DTO that holds the ID of the client to be updated and its new configuration settings.
    
**Namespace:** Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Application/ClientInstances/Commands/UpdateClientConfiguration/UpdateClientConfigurationCommandHandler.cs  
**Description:** Handles the logic for the UpdateClientConfigurationCommand. It orchestrates the domain model and repository to fulfill the use case.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** UpdateClientConfigurationCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/ClientInstances/Commands/UpdateClientConfiguration  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - CommandHandler
    
**Members:**
    
    - **Name:** _clientInstanceRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    - **Name:** _unitOfWork  
**Type:** IUnitOfWork  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - UpdateClientConfigurationCommand command
    - CancellationToken cancellationToken
    
**Return Type:** Task<Result>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Client Configuration Update Logic
    
**Requirement Ids:**
    
    - REQ-6-001
    
**Purpose:** To execute the business logic for updating a client's configuration.  
**Logic Description:** The handler will fetch the OpcClientInstance aggregate from the repository using the provided ID. It will then call the UpdateConfiguration method on the aggregate, passing in the new configuration. Finally, it will use the repository to persist the changes and commit the transaction via the unit of work.  
**Documentation:**
    
    - **Summary:** This handler processes a command to update a client's configuration. It retrieves the client aggregate, invokes its business logic, and saves the result.
    
**Namespace:** Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Application/ClientInstances/Commands/ExecuteBulkUpdate/ExecuteBulkUpdateCommand.cs  
**Description:** A command to apply a new configuration to multiple client instances simultaneously.  
**Template:** C# Record  
**Dependency Level:** 1  
**Name:** ExecuteBulkUpdateCommand  
**Type:** Command  
**Relative Path:** Application/ClientInstances/Commands/ExecuteBulkUpdate  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - Command
    
**Members:**
    
    - **Name:** ClientInstanceIds  
**Type:** IEnumerable<Guid>  
**Attributes:** public  
    - **Name:** ConfigurationToApply  
**Type:** ClientConfigurationDto  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Bulk Configuration Update Use Case
    
**Requirement Ids:**
    
    - REQ-6-002
    - REQ-9-005
    
**Purpose:** To encapsulate the data needed for a bulk configuration update operation.  
**Logic Description:** This record holds a list of client IDs and the common configuration that should be applied to all of them.  
**Documentation:**
    
    - **Summary:** A command that triggers the application of a configuration to a specified list of client instances.
    
**Namespace:** Opc.System.Services.Management.Application.ClientInstances.Commands.ExecuteBulkUpdate  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Application/ClientInstances/Commands/ExecuteBulkUpdate/ExecuteBulkUpdateCommandHandler.cs  
**Description:** Handles the logic for applying a configuration to multiple clients. This implements the BulkOperationHandler component's logic.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** ExecuteBulkUpdateCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/ClientInstances/Commands/ExecuteBulkUpdate  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - CommandHandler
    
**Members:**
    
    - **Name:** _clientInstanceRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    - **Name:** _unitOfWork  
**Type:** IUnitOfWork  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ExecuteBulkUpdateCommand command
    - CancellationToken cancellationToken
    
**Return Type:** Task<BulkUpdateResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Bulk Configuration Update Logic
    
**Requirement Ids:**
    
    - REQ-6-002
    - REQ-9-005
    
**Purpose:** To orchestrate the update of multiple client instances in a single transaction.  
**Logic Description:** The handler will retrieve all specified OpcClientInstance aggregates from the repository. It will iterate through each one, calling its UpdateConfiguration method. After processing all clients, it will persist all changes via the repository and commit the transaction. It will return a result summarizing which updates succeeded or failed.  
**Documentation:**
    
    - **Summary:** Processes a bulk update command, applying a new configuration to a list of client aggregates and saving the changes.
    
**Namespace:** Opc.System.Services.Management.Application.ClientInstances.Commands.ExecuteBulkUpdate  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Application/ClientInstances/Queries/GetAllClients/GetAllClientsQueryHandler.cs  
**Description:** Handles the request to retrieve a summary of all managed OPC client instances, including their health status.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** GetAllClientsQueryHandler  
**Type:** QueryHandler  
**Relative Path:** Application/ClientInstances/Queries/GetAllClients  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - QueryHandler
    
**Members:**
    
    - **Name:** _clientInstanceRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - GetAllClientsQuery query
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<ClientSummaryDto>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Client Monitoring
    - KPI Aggregation
    
**Requirement Ids:**
    
    - REQ-6-001
    - REQ-6-002
    - REQ-9-004
    - REQ-9-005
    
**Purpose:** To provide the data needed for the main management dashboard view.  
**Logic Description:** This handler queries the repository for all OpcClientInstance aggregates. It then maps the domain entities to a lightweight DTO (ClientSummaryDto) suitable for display on the dashboard. The DTO would include ID, name, health status, and last seen timestamp. This logic contributes to the ClientHealthAggregator component's responsibilities by providing aggregated views.  
**Documentation:**
    
    - **Summary:** Retrieves all managed client instances and maps them to a summary DTO for display in monitoring dashboards.
    
**Namespace:** Opc.System.Services.Management.Application.ClientInstances.Queries.GetAllClients  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Application/Migration/Commands/ImportConfiguration/ImportConfigurationCommandHandler.cs  
**Description:** Handles the command for importing a legacy client configuration, parsing it, and creating a new OpcClientInstance.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** ImportConfigurationCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/Migration/Commands/ImportConfiguration  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - CQRS
    - Factory
    
**Members:**
    
    - **Name:** _parserFactory  
**Type:** IConfigurationParserFactory  
**Attributes:** private|readonly  
    - **Name:** _clientInstanceRepository  
**Type:** IOpcClientInstanceRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ImportConfigurationCommand command
    - CancellationToken cancellationToken
    
**Return Type:** Task<Result<Guid>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Legacy Configuration Import
    
**Requirement Ids:**
    
    - REQ-SAP-009
    
**Purpose:** To implement the business logic for migrating a client configuration from a file.  
**Logic Description:** The handler will use a factory to get the correct parser for the file type (e.g., CSV, XML). It will use the parser to transform the file content into a ClientConfiguration domain object. It then creates a new OpcClientInstance aggregate using this configuration and saves it to the repository. This embodies the migration strategy logic.  
**Documentation:**
    
    - **Summary:** Orchestrates the process of parsing a legacy configuration file, creating a new client aggregate, and persisting it.
    
**Namespace:** Opc.System.Services.Management.Application.Migration.Commands.ImportConfiguration  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Management/Management.Infrastructure/Persistence/ManagementDbContext.cs  
**Description:** The Entity Framework Core DbContext for the Management service. It defines the database schema and provides the session for database operations.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** ManagementDbContext  
**Type:** DbContext  
**Relative Path:** Infrastructure/Persistence  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - UnitOfWork
    
**Members:**
    
    - **Name:** OpcClientInstances  
**Type:** DbSet<OpcClientInstance>  
**Attributes:** public  
    - **Name:** MigrationStrategies  
**Type:** DbSet<MigrationStrategy>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder modelBuilder
    
**Return Type:** void  
**Attributes:** protected override  
    
**Implemented Features:**
    
    - Database Schema Definition
    - Unit of Work
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** To manage the database connection and map domain objects to relational database tables.  
**Logic Description:** This class inherits from EF Core's DbContext. It includes DbSet properties for each aggregate root in the domain. The OnModelCreating method is used to apply entity configurations from the same assembly, defining table names, relationships, and constraints.  
**Documentation:**
    
    - **Summary:** The EF Core database context for the Management microservice, responsible for all interactions with the relational database.
    
**Namespace:** Opc.System.Services.Management.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Management/Management.Infrastructure/Persistence/Repositories/OpcClientInstanceRepository.cs  
**Description:** The EF Core implementation of the IOpcClientInstanceRepository interface. Handles the actual CRUD operations against the database.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** OpcClientInstanceRepository  
**Type:** Repository  
**Relative Path:** Infrastructure/Persistence/Repositories  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - Repository
    
**Members:**
    
    - **Name:** _context  
**Type:** ManagementDbContext  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - ClientInstanceId id
    - CancellationToken cancellationToken
    
**Return Type:** Task<OpcClientInstance>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Client Persistence Implementation
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** To provide a concrete implementation for persisting and retrieving OpcClientInstance aggregates using Entity Framework Core.  
**Logic Description:** Implements the methods defined in IOpcClientInstanceRepository. It uses the ManagementDbContext to query and save data. For example, GetByIdAsync will use _context.OpcClientInstances.FindAsync(id). It ensures that domain logic remains separate from EF Core specifics.  
**Documentation:**
    
    - **Summary:** Provides data access methods for OpcClientInstance aggregates, translating repository calls into EF Core database queries.
    
**Namespace:** Opc.System.Services.Management.Infrastructure.Persistence.Repositories  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Management/Management.Api/Controllers/ClientInstancesController.cs  
**Description:** REST API controller for managing and monitoring OPC client instances. This is a primary entry point for the management UI.  
**Template:** C# Controller  
**Dependency Level:** 3  
**Name:** ClientInstancesController  
**Type:** Controller  
**Relative Path:** Api/Controllers  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - MVC Controller
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetAllClientSummaries  
**Parameters:**
    
    - CancellationToken cancellationToken
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetClientDetails  
**Parameters:**
    
    - Guid id
    - CancellationToken cancellationToken
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** UpdateConfiguration  
**Parameters:**
    
    - Guid id
    - UpdateConfigurationRequest request
    - CancellationToken cancellationToken
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Centralized Client Configuration
    - Client Monitoring API
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-6-001
    - REQ-9-004
    
**Purpose:** To expose client management and monitoring functionalities over a RESTful HTTP API.  
**Logic Description:** This controller receives HTTP requests, validates them, and translates them into Commands or Queries. It uses MediatR's ISender to dispatch these to the appropriate handlers in the application layer. It then formats the handler's response into an HTTP response (e.g., Ok, NotFound, BadRequest). This implements the ClientConfigurationApiEndpoints and ClientMonitoringApiEndpoints components.  
**Documentation:**
    
    - **Summary:** Provides REST endpoints for retrieving client summaries, getting detailed configurations, and updating client settings.
    
**Namespace:** Opc.System.Services.Management.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Management/Management.Api/Controllers/BulkOperationsController.cs  
**Description:** REST API controller for executing bulk operations on multiple client instances, such as applying a common configuration.  
**Template:** C# Controller  
**Dependency Level:** 3  
**Name:** BulkOperationsController  
**Type:** Controller  
**Relative Path:** Api/Controllers  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - MVC Controller
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ApplyBulkConfiguration  
**Parameters:**
    
    - BulkUpdateRequest request
    - CancellationToken cancellationToken
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Bulk Operations API
    
**Requirement Ids:**
    
    - REQ-6-002
    - REQ-9-005
    
**Purpose:** To provide an API endpoint for initiating bulk configuration updates.  
**Logic Description:** Receives a request containing a list of client IDs and a configuration payload. It creates an ExecuteBulkUpdateCommand and dispatches it. The result from the handler, which details the success or failure for each client, is returned in the HTTP response.  
**Documentation:**
    
    - **Summary:** Exposes an endpoint for applying a configuration template to multiple client instances in a single API call.
    
**Namespace:** Opc.System.Services.Management.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Management/Management.Api/Program.cs  
**Description:** The main entry point for the Management microservice. Configures and launches the ASP.NET Core host.  
**Template:** C# Program  
**Dependency Level:** 4  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:** Api  
**Repository Id:** REPO-SAP-004  
**Pattern Ids:**
    
    - HostBuilder
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Service Bootstrap
    - Dependency Injection Setup
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** To initialize and run the Management microservice application.  
**Logic Description:** Configures the web application builder. This includes setting up dependency injection for all layers (e.g., registering repositories, command/query handlers), configuring logging, adding controllers, setting up the EF Core DbContext with its connection string, and configuring authentication and authorization middleware. Finally, it builds and runs the application.  
**Documentation:**
    
    - **Summary:** The bootstrap file for the Management service. It wires up all dependencies, configurations, and middleware before starting the web host.
    
**Namespace:** Opc.System.Services.Management.Api  
**Metadata:**
    
    - **Category:** Presentation
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableLegacyXmlMigration
  - EnableRealtimeHealthAggregation
  
- **Database Configs:**
  
  - ConnectionStrings:ManagementDb
  


---

