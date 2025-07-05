# Specification

# 1. Files

- **Path:** src/Services/Reporting/Reporting.API/Reporting.API.csproj  
**Description:** The C# project file for the Reporting Service's API layer, defining its framework, dependencies, and project references.  
**Template:** C# Project File  
**Dependency Level:** 4  
**Name:** Reporting.API  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the Reporting API project, referencing the Application and Infrastructure layers to build the executable microservice.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Manages project dependencies, including other project layers (Application, Infrastructure) and NuGet packages for ASP.NET Core, Swagger, and API functionalities.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Reporting/Reporting.API/Program.cs  
**Description:** The main entry point for the Reporting Service application. Configures the ASP.NET Core host, dependency injection, and the HTTP request processing pipeline.  
**Template:** C# Program  
**Dependency Level:** 5  
**Name:** Program  
**Type:** Entrypoint  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
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
    
    - Application Bootstrap
    - Middleware Configuration
    
**Requirement Ids:**
    
    - REQ-7-019
    - REQ-7-020
    - REQ-7-021
    
**Purpose:** Initializes and runs the reporting microservice, setting up all necessary configurations and services.  
**Logic Description:** Creates a WebApplicationBuilder. Calls extension methods from the Application and Infrastructure layers to register their services via dependency injection. Configures middleware for the HTTP pipeline, such as Swagger/OpenAPI, authentication, authorization, global exception handling, and routing. Maps the API controllers. Runs the application.  
**Documentation:**
    
    - **Summary:** The bootstrap file for the microservice. It wires up all the architectural layers and configures the runtime environment.
    
**Namespace:** Reporting.API  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.API/appsettings.Development.json  
**Description:** Configuration file for the development environment, containing settings like database connection strings, external service URLs, and logging levels.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings.Development  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Provides environment-specific settings for development, allowing developers to run the service locally without using production credentials.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Contains configuration keys for 'ConnectionStrings', 'JwtSettings', 'EmailSettings', 'AiServiceUrl', 'DataServiceUrl', and other development-time values.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services/Reporting/Reporting.API/Controllers/ReportTemplatesController.cs  
**Description:** RESTful API controller for managing report templates. Provides endpoints for creating, retrieving, updating, and deleting templates.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** ReportTemplatesController  
**Type:** Controller  
**Relative Path:** Controllers  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** CreateTemplate  
**Parameters:**
    
    - CreateReportTemplateRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetTemplateById  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** ListTemplates  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** UpdateTemplate  
**Parameters:**
    
    - Guid id
    - UpdateReportTemplateRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** DeleteTemplate  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Template Management
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Exposes HTTP endpoints to allow users or other systems to manage the lifecycle of report templates.  
**Logic Description:** This controller receives HTTP requests and their payloads (DTOs). It uses MediatR's ISender to dispatch corresponding commands (Create, Update, Delete) or queries (Get, List) to the application layer. It then maps the results from the application layer back to HTTP responses (e.g., 201 Created, 200 OK, 404 Not Found).  
**Documentation:**
    
    - **Summary:** Handles all incoming HTTP requests related to report templates, acting as the primary interface for template configuration.
    
**Namespace:** Reporting.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Reporting/Reporting.API/Controllers/ReportsController.cs  
**Description:** RESTful API controller for generating and retrieving reports. Provides endpoints to trigger on-demand generation, check status, and download completed reports.  
**Template:** C# Controller  
**Dependency Level:** 4  
**Name:** ReportsController  
**Type:** Controller  
**Relative Path:** Controllers  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateReportOnDemand  
**Parameters:**
    
    - Guid templateId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetReportStatus  
**Parameters:**
    
    - Guid reportId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** DownloadReport  
**Parameters:**
    
    - Guid reportId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - On-Demand Report Generation
    - Report Retrieval
    
**Requirement Ids:**
    
    - REQ-7-018
    - REQ-7-020
    
**Purpose:** Exposes HTTP endpoints for initiating and accessing generated reports.  
**Logic Description:** Receives HTTP requests to trigger report generation by dispatching a command via MediatR. Provides endpoints to query the status of a generation job and to download the final report file once it is ready. The download endpoint will return a FileResult with the appropriate content type.  
**Documentation:**
    
    - **Summary:** Handles all incoming HTTP requests related to the generation and retrieval of specific report instances.
    
