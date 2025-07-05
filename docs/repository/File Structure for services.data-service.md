# Specification

# 1. Files

- **Path:** services/data-service/src/DataService.Api/DataService.Api.csproj  
**Description:** The main project file for the Data Service API. It defines the project as an SDK-style .NET project, targets .NET 8, and includes references to other projects in the solution (Application, Infrastructure) and necessary NuGet packages like Swashbuckle for API documentation and Serilog for logging.  
**Template:** C# Project File  
**Dependency Level:** 3  
**Name:** DataService.Api  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - API Hosting
    - Dependency Management
    
**Requirement Ids:**
    
    
**Purpose:** Defines all dependencies, project references, and build configurations for the API layer of the Data Service microservice.  
**Logic Description:** This file configures the project's target framework to .NET 8. It adds project references to DataService.Application and DataService.Infrastructure to enable the dependency flow of Clean Architecture. It also lists package references for ASP.NET Core hosting, API documentation, and logging.  
**Documentation:**
    
    - **Summary:** Specifies the build and dependency settings for the main executable project of the Data Service microservice.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/data-service/src/DataService.Api/Program.cs  
**Description:** The main entry point for the Data Service application. This file configures and bootstraps the ASP.NET Core host, setting up dependency injection, configuring middleware pipelines (like exception handling, routing, authentication, authorization), and registering services from other layers.  
**Template:** C# Entry Point  
**Dependency Level:** 4  
**Name:** Program  
**Type:** Configuration  
**Relative Path:** DataService.Api  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    - Layered Architecture
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Application Bootstrap
    - Dependency Injection Setup
    - Middleware Configuration
    
**Requirement Ids:**
    
    
**Purpose:** Initializes and configures the web application host, wiring together all the services and components of the microservice.  
**Logic Description:** The logic creates a WebApplicationBuilder, registers application and infrastructure services by calling their respective extension methods. It configures logging with Serilog, adds controllers, sets up API documentation with Swashbuckle, and configures the HTTP request pipeline. It also registers and starts hosted services for message consumption.  
**Documentation:**
    
    - **Summary:** Bootstraps the Data Service API, configuring all services, dependencies, and the HTTP request processing pipeline.
    
**Namespace:** DataService.Api  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** services/data-service/src/DataService.Api/Controllers/HistoricalDataController.cs  
**Description:** API controller for handling requests related to historical data. It receives HTTP requests, translates them into application queries, and returns the results as JSON. This is a primary data query endpoint.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** HistoricalDataController  
**Type:** Controller  
**Relative Path:** DataService.Api/Controllers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetHistoricalData  
**Parameters:**
    
    - [FromQuery] GetHistoricalDataQuery query
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Historical Data Querying
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** Exposes HTTP endpoints for querying historical time-series data stored in the system.  
**Logic Description:** This controller uses constructor injection to get an instance of MediatR's ISender. The GET endpoint receives query parameters, maps them to a GetHistoricalDataQuery object, sends the query through MediatR to the corresponding handler in the Application layer, and returns the DTO result with an appropriate HTTP status code.  
**Documentation:**
    
    - **Summary:** Provides RESTful endpoints for clients to query historical process data.
    
**Namespace:** DataService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** services/data-service/src/DataService.Api/Controllers/ConfigurationsController.cs  
**Description:** API controller for managing system and user configurations stored in the relational database.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** ConfigurationsController  
**Type:** Controller  
**Relative Path:** DataService.Api/Controllers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetConfiguration  
**Parameters:**
    
    - string key
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** SaveConfiguration  
**Parameters:**
    
    - SaveConfigurationCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Configuration Management
    
**Requirement Ids:**
    
    - REQ-DLP-008
    
