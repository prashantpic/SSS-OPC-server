# Specification

# 1. Files

- **Path:** src/Services/Reporting/Reporting.sln  
**Description:** Visual Studio Solution file for the Reporting microservice, containing references to all projects within this service.  
**Template:** .NET Solution  
**Dependency Level:** 0  
**Name:** Reporting  
**Type:** Solution  
**Relative Path:**   
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the project structure and dependencies for the Reporting microservice.  
**Logic Description:** This file groups the API, Application, Domain, and Infrastructure projects together for development and build purposes.  
**Documentation:**
    
    - **Summary:** Solution file that orchestrates the build process for the entire Reporting microservice.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services/Reporting/Reporting.Domain/Aggregates/ReportTemplate/ReportTemplate.cs  
**Description:** The root entity of the ReportTemplate aggregate. Encapsulates all business logic for creating and managing report templates, including their structure, data sources, and branding.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** ReportTemplate  
**Type:** AggregateRoot  
**Relative Path:** Reporting.Domain/Aggregates/ReportTemplate  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Aggregate
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Name  
**Type:** string  
**Attributes:** public  
    - **Name:** DataSources  
**Type:** IReadOnlyCollection<DataSource>  
**Attributes:** private  
    - **Name:** KpiDefinitions  
**Type:** IReadOnlyCollection<KpiDefinition>  
**Attributes:** private  
    - **Name:** ChartConfigurations  
**Type:** IReadOnlyCollection<ChartConfiguration>  
**Attributes:** private  
    - **Name:** Branding  
**Type:** Branding  
**Attributes:** public  
    - **Name:** DefaultDistributionTargets  
**Type:** IReadOnlyCollection<DistributionTarget>  
**Attributes:** private  
    
**Methods:**
    
    - **Name:** Create  
**Parameters:**
    
    - string name
    - Branding branding
    
**Return Type:** ReportTemplate  
**Attributes:** public static  
    - **Name:** UpdateDetails  
**Parameters:**
    
    - string name
    - Branding branding
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** AddDataSource  
**Parameters:**
    
    - DataSource dataSource
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UpdateChartConfiguration  
**Parameters:**
    
    - ChartConfiguration newConfig
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Template Management
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Represents the core domain model for a report template, enforcing consistency and business rules for its configuration.  
**Logic Description:** This class will act as an aggregate root. Its constructor will be private to enforce creation via a static factory method. All mutations to the template's collections (DataSources, etc.) will be handled through methods on this class to ensure validation rules are applied. For example, AddDataSource would check for duplicates or conflicts before adding.  
**Documentation:**
    
    - **Summary:** Defines the structure and behavior of a report template, including its customizable elements like data sources, KPIs, charts, and branding.
    
**Namespace:** Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Reporting/Reporting.Domain/Aggregates/ReportTemplate/ValueObjects/DataSource.cs  
**Description:** A value object representing a single data source for a report, such as a set of OPC tags, a historical data query, or an AI insight.  
**Template:** C# Record  
**Dependency Level:** 0  
**Name:** DataSource  
**Type:** ValueObject  
**Relative Path:** Reporting.Domain/Aggregates/ReportTemplate/ValueObjects  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - ValueObject
    
**Members:**
    
    - **Name:** SourceType  
**Type:** string  
**Attributes:** public  
    - **Name:** Parameters  
**Type:** Dictionary<string, string>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Report Data Source Definition
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** Defines an immutable data source configuration for use within a ReportTemplate.  
**Logic Description:** This will be an immutable record class. The SourceType could be an enum (e.g., Historical, Realtime, AIInsight). The Parameters dictionary will hold the specific query details, like tag names or time ranges.  
**Documentation:**
    
    - **Summary:** Encapsulates the configuration for a source of data to be included in a report.
    
**Namespace:** Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate.ValueObjects  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Reporting/Reporting.Domain/Aggregates/GeneratedReport/GeneratedReport.cs  
**Description:** The root entity for the GeneratedReport aggregate. Represents a specific instance of a report that was generated at a point in time.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** GeneratedReport  
**Type:** AggregateRoot  
**Relative Path:** Reporting.Domain/Aggregates/GeneratedReport  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Aggregate
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** ReportTemplateId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** GenerationTimestamp  
**Type:** DateTimeOffset  
**Attributes:** public  
    - **Name:** Status  
**Type:** ReportStatus  
**Attributes:** public  
    - **Name:** StoragePath  
**Type:** string  
**Attributes:** public  
    - **Name:** Version  
**Type:** int  
**Attributes:** public  
    - **Name:** SignOffStatus  
**Type:** SignOffStatus  
**Attributes:** public  
    - **Name:** SignOffBy  