**Namespace:** Reporting.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Reporting/Reporting.Application/Reporting.Application.csproj  
**Description:** The C# project file for the Reporting Service's Application layer. Contains references to the Domain layer and libraries like MediatR.  
**Template:** C# Project File  
**Dependency Level:** 2  
**Name:** Reporting.Application  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the application project, containing use case logic and orchestration, while remaining independent of UI and infrastructure concerns.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Manages project dependencies for the application layer, ensuring it only references the Domain layer and necessary abstraction libraries.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Reporting/Reporting.Application/Templates/Commands/CreateReportTemplate/CreateReportTemplateCommand.cs  
**Description:** Command object representing the intent to create a new report template. Contains all necessary data for creation.  
**Template:** C# Record  
**Dependency Level:** 2  
**Name:** CreateReportTemplateCommand  
**Type:** Command  
**Relative Path:** Templates/Commands/CreateReportTemplate  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** DataSources  
**Type:** List<DataSourceDto>  
**Attributes:** public  
    - **Name:** Layout  
**Type:** LayoutConfigurationDto  
**Attributes:** public  
    - **Name:** Branding  
**Type:** BrandingDto  
**Attributes:** public  
    - **Name:** DefaultFormat  
**Type:** string  
**Attributes:** public  
    - **Name:** Schedule  
**Type:** string?  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-019
    - REQ-7-020
    
**Purpose:** Encapsulates the data required to execute the use case of creating a new report template.  
**Logic Description:** This is a plain data record that implements IRequest<Guid> from MediatR. It carries the necessary information from the API layer to the command handler.  
**Documentation:**
    
    - **Summary:** A Data Transfer Object for the create report template command.
    
**Namespace:** Reporting.Application.Templates.Commands.CreateReportTemplate  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Application/Templates/Commands/CreateReportTemplate/CreateReportTemplateCommandHandler.cs  
**Description:** The handler for the CreateReportTemplateCommand. Contains the business logic to process the creation request.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** CreateReportTemplateCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Templates/Commands/CreateReportTemplate  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** _reportTemplateRepository  
**Type:** IReportTemplateRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - CreateReportTemplateCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<Guid>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Template Creation Logic
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Orchestrates the creation of a ReportTemplate aggregate and persists it.  
**Logic Description:** The handler receives the command. It validates the input data. It uses a factory or the aggregate's constructor to create a new ReportTemplate domain object from the command's data. It then uses the IReportTemplateRepository to add the new aggregate to the database. Finally, it returns the ID of the newly created template.  
**Documentation:**
    
    - **Summary:** Implements the logic for the create report template use case.
    
**Namespace:** Reporting.Application.Templates.Commands.CreateReportTemplate  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Application/Generation/Commands/GenerateReportOnDemand/GenerateReportOnDemandCommand.cs  
**Description:** Command object representing the intent to generate a report immediately based on a template.  
**Template:** C# Record  
**Dependency Level:** 2  
**Name:** GenerateReportOnDemandCommand  
**Type:** Command  
**Relative Path:** Generation/Commands/GenerateReportOnDemand  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** TemplateId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** OutputFormat  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-018
    - REQ-7-020
    
**Purpose:** Encapsulates the data required to execute the use case of generating a report on demand.  
**Logic Description:** This record implements IRequest<Guid> from MediatR, carrying the template ID and desired format to the handler.  
**Documentation:**
    
    - **Summary:** A Data Transfer Object for the on-demand report generation command.
    
**Namespace:** Reporting.Application.Generation.Commands.GenerateReportOnDemand  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Application/Generation/Commands/GenerateReportOnDemand/GenerateReportOnDemandCommandHandler.cs  
**Description:** The handler for the GenerateReportOnDemandCommand. Orchestrates the entire report generation process.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** GenerateReportOnDemandCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Generation/Commands/GenerateReportOnDemand  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** _reportTemplateRepository  
**Type:** IReportTemplateRepository  
**Attributes:** private|readonly  
    - **Name:** _generatedReportRepository  
**Type:** IGeneratedReportRepository  
**Attributes:** private|readonly  
    - **Name:** _dataServiceClient  
**Type:** IDataServiceClient  
**Attributes:** private|readonly  
    - **Name:** _aiServiceClient  