**Purpose:** Exposes HTTP endpoints for reading and writing key-value based user configurations.  
**Logic Description:** Uses MediatR to dispatch GetConfigurationQuery and SaveConfigurationCommand to the Application layer. It handles HTTP request/response serialization and returns appropriate status codes based on the outcome of the operations.  
**Documentation:**
    
    - **Summary:** Provides RESTful endpoints for managing user configurations stored in the relational database.
    
**Namespace:** DataService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** services/data-service/src/DataService.Api/Controllers/DataRetentionController.cs  
**Description:** API controller for managing data retention policies. Allows administrators to define and update rules for data archival and purging.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** DataRetentionController  
**Type:** Controller  
**Relative Path:** DataService.Api/Controllers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetPolicies  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** UpdatePolicy  
**Parameters:**
    
    - UpdatePolicyCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Data Retention Policy Management
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** Exposes HTTP endpoints for administrators to configure data lifecycle management policies.  
**Logic Description:** This controller provides endpoints to list all current retention policies and to create or update a specific policy. It uses MediatR to send commands and queries to the Application layer handlers for processing.  
**Documentation:**
    
    - **Summary:** Provides RESTful endpoints for configuring data retention policies for different data types.
    
**Namespace:** DataService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** services/data-service/src/DataService.Api/Messaging/DataIngestionConsumer.cs  
**Description:** A background service that consumes messages from a message queue (e.g., RabbitMQ or Kafka). It listens for new historical data points and alarm events published by the Core OPC Client Service.  
**Template:** C# Hosted Service  
**Dependency Level:** 4  
**Name:** DataIngestionConsumer  
**Type:** Consumer  
**Relative Path:** DataService.Api/Messaging  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Event-Driven Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ExecuteAsync  
**Parameters:**
    
    - CancellationToken stoppingToken
    
**Return Type:** Task  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Asynchronous Data Ingestion
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** Decouples the data ingestion process from the primary request-response flow by listening to a message queue for new data.  
**Logic Description:** Implements the IHostedService interface. In the ExecuteAsync method, it subscribes to specific topics on the message broker. Upon receiving a message, it deserializes the payload into a command object (e.g., StoreHistoricalDataBatchCommand) and dispatches it using MediatR for processing by the Application layer. Includes error handling and message acknowledgement logic.  
**Documentation:**
    
    - **Summary:** A hosted service that continuously listens for incoming historical and alarm data from a message bus and initiates the storage process.
    
**Namespace:** DataService.Api.Messaging  
**Metadata:**
    
    - **Category:** Integration
    
- **Path:** services/data-service/src/DataService.Application/Features/HistoricalData/Commands/StoreHistoricalDataBatchCommand.cs  
**Description:** Represents a command to store a batch of historical data points. This is part of the CQRS pattern.  
**Template:** C# Command  
**Dependency Level:** 1  
**Name:** StoreHistoricalDataBatchCommand  
**Type:** Command  
**Relative Path:** DataService.Application/Features/HistoricalData/Commands  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - CQRS
    
**Members:**
    
    - **Name:** DataPoints  
**Type:** IEnumerable<HistoricalDataDto>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** Encapsulates the data and intent for the use case of storing multiple historical data points.  
**Logic Description:** This is a plain C# record or class that implements MediatR's IRequest interface. It contains a collection of DTOs representing the historical data to be stored. It carries the necessary information for the handler to perform the action.  
**Documentation:**
    
    - **Summary:** A command object that holds a batch of historical data points intended for persistence.
    
**Namespace:** DataService.Application.Features.HistoricalData.Commands  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/data-service/src/DataService.Application/Features/HistoricalData/Commands/StoreHistoricalDataBatchCommandHandler.cs  
**Description:** The handler for the StoreHistoricalDataBatchCommand. It contains the business logic to process the command.  
**Template:** C# Command Handler  
**Dependency Level:** 2  
**Name:** StoreHistoricalDataBatchCommandHandler  
**Type:** Service  
**Relative Path:** DataService.Application/Features/HistoricalData/Commands  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - CQRS
    