**Type:** string  
**Attributes:** public  
    - **Name:** SignOffTimestamp  
**Type:** DateTimeOffset?  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** MarkAsCompleted  
**Parameters:**
    
    - string storagePath
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** MarkAsFailed  
**Parameters:**
    
    - string reason
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** ApproveSignOff  
**Parameters:**
    
    - string userId
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Instance Tracking
    - Report Versioning
    - Report Sign-off Workflow
    
**Requirement Ids:**
    
    - REQ-7-020
    - REQ-7-022
    
**Purpose:** Models a generated report, tracking its lifecycle, storage location, and approval status.  
**Logic Description:** This class will manage the state transitions of a report, such as moving from 'Generating' to 'Completed' or 'Failed'. The ApproveSignOff method will contain logic to check if the user has the correct permissions before changing the status.  
**Documentation:**
    
    - **Summary:** Represents an instance of a generated report, containing metadata about its creation, status, version, and sign-off state.
    
**Namespace:** Opc.System.Services.Reporting.Domain.Aggregates.GeneratedReport  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Reporting/Reporting.Domain/Entities/ReportSchedule.cs  
**Description:** Represents a scheduled task for generating a report from a specific template.  
**Template:** C# Class  
**Dependency Level:** 1  
**Name:** ReportSchedule  
**Type:** Entity  
**Relative Path:** Reporting.Domain/Entities  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** ReportTemplateId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** CronExpression  
**Type:** string  
**Attributes:** public  
    - **Name:** IsEnabled  
**Type:** bool  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** Disable  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Enable  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Scheduling
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** Models the configuration for a recurring report generation task.  
**Logic Description:** This entity holds the CRON expression that defines the schedule and links to the template that should be used. The business logic here is simple, primarily enabling or disabling the schedule.  
**Documentation:**
    
    - **Summary:** Defines a schedule, based on a CRON expression, for the automated generation of a specific report template.
    
**Namespace:** Opc.System.Services.Reporting.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Reporting/Reporting.Domain/Abstractions/IReportTemplateRepository.cs  
**Description:** Interface defining the contract for data persistence operations for the ReportTemplate aggregate.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IReportTemplateRepository  
**Type:** RepositoryInterface  
**Relative Path:** Reporting.Domain/Abstractions  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Repository
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    - CancellationToken cancellationToken
    
**Return Type:** Task<ReportTemplate?>  
**Attributes:**   
    - **Name:** AddAsync  
**Parameters:**
    
    - ReportTemplate template
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    - **Name:** UpdateAsync  
**Parameters:**
    
    - ReportTemplate template
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** To decouple the domain model from the specific data storage technology used for report templates.  
**Logic Description:** This interface provides the essential CRUD-like operations needed by the application layer to manage ReportTemplate aggregates. It follows the Repository pattern, ensuring persistence ignorance for the domain.  
**Documentation:**
    
    - **Summary:** Contract for a repository that manages the persistence of ReportTemplate aggregates.
    
**Namespace:** Opc.System.Services.Reporting.Domain.Abstractions  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services/Reporting/Reporting.Application/Abstractions/IDataServiceClient.cs  
**Description:** Interface defining the contract for communicating with the external Data Service to fetch historical or real-time data for reports.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IDataServiceClient  
**Type:** ServiceInterface  
**Relative Path:** Reporting.Application/Abstractions  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Anti-Corruption Layer
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetHistoricalDataAsync  
**Parameters:**
    
    - HistoricalDataQuery query
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<DataPoint>>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-019
    
**Purpose:** To abstract the communication with the Data Service, decoupling the application logic from the specific gRPC/REST client implementation.  
**Logic Description:** This interface provides a clean, domain-aligned method for the reporting service to request data. The implementation in the Infrastructure layer will handle the actual gRPC or HTTP call and data transformation.  
**Documentation:**
    
    - **Summary:** Defines the contract for a client that fetches operational data from the central Data Service.
    
**Namespace:** Opc.System.Services.Reporting.Application.Abstractions  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.Application/Abstractions/IAiServiceClient.cs  
**Description:** Interface defining the contract for communicating with the AI Service to fetch insights, trends, and anomaly data for reports.  
**Template:** C# Interface  
**Dependency Level:** 2  
**Name:** IAiServiceClient  
**Type:** ServiceInterface  
**Relative Path:** Reporting.Application/Abstractions  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Anti-Corruption Layer
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetAnomaliesAsync  
**Parameters:**
    
    - TimeRange timeRange
    - CancellationToken cancellationToken
    
**Return Type:** Task<IEnumerable<AnomalyInsight>>  
**Attributes:**   
    - **Name:** GetTrendInsightsAsync  