**Type:** IAiServiceClient  
**Attributes:** private|readonly  
    - **Name:** _reportGeneratorFactory  
**Type:** IReportGeneratorFactory  
**Attributes:** private|readonly  
    - **Name:** _fileStorageService  
**Type:** IFileStorageService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - GenerateReportOnDemandCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<Guid>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Generation Orchestration
    
**Requirement Ids:**
    
    - REQ-7-018
    - REQ-7-020
    
**Purpose:** Coordinates fetching data, generating the report document, and saving it.  
**Logic Description:** 1. Fetches the ReportTemplate from the repository using the TemplateId. 2. Creates a new GeneratedReport entity with a 'Processing' status and saves it. 3. Based on the template's data source definitions, calls the IDataServiceClient and IAiServiceClient to fetch all required data. 4. Aggregates the collected data into a report data model. 5. Uses the IReportGeneratorFactory to get the correct generator (PDF, Excel, HTML) for the requested format. 6. Calls the generator with the report data model to create the report byte stream. 7. Uses the IFileStorageService to save the generated file. 8. Updates the GeneratedReport entity with 'Completed' status and the file path. 9. Returns the ID of the GeneratedReport.  
**Documentation:**
    
    - **Summary:** Implements the complex logic for generating a report, coordinating with multiple external services and internal components.
    
**Namespace:** Reporting.Application.Generation.Commands.GenerateReportOnDemand  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Application/Contracts/Infrastructure/IReportTemplateRepository.cs  
**Description:** Interface defining the contract for data access operations related to the ReportTemplate aggregate.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IReportTemplateRepository  
**Type:** Interface  
**Relative Path:** Contracts/Infrastructure  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - RepositoryPattern
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<ReportTemplate?>  
**Attributes:**   
    - **Name:** AddAsync  
**Parameters:**
    
    - ReportTemplate template
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** UpdateAsync  
**Parameters:**
    
    - ReportTemplate template
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** ListAllAsync  
**Parameters:**
    
    
**Return Type:** Task<List<ReportTemplate>>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Decouples the application layer from the specific data persistence technology used for report templates.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Defines the standard repository methods for CRUD operations on ReportTemplate entities.
    
**Namespace:** Reporting.Application.Contracts.Infrastructure  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/Reporting/Reporting.Application/Contracts/Services/IAiServiceClient.cs  
**Description:** Interface defining the contract for communicating with the external AI Service.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAiServiceClient  
**Type:** Interface  
**Relative Path:** Contracts/Services  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - DependencyInversion
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetAnomaliesForReportAsync  
**Parameters:**
    
    - DateTime startTime
    - DateTime endTime
    
**Return Type:** Task<List<AnomalyInsightDto>>  
**Attributes:**   
    - **Name:** GetMaintenancePredictionsAsync  
**Parameters:**
    
    - List<string> assetIds
    
**Return Type:** Task<List<MaintenanceInsightDto>>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-018
    
**Purpose:** Abstracts the communication details for fetching AI-driven insights, allowing the application layer to remain ignorant of gRPC/REST implementation details.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Defines methods to retrieve data, such as detected anomalies or predictive maintenance forecasts, from the AI microservice.
    
**Namespace:** Reporting.Application.Contracts.Services  
**Metadata:**
    
    - **Category:** Integration
    
- **Path:** src/Services/Reporting/Reporting.Domain/Reporting.Domain.csproj  
**Description:** The C# project file for the Reporting Service's Domain layer. It should have no dependencies on other project layers.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** Reporting.Domain  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the domain project, containing the core business logic, entities, and rules of the reporting bounded context.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Manages dependencies for the domain layer. This project should be as dependency-free as possible, containing only the core logic.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Reporting/Reporting.Domain/Aggregates/ReportTemplate.cs  
**Description:** The aggregate root for a report template. Encapsulates all information and rules related to a template's definition.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** ReportTemplate  
**Type:** AggregateRoot  
**Relative Path:** Aggregates  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - DDD-Aggregate
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public|private set  
    - **Name:** _dataSources  
**Type:** List<DataSource>  
**Attributes:** private  
    - **Name:** DataSources  
**Type:** IReadOnlyCollection<DataSource>  
**Attributes:** public  
    - **Name:** Schedule  
**Type:** Schedule?  
**Attributes:** public|private set  
    - **Name:** DefaultFormat  