**Members:**
    
    - **Name:** _timeSeriesRepository  
**Type:** ITimeSeriesRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - StoreHistoricalDataBatchCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<Unit>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Historical Data Persistence Logic
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** Orchestrates the storage of historical data by interacting with the time-series repository.  
**Logic Description:** This handler receives the command, maps the DTOs to domain entities, and then calls the AddBatchAsync method on the ITimeSeriesRepository to persist the data. It contains the orchestration logic for this specific use case, but not the data access implementation itself.  
**Documentation:**
    
    - **Summary:** Processes the command to store a batch of historical data by using the appropriate data repository.
    
**Namespace:** DataService.Application.Features.HistoricalData.Commands  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/data-service/src/DataService.Application/Features/DataRetention/Commands/UpdatePolicyCommandHandler.cs  
**Description:** The handler for updating or creating a data retention policy. It processes the command and persists the changes.  
**Template:** C# Command Handler  
**Dependency Level:** 2  
**Name:** UpdatePolicyCommandHandler  
**Type:** Service  
**Relative Path:** DataService.Application/Features/DataRetention/Commands  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - CQRS
    
**Members:**
    
    - **Name:** _policyRepository  
**Type:** IDataRetentionPolicyRepository  
**Attributes:** private|readonly  
    - **Name:** _unitOfWork  
**Type:** IUnitOfWork  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - UpdatePolicyCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<Unit>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Data Retention Policy Logic
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** Handles the business logic for creating or updating a data retention policy in the system.  
**Logic Description:** The handler retrieves the existing policy from the repository or creates a new one. It updates the properties based on the command data, performs any necessary validation, and then uses the repository to persist the changes. Finally, it commits the transaction using the Unit of Work.  
**Documentation:**
    
    - **Summary:** Processes a command to update a data retention policy and saves the changes to the database.
    
**Namespace:** DataService.Application.Features.DataRetention.Commands  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/data-service/src/DataService.Domain/Entities/UserConfiguration.cs  
**Description:** Domain entity representing a single user configuration setting. This will be stored in the relational database.  
**Template:** C# Entity  
**Dependency Level:** 0  
**Name:** UserConfiguration  
**Type:** Model  
**Relative Path:** DataService.Domain/Entities  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - Domain-Driven Design
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Key  
**Type:** string  
**Attributes:** public  
    - **Name:** Value  
**Type:** string  
**Attributes:** public  
    - **Name:** IsEncrypted  
**Type:** bool  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-008
    
**Purpose:** Defines the core structure and properties for a user configuration record.  
**Logic Description:** A plain C# class representing the UserConfiguration entity. It includes properties for its identifier, a key, a value, and a flag indicating if the value is encrypted, fulfilling part of REQ-DLP-008. This class contains no logic related to data access or infrastructure.  
**Documentation:**
    
    - **Summary:** Represents a key-value pair for user or system configuration, with an encryption flag.
    
**Namespace:** DataService.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** services/data-service/src/DataService.Domain/Entities/DataRetentionPolicy.cs  
**Description:** Domain entity representing a data retention policy for a specific data type (e.g., Historical, Alarm, Audit).  
**Template:** C# Entity  
**Dependency Level:** 0  
**Name:** DataRetentionPolicy  
**Type:** Model  
**Relative Path:** DataService.Domain/Entities  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - Domain-Driven Design
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** DataType  
**Type:** DataType  
**Attributes:** public  
    - **Name:** RetentionPeriodDays  
**Type:** int  
**Attributes:** public  
    - **Name:** Action  
**Type:** RetentionAction  
**Attributes:** public  
    - **Name:** ArchiveLocation  
**Type:** string  
**Attributes:** public|nullable  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** Defines the structure and business rules for a single data retention policy.  
**Logic Description:** This entity class defines the properties for a retention policy, including the type of data it applies to, the retention duration in days, and the action to take when the period expires (e.g., Purge, Archive). This directly models the core concept of REQ-DLP-017.  
**Documentation:**
    
    - **Summary:** Represents a rule for managing the lifecycle of a specific type of data, including how long to keep it and what to do with it afterward.
    