**Parameters:**
    
    - TimeRange timeRange
    - CancellationToken cancellationToken
    
**Return Type:** Task<TrendSummary>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-7-018
    
**Purpose:** To abstract the communication with the AI Service, enabling the reporting application logic to request AI-driven insights without direct knowledge of the transport mechanism.  
**Logic Description:** This interface defines methods for fetching pre-computed insights from the AI service. The implementation in the Infrastructure layer will handle the actual client call and map the response to application-level DTOs.  
**Documentation:**
    
    - **Summary:** Defines the contract for a client that fetches analytical insights from the AI Service.
    
**Namespace:** Opc.System.Services.Reporting.Application.Abstractions  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.Application/Features/Reports/Commands/GenerateReport/GenerateReportCommandHandler.cs  
**Description:** Handles the GenerateReportCommand. Orchestrates the process of fetching data, calling the generation engine, and saving the resulting report.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** GenerateReportCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Reporting.Application/Features/Reports/Commands/GenerateReport  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** _reportTemplateRepository  
**Type:** IReportTemplateRepository  
**Attributes:** private|readonly  
    - **Name:** _dataServiceClient  
**Type:** IDataServiceClient  
**Attributes:** private|readonly  
    - **Name:** _aiServiceClient  
**Type:** IAiServiceClient  
**Attributes:** private|readonly  
    - **Name:** _reportGenerationEngine  
**Type:** IReportGenerationEngine  
**Attributes:** private|readonly  
    - **Name:** _generatedReportRepository  
**Type:** IGeneratedReportRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - GenerateReportCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<Guid>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Generation Orchestration
    
**Requirement Ids:**
    
    - REQ-7-018
    - REQ-7-019
    - REQ-7-020
    
**Purpose:** Contains the core application logic for orchestrating the generation of a single report.  
**Logic Description:** The handler will first fetch the ReportTemplate. Then, for each data source in the template, it will call the appropriate service client (IDataServiceClient, IAiServiceClient) to get the data. It will then aggregate all the data and pass it to the IReportGenerationEngine. Finally, it will save the metadata of the generated report using the IGeneratedReportRepository and return the ID of the new report instance.  
**Documentation:**
    
    - **Summary:** Orchestrates the report generation use case by fetching template, data, and insights, then invoking the generation engine and persisting the result.
    
**Namespace:** Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.Application/Features/Schedules/ReportSchedulingService.cs  
**Description:** A background service responsible for managing and triggering scheduled report generation jobs.  
**Template:** C# HostedService  
**Dependency Level:** 3  
**Name:** ReportSchedulingService  
**Type:** Service  
**Relative Path:** Reporting.Application/Features/Schedules  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - BackgroundService
    
**Members:**
    
    - **Name:** _schedulerFactory  
**Type:** ISchedulerFactory  
**Attributes:** private|readonly  
    - **Name:** _reportScheduleRepository  
**Type:** IReportScheduleRepository  
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
    
    - Automated Report Scheduling
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** To load all active report schedules from the database and configure them with a scheduling library like Quartz.NET.  
**Logic Description:** On application startup (StartAsync), this service will connect to the scheduling engine (e.g., Quartz.NET). It will fetch all active schedules from the IReportScheduleRepository and register a job (e.g., ReportGenerationJob) for each one with the corresponding CRON expression. This ensures that schedules are active and will trigger automatically.  
**Documentation:**
    
    - **Summary:** A long-running background service that initializes and manages the job scheduler, ensuring that scheduled reports are triggered at their configured times.
    
**Namespace:** Opc.System.Services.Reporting.Application.Features.Schedules  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.Application/Features/Distribution/ReportDistributionService.cs  
**Description:** Service responsible for orchestrating the distribution of a generated report to its configured targets.  
**Template:** C# Service  
**Dependency Level:** 3  
**Name:** ReportDistributionService  
**Type:** Service  
**Relative Path:** Reporting.Application/Features/Distribution  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _emailService  
**Type:** IEmailService  
**Attributes:** private|readonly  
    - **Name:** _fileStorageService  
**Type:** IFileStorageService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** DistributeAsync  
**Parameters:**
    
    - Guid generatedReportId
    - IEnumerable<DistributionTarget> targets
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Report Distribution Logic
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** To manage the multi-channel distribution of a completed report.  
**Logic Description:** This service will be called after a report is successfully generated. It will iterate through the list of distribution targets. Based on the target type (e.g., 'Email', 'NetworkShare'), it will invoke the corresponding infrastructure service (IEmailService, IFileStorageService) to perform the actual delivery.  
**Documentation:**
    
    - **Summary:** Handles the business logic for distributing a generated report to various channels like email or file shares based on configured targets.
    