**Type:** ReportFormat  
**Attributes:** public|private set  
    - **Name:** Branding  
**Type:** Branding  
**Attributes:** public|private set  
    
**Methods:**
    
    - **Name:** Create  
**Parameters:**
    
    - string name
    - List<DataSource> dataSources
    - ReportFormat defaultFormat
    
**Return Type:** ReportTemplate  
**Attributes:** public|static  
    - **Name:** UpdateName  
**Parameters:**
    
    - string newName
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UpdateSchedule  
**Parameters:**
    
    - Schedule? newSchedule
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** AddDataSource  
**Parameters:**
    
    - DataSource newSource
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Template Definition
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Models a single, consistent definition of a report, ensuring its internal state is always valid through its methods.  
**Logic Description:** Contains a static factory method for creation, enforcing initial validation rules. Public methods modify the state while ensuring business invariants are maintained (e.g., a template must always have a name). It holds a collection of DataSource entities and Branding/Schedule value objects.  
**Documentation:**
    
    - **Summary:** Represents the core business entity for a report template, containing all its configuration and business rules.
    
**Namespace:** Reporting.Domain.Aggregates  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Domain/Enums/ReportFormat.cs  
**Description:** Enumeration for the supported report output formats.  
**Template:** C# Enum  
**Dependency Level:** 0  
**Name:** ReportFormat  
**Type:** Enum  
**Relative Path:** Enums  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** PDF  
**Type:** enum  
**Attributes:**   
    - **Name:** Excel  
**Type:** enum  
**Attributes:**   
    - **Name:** HTML  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** Provides a strongly-typed representation of the possible report formats.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Defines the supported file formats for report generation: PDF, Excel, and HTML.
    
**Namespace:** Reporting.Domain.Enums  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Reporting.Infrastructure.csproj  
**Description:** The C# project file for the Reporting Service's Infrastructure layer. Contains dependencies for EF Core, database drivers, HTTP clients, and other external services.  
**Template:** C# Project File  
**Dependency Level:** 3  
**Name:** Reporting.Infrastructure  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the infrastructure project, which implements the contracts defined in the Application layer to interact with the outside world.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** Manages all NuGet packages and project references required for implementing data persistence, external service communication, and other infrastructure concerns.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Persistence/Repositories/ReportTemplateRepository.cs  
**Description:** EF Core implementation of the IReportTemplateRepository interface. Handles database operations for ReportTemplate aggregates.  
**Template:** C# Repository  
**Dependency Level:** 3  
**Name:** ReportTemplateRepository  
**Type:** Repository  
**Relative Path:** Persistence/Repositories  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _dbContext  
**Type:** ReportingDbContext  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<ReportTemplate?>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - ReportTemplate template
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Template Persistence
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Provides a concrete implementation for persisting and retrieving ReportTemplate aggregates using Entity Framework Core.  
**Logic Description:** Implements the methods defined in IReportTemplateRepository. Uses the ReportingDbContext to query and save data to the underlying database. It handles change tracking and saving operations. For GetByIdAsync, it may use .Include() to eagerly load related entities like DataSources.  
**Documentation:**
    
    - **Summary:** The data access implementation for ReportTemplate entities.
    
**Namespace:** Reporting.Infrastructure.Persistence.Repositories  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Persistence/ReportingDbContext.cs  
**Description:** The Entity Framework Core DbContext for the reporting service. Defines the DbSets for all aggregates and configures the database connection.  
**Template:** C# DbContext  
**Dependency Level:** 2  
**Name:** ReportingDbContext  
**Type:** DbContext  
**Relative Path:** Persistence  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - UnitOfWork
    
**Members:**
    
    - **Name:** ReportTemplates  
**Type:** DbSet<ReportTemplate>  
**Attributes:** public  
    - **Name:** GeneratedReports  
**Type:** DbSet<GeneratedReport>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder modelBuilder
    
**Return Type:** void  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Database Schema Definition
    
**Requirement Ids:**
    
    
**Purpose:** Represents a session with the database, allowing querying and saving of entities. It is the central point for EF Core configuration.  
**Logic Description:** Inherits from DbContext. Defines DbSet properties for each aggregate root that needs to be persisted. The OnModelCreating method is used to apply entity configurations from separate configuration classes or to define them inline, setting up primary keys, relationships, and value object conversions.  
**Documentation:**
    
    - **Summary:** Manages the database session and defines the entity-to-table mappings for the reporting service.
    