**Namespace:** DataService.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** services/data-service/src/DataService.Domain/Repositories/ITimeSeriesRepository.cs  
**Description:** Interface (contract) for a repository that handles operations for time-series data, like historical data and alarms. This abstracts the data access technology.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** ITimeSeriesRepository  
**Type:** Repository  
**Relative Path:** DataService.Domain/Repositories  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddHistoricalDataBatchAsync  
**Parameters:**
    
    - IEnumerable<HistoricalDataPoint> dataPoints
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** QueryHistoricalDataAsync  
**Parameters:**
    
    - Guid tagId
    - DateTimeOffset start
    - DateTimeOffset end
    
**Return Type:** Task<IEnumerable<HistoricalDataPoint>>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** To define a consistent contract for accessing time-series data, decoupling the application logic from the specific time-series database implementation.  
**Logic Description:** This interface defines methods for common time-series operations, such as writing a batch of data points and querying data within a specific time range for a given tag. The Application layer will depend on this interface, not its concrete implementation.  
**Documentation:**
    
    - **Summary:** Defines the contract for all persistence operations related to time-series data.
    
**Namespace:** DataService.Domain.Repositories  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** services/data-service/src/DataService.Domain/Repositories/IDataRetentionPolicyRepository.cs  
**Description:** Interface for a repository that manages DataRetentionPolicy entities.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IDataRetentionPolicyRepository  
**Type:** Repository  
**Relative Path:** DataService.Domain/Repositories  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetAllAsync  
**Parameters:**
    
    
**Return Type:** Task<IEnumerable<DataRetentionPolicy>>  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - DataRetentionPolicy policy
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** To define the contract for persistence operations related to data retention policies.  
**Logic Description:** This interface abstracts the data access for DataRetentionPolicy entities, allowing the Application layer to manage policies without knowing about the underlying relational database.  
**Documentation:**
    
    - **Summary:** Defines the contract for persistence operations related to DataRetentionPolicy entities.
    
**Namespace:** DataService.Domain.Repositories  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** services/data-service/src/DataService.Infrastructure/Persistence/Relational/AppDbContext.cs  
**Description:** The Entity Framework Core DbContext for the relational database (PostgreSQL). It defines the DbSets for all entities stored in the relational DB and configures their relationships and mappings.  
**Template:** C# DbContext  
**Dependency Level:** 2  
**Name:** AppDbContext  
**Type:** DataAccess  
**Relative Path:** DataService.Infrastructure/Persistence/Relational  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - UnitOfWork
    
**Members:**
    
    - **Name:** UserConfigurations  
**Type:** DbSet<UserConfiguration>  
**Attributes:** public  
    - **Name:** DataRetentionPolicies  
**Type:** DbSet<DataRetentionPolicy>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder modelBuilder
    
**Return Type:** void  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Relational Data Mapping
    - Unit of Work Implementation
    
**Requirement Ids:**
    
    - REQ-DLP-008
    - REQ-DLP-017
    
**Purpose:** Manages the connection to the relational database and provides the mechanism for querying and saving entities via EF Core.  
**Logic Description:** This class inherits from EF Core's DbContext. It includes DbSet properties for each relational entity. The OnModelCreating method is used to apply entity type configurations from separate classes, keeping the model definition clean. It also acts as the Unit of Work for relational transactions.  
**Documentation:**
    
    - **Summary:** Represents a session with the PostgreSQL database, allowing for querying and saving of entities.
    