**Namespace:** Opc.System.Services.Reporting.Application.Features.Distribution  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/ReportGeneration/ReportGenerationEngine.cs  
**Description:** Core engine that takes prepared report data and a format, and invokes the correct generator to produce the final report file.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** ReportGenerationEngine  
**Type:** Engine  
**Relative Path:** Reporting.Infrastructure/ReportGeneration  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - Strategy
    
**Members:**
    
    - **Name:** _serviceProvider  
**Type:** IServiceProvider  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateAsync  
**Parameters:**
    
    - ReportDataModel data
    - ReportFormat format
    - CancellationToken cancellationToken
    
**Return Type:** Task<Stream>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Multi-format Report Generation
    
**Requirement Ids:**
    
    - REQ-7-021
    
**Purpose:** To act as a factory or strategy selector for the different concrete report format generators (PDF, Excel, HTML).  
**Logic Description:** This class implements the IReportGenerationEngine interface. The GenerateAsync method will use a switch statement or a dictionary lookup based on the 'format' parameter to resolve the correct IReportFormatGenerator (e.g., PdfGenerator) from the dependency injection container. It then calls the 'Generate' method on the selected generator and returns the resulting file stream.  
**Documentation:**
    
    - **Summary:** Central component for report generation. It selects the appropriate format-specific generator (PDF, Excel, etc.) and invokes it to create the report file stream.
    
**Namespace:** Opc.System.Services.Reporting.Infrastructure.ReportGeneration  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/ReportGeneration/PdfGenerator.cs  
**Description:** Implements IReportFormatGenerator for creating PDF reports using the QuestPDF library.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** PdfGenerator  
**Type:** Service  
**Relative Path:** Reporting.Infrastructure/ReportGeneration  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Generate  
**Parameters:**
    
    - ReportDataModel data
    
**Return Type:** Stream  
**Attributes:** public  
    
**Implemented Features:**
    
    - PDF Report Generation
    
**Requirement Ids:**
    
    - REQ-7-020
    - REQ-7-021
    
**Purpose:** To handle the specific logic and layout for converting structured report data into a PDF document.  
**Logic Description:** This class will use the QuestPDF fluent API to define the document structure. It will iterate through the provided ReportDataModel, creating pages, headers, footers, tables, and charts as defined by the data. It will handle styling, branding (logos), and layout for the PDF format.  
**Documentation:**
    
    - **Summary:** A concrete generator that uses the QuestPDF library to create a report in PDF format from a given data model.
    
**Namespace:** Opc.System.Services.Reporting.Infrastructure.ReportGeneration  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/ReportGeneration/ExcelGenerator.cs  
**Description:** Implements IReportFormatGenerator for creating Excel reports using the ClosedXML library.  
**Template:** C# Class  
**Dependency Level:** 3  
**Name:** ExcelGenerator  
**Type:** Service  
**Relative Path:** Reporting.Infrastructure/ReportGeneration  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Generate  
**Parameters:**
    
    - ReportDataModel data
    
**Return Type:** Stream  
**Attributes:** public  
    
**Implemented Features:**
    
    - Excel Report Generation
    
**Requirement Ids:**
    
    - REQ-7-020
    - REQ-7-021
    
**Purpose:** To handle the specific logic for converting structured report data into an Excel (.xlsx) file.  
**Logic Description:** This class will use the ClosedXML library to create a new Excel workbook. It will create worksheets for different sections of the report. It will populate cells with data from the ReportDataModel, apply formatting, create tables, and potentially embed charts. The final workbook is saved to a memory stream.  
**Documentation:**
    
    - **Summary:** A concrete generator that uses the ClosedXML library to create a report in Excel format from a given data model.
    
**Namespace:** Opc.System.Services.Reporting.Infrastructure.ReportGeneration  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.Infrastructure/Scheduling/ReportGenerationJob.cs  
**Description:** A Quartz.NET job that is triggered by the scheduler. Its purpose is to initiate the report generation process.  
**Template:** C# Class  
**Dependency Level:** 4  
**Name:** ReportGenerationJob  
**Type:** Job  
**Relative Path:** Reporting.Infrastructure/Scheduling  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _mediator  
**Type:** IMediator  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Execute  
**Parameters:**
    
    - IJobExecutionContext context
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - Scheduled Job Execution
    
**Requirement Ids:**
    
    - REQ-7-020
    