**Namespace:** Reporting.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Scheduling/Jobs/ScheduledReportJob.cs  
**Description:** A background job, designed to be executed by a scheduler like Hangfire, that triggers the generation of a scheduled report.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** ScheduledReportJob  
**Type:** BackgroundJob  
**Relative Path:** Scheduling/Jobs  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Execute  
**Parameters:**
    
    - Guid templateId
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Scheduled Report Trigger
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** Acts as the entry point for a scheduled task, decoupling the scheduler from the application's core logic.  
**Logic Description:** This class is instantiated and executed by the scheduling framework. Its Execute method receives the ID of the template to be generated. It then creates and dispatches a GenerateReportOnDemandCommand (or a similar command) using MediatR to kick off the generation process within the application layer.  
**Documentation:**
    
    - **Summary:** A simple job wrapper that translates a scheduled trigger into an application command.
    
**Namespace:** Reporting.Infrastructure.Scheduling.Jobs  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Services/Generation/PdfReportGenerator.cs  
**Description:** Implements the logic for generating a PDF report using the QuestPDF library.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** PdfReportGenerator  
**Type:** Service  
**Relative Path:** Services/Generation  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Generate  
**Parameters:**
    
    - ReportDataModel data
    
**Return Type:** byte[]  
**Attributes:** public  
    
**Implemented Features:**
    
    - PDF Report Generation
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** Translates a generic report data model into a specific PDF document.  
**Logic Description:** Implements a common IReportGenerator interface. The Generate method takes a structured data model. It uses the QuestPDF fluent API to define the document structure: header, footer, content sections, tables, and charts (as images). It populates the QuestPDF document with the data from the input model and then calls the library's method to generate the PDF as a byte array.  
**Documentation:**
    
    - **Summary:** A specialized service responsible for creating PDF report files.
    
**Namespace:** Reporting.Infrastructure.Services.Generation  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Services/Generation/ExcelReportGenerator.cs  
**Description:** Implements the logic for generating an Excel report using the ClosedXML library.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** ExcelReportGenerator  
**Type:** Service  
**Relative Path:** Services/Generation  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Generate  
**Parameters:**
    
    - ReportDataModel data
    
**Return Type:** byte[]  
**Attributes:** public  
    
**Implemented Features:**
    
    - Excel Report Generation
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** Translates a generic report data model into a specific Excel workbook.  
**Logic Description:** Implements a common IReportGenerator interface. The Generate method creates a new XLWorkbook using ClosedXML. It iterates through the input data model, creating worksheets, populating cells with data, applying formatting, and creating tables or charts. Finally, it saves the workbook to a memory stream and returns the resulting byte array.  
**Documentation:**
    
    - **Summary:** A specialized service responsible for creating Excel (.xlsx) report files.
    
**Namespace:** Reporting.Infrastructure.Services.Generation  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/DependencyInjection.cs  
**Description:** Extension method to register all infrastructure-layer services with the dependency injection container.  
**Template:** C# Static Class  
**Dependency Level:** 4  
**Name:** DependencyInjection  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddInfrastructureServices  
**Parameters:**
    
    - IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - Service Registration
    
**Requirement Ids:**
    
    
**Purpose:** Centralizes the registration of infrastructure components to keep the main Program.cs file clean.  
**Logic Description:** This static class contains an extension method for IServiceCollection. It registers the ReportingDbContext, repository implementations (e.g., services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>()), external service clients (HTTP or gRPC), and other infrastructure services like email and file storage. It reads necessary configuration like connection strings from the IConfiguration object.  
**Documentation:**
    
    - **Summary:** A setup class that encapsulates the dependency injection configuration for the Infrastructure layer.
    
**Namespace:** Reporting.Infrastructure  
**Metadata:**
    
    - **Category:** Configuration
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableScheduledReporting
  - EnableEventDrivenReporting
  
- **Database Configs:**
  
  - ConnectionStrings:ReportingDb
  - AiService:Url
  - DataService:Url
  - EmailSettings:SmtpHost
  - EmailSettings:SmtpPort
  - FileStorage:BasePath
  


---