**Namespace:** DataService.Infrastructure.Persistence.Relational  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/data-service/src/DataService.Infrastructure/Persistence/TimeSeries/TimeSeriesRepository.cs  
**Description:** The concrete implementation of the ITimeSeriesRepository interface. This class interacts directly with the time-series database client (e.g., InfluxDB.Client) to perform data operations.  
**Template:** C# Repository  
**Dependency Level:** 2  
**Name:** TimeSeriesRepository  
**Type:** Repository  
**Relative Path:** DataService.Infrastructure/Persistence/TimeSeries  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Layered Architecture
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _influxDbClient  
**Type:** IInfluxDBClient  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** AddHistoricalDataBatchAsync  
**Parameters:**
    
    - IEnumerable<HistoricalDataPoint> dataPoints
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** QueryHistoricalDataAsync  
**Parameters:**
    
    - Guid tagId
    - DateTimeOffset start
    - DateTimeOffset end
    
**Return Type:** Task<IEnumerable<HistoricalDataPoint>>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Time-Series Data Access
    
**Requirement Ids:**
    
    - REQ-DLP-001
    
**Purpose:** Provides the concrete implementation for storing and retrieving time-series data from InfluxDB.  
**Logic Description:** This class implements the ITimeSeriesRepository. The methods will use the injected InfluxDB client to construct and execute write API calls for batch data and Flux queries for reading data. It handles the mapping between the domain objects and the InfluxDB data structures (Points and query results).  
**Documentation:**
    
    - **Summary:** Implements the data access logic for time-series data using the InfluxDB client library.
    
**Namespace:** DataService.Infrastructure.Persistence.TimeSeries  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/data-service/src/DataService.Worker/DataService.Worker.csproj  
**Description:** The project file for the background worker service. It defines the project as a .NET Worker Service and includes references to the Application and Infrastructure layers.  
**Template:** C# Project File  
**Dependency Level:** 3  
**Name:** DataService.Worker  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Background Job Hosting
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** Defines the build and dependency settings for the standalone background worker process.  
**Logic Description:** This file configures the project to produce an executable worker. It references the Application and Infrastructure projects to allow the background jobs to access the necessary business logic and data access implementations.  
**Documentation:**
    
    - **Summary:** Specifies build and dependency settings for the Data Service's background worker component.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/data-service/src/DataService.Worker/Workers/DataRetentionWorker.cs  
**Description:** A background service (IHostedService) that periodically enforces data retention policies. It queries for expired data and performs the configured action (purge or archive).  
**Template:** C# Hosted Service  
**Dependency Level:** 4  
**Name:** DataRetentionWorker  
**Type:** Worker  
**Relative Path:** DataService.Worker/Workers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _policyRepository  
**Type:** IDataRetentionPolicyRepository  
**Attributes:** private|readonly  
    - **Name:** _timeSeriesRepository  
**Type:** ITimeSeriesRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ExecuteAsync  
**Parameters:**
    
    - CancellationToken stoppingToken
    
**Return Type:** Task  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Automated Data Archival
    - Automated Data Purging
    
**Requirement Ids:**
    
    - REQ-DLP-017
    
**Purpose:** Implements the automated, long-running process for managing the data lifecycle according to defined policies.  
**Logic Description:** This class inherits from BackgroundService. The ExecuteAsync method contains a loop that runs periodically (e.g., once a day). In each iteration, it fetches all active retention policies from the repository. For each policy, it calculates the cutoff date and calls the appropriate repository methods to delete or archive data older than that date. All actions are logged.  
**Documentation:**
    
    - **Summary:** A background worker that periodically applies configured data retention policies to purge or archive old data from the persistence stores.
    
**Namespace:** DataService.Worker.Workers  
**Metadata:**
    
    - **Category:** Application
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableDataArchiving
  - EnableSensitiveDataEncryption
  
- **Database Configs:**
  
  - ConnectionStrings:PostgresConnection
  - InfluxDb:Url
  - InfluxDb:Token
  - InfluxDb:Org
  - InfluxDb:BucketHistorical
  - InfluxDb:BucketAlarms
  


---