**Purpose:** To decouple the scheduling mechanism from the application logic. This job acts as the entry point for a scheduled task.  
**Logic Description:** When triggered by Quartz.NET, the Execute method will be invoked. It will extract the ReportTemplateId from the job's data map. It will then create and send a `GenerateReportCommand` using MediatR. This command will be picked up by the handler in the Application layer, starting the report generation workflow.  
**Documentation:**
    
    - **Summary:** Implements a schedulable job (e.g., for Quartz.NET) that, when executed, sends a command to the application layer to start generating a specific report.
    
**Namespace:** Opc.System.Services.Reporting.Infrastructure.Scheduling  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services/Reporting/Reporting.API/Controllers/ReportsController.cs  
**Description:** REST API controller for managing and retrieving generated reports.  
**Template:** C# Controller  
**Dependency Level:** 5  
**Name:** ReportsController  
**Type:** Controller  
**Relative Path:** Reporting.API/Controllers  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    - REST
    
**Members:**
    
    - **Name:** _mediator  
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
    - **Name:** ApproveReport  
**Parameters:**
    
    - Guid reportId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - On-demand Report Generation API
    - Report Download API
    - Report Status API
    - Report Sign-off API
    
**Requirement Ids:**
    
    - REQ-7-020
    - REQ-7-022
    
**Purpose:** To provide external clients and the UI with HTTP endpoints for interacting with reports.  
**Logic Description:** Each action method will create a corresponding command or query object and send it via MediatR. For example, GenerateReportOnDemand will send a `GenerateReportCommand`. DownloadReport will send a query to get the report's storage path and then return a FileStreamResult. Authorization attributes will be used to enforce RBAC for sensitive operations like downloading or approving reports.  
**Documentation:**
    
    - **Summary:** Exposes HTTP endpoints for on-demand report generation, status checking, downloading completed reports, and managing sign-off workflows.
    
**Namespace:** Opc.System.Services.Reporting.API.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Reporting/Reporting.API/Program.cs  
**Description:** The main entry point for the Reporting microservice application. Configures and launches the ASP.NET Core host.  
**Template:** C# Program  
**Dependency Level:** 6  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:** Reporting.API  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Service Bootstrap
    - Dependency Injection Configuration
    - Middleware Configuration
    
**Requirement Ids:**
    
    - REQ-7-021
    
**Purpose:** To bootstrap the entire microservice, setting up dependency injection, configuring middleware, and starting the web server.  
**Logic Description:** This file will contain the application's composition root. It will set up the web application builder, register services from all layers (Application, Infrastructure) into the DI container, configure logging, authentication, authorization, and the ASP.NET Core request pipeline (e.g., UseRouting, UseAuthentication, UseAuthorization, MapControllers). It will also register and start hosted services like the ReportSchedulingService.  
**Documentation:**
    
    - **Summary:** The main application entry point that configures and runs the Reporting microservice.
    
**Namespace:** Opc.System.Services.Reporting.API  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services/Reporting/Reporting.API/appsettings.json  
**Description:** Configuration file for the Reporting microservice, containing settings for database connections, external service URLs, and feature toggles.  
**Template:** JSON  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** Reporting.API  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To provide a centralized location for environment-specific application settings.  
**Logic Description:** This JSON file will be structured with sections for ConnectionStrings, URLs for the Data and AI services, SMTP settings for email distribution, file storage connection details, and logging configurations. It will be mapped to strongly-typed options classes at startup.  
**Documentation:**
    
    - **Summary:** Contains configuration settings for the Reporting service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services/Reporting/Reporting.csproj  
**Description:** The main project file for the Reporting.API project, defining its framework, dependencies, and build properties.  
**Template:** XML  
**Dependency Level:** 7  
**Name:** Reporting  
**Type:** ProjectFile  
**Relative Path:** Reporting.API  
**Repository Id:** REPO-SAP-006  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To manage the build process and package dependencies for the main executable project of the microservice.  
**Logic Description:** This XML file will specify the TargetFramework (e.g., net8.0). It will include PackageReference items for ASP.NET Core, MediatR, Serilog, QuestPDF, ClosedXML, Quartz.NET, and other necessary NuGet packages. It will also have ProjectReference items pointing to the Application, Domain, and Infrastructure projects.  
**Documentation:**
    
    - **Summary:** The MSBuild project file for the Reporting.API project.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableReportSignOffWorkflow
  - EnableHtmlReportFormat
  - UseCloudFileStorage
  
- **Database Configs:**
  
  - ConnectionStrings:ReportingDb
  - ServiceEndpoints:DataServiceUrl
  - ServiceEndpoints:AiServiceUrl
  - SmtpSettings:Host
  - SmtpSettings:Port
  - FileStorage:ConnectionString
  - FileStorage:ContainerName
  


---

