# Software Design Specification (SDS): Reporting Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Reporting Service** (`Opc.System.Services.Reporting`). This microservice is responsible for the generation, management, scheduling, and distribution of reports based on operational and analytical data from the wider system. It serves as a backend for user-facing reporting features, supporting template customization, multiple output formats, and integration with other system services like the Data Service and AI Service.

### 1.2. Scope
The scope of this document is limited to the design and implementation of the `Services.Reporting` microservice. This includes:
-   Domain models for report templates, schedules, and generated reports.
-   Application logic for orchestrating report generation and distribution.
-   Infrastructure for persistence, scheduling, and communication with external services.
-   RESTful API for external interaction.

### 1.3. Architecture Overview
The service will be built using **.NET 8** and follow a **Clean (Onion) Architecture**, promoting separation of concerns, testability, and maintainability. The layers are:
-   **Domain:** Contains core business logic, entities, and aggregates.
-   **Application:** Orchestrates domain logic, handles commands/queries (CQRS with MediatR), and defines abstractions for external dependencies.
-   **Infrastructure:** Implements external-facing concerns like database access, file I/O, and clients for other services.
-   **API (Presentation):** Exposes the service's functionality via a secure ASP.NET Core RESTful API.

## 2. System Design

### 2.1. Domain Layer (`Reporting.Domain`)

This layer contains the core business models and rules, free from any infrastructure or application-specific concerns.

#### 2.1.1. Aggregates and Entities

**`ReportTemplate` (Aggregate Root)**
-   **Description:** Represents a customizable report definition. It is the consistency boundary for all template-related configurations.
-   **Properties:**
    -   `Id (Guid)`: Primary key.
    -   `Name (string)`: The user-defined name of the template.
    -   `Branding (Branding)`: A value object for logos and color schemes.
    -   `_dataSources (private List<DataSource>)`: A collection of data sources for the report.
    -   `_kpiDefinitions (private List<KpiDefinition>)`: A collection of KPI calculation rules.
    -   `_chartConfigurations (private List<ChartConfiguration>)`: A collection of chart definitions.
    -   `_distributionTargets (private List<DistributionTarget>)`: Default recipients of the report.
-   **Methods:**
    -   `Create(...)`: Static factory method to ensure valid initial state.
    -   `UpdateDetails(string newName, Branding newBranding)`: Updates basic properties.
    -   `AddDataSource(DataSource source)`: Adds a data source, performing validation (e.g., preventing duplicates).
    -   `RemoveDataSource(Guid sourceId)`: Removes a data source.
    -   `UpdateChartConfiguration(...)`: Modifies a chart configuration.
    -   `AddDistributionTarget(...)`: Adds a default distribution target.

**`GeneratedReport` (Aggregate Root)**
-   **Description:** Represents a specific instance of a report generated from a template at a point in time.
-   **Properties:**
    -   `Id (Guid)`: Primary key.
    -   `ReportTemplateId (Guid)`: FK to the `ReportTemplate`.
    -   `GenerationTimestamp (DateTimeOffset)`: When the report generation was initiated.
    -   `Status (ReportStatus)`: Enum (`Pending`, `Generating`, `Completed`, `Failed`).
    -   `StoragePath (string)`: The path to the generated file (e.g., blob storage URI or file path).
    -   `Version (int)`: Version number for reports based on the same template.
    -   `SignOffStatus (SignOffStatus)`: Enum (`NotRequired`, `Pending`, `Approved`, `Rejected`).
    -   `SignOffBy (string?)`: User ID of the approver.
    -   `SignOffTimestamp (DateTimeOffset?)`: Timestamp of the sign-off action.
-   **Methods:**
    -   `Create(...)`: Static factory to initialize a report instance in the `Pending` state.
    -   `StartGeneration()`: Moves status to `Generating`.
    -   `MarkAsCompleted(string storagePath)`: Sets status to `Completed` and records the storage path.
    -   `MarkAsFailed(string reason)`: Sets status to `Failed`.
    -   `RequestSignOff()`: Sets sign-off status to `Pending`.
    -   `ApproveSignOff(string userId)`: Sets sign-off status to `Approved`.
    -   `RejectSignOff(string userId)`: Sets sign-off status to `Rejected`.

**`ReportSchedule` (Entity)**
-   **Description:** Defines a recurring schedule for a report template.
-   **Properties:**
    -   `Id (Guid)`: Primary key.
    -   `ReportTemplateId (Guid)`: FK to the `ReportTemplate`.
    -   `CronExpression (string)`: The schedule in CRON format.
    -   `IsEnabled (bool)`: Flag to activate or deactivate the schedule.
-   **Methods:**
    -   `Enable()` / `Disable()`: Toggles the `IsEnabled` flag.

#### 2.1.2. Value Objects
-   **`DataSource`**: `(string SourceType, Dictionary<string, string> Parameters)` - Defines a data source (e.g., `SourceType="Historical"`, `Parameters={"tags": "tag1,tag2", "range": "24h"}`).
-   **`KpiDefinition`**: `(string Name, string CalculationLogic, Guid DataSourceId)` - Defines a Key Performance Indicator.
-   **`ChartConfiguration`**: `(string Title, ChartType Type, List<Guid> DataSourceIds)` - Defines a chart.
-   **`Branding`**: `(string LogoUrl, string PrimaryColor)` - Defines branding elements.
-   **`DistributionTarget`**: `(DistributionChannel Channel, string Address)` - Defines a recipient (e.g., `Channel="Email"`, `Address="team@example.com"`).

### 2.2. Application Layer (`Reporting.Application`)

This layer orchestrates the domain logic by handling application use cases.

#### 2.2.1. CQRS with MediatR
-   **Commands:** Represent actions that change the system state.
    -   `CreateReportTemplateCommand`: Creates a new `ReportTemplate`.
    -   `GenerateReportCommand`: Triggers the generation of a report from a template.
    -   `ScheduleReportCommand`: Creates or updates a `ReportSchedule`.
    -   `DistributeReportCommand`: Initiates the distribution of a completed report.
    -   `ApproveReportSignOffCommand`: Handles the sign-off workflow.
-   **Queries:** Represent requests for data that do not change state.
    -   `GetReportTemplateByIdQuery`: Fetches a report template.
    -   `GetGeneratedReportStatusQuery`: Checks the status of a generation job.
    -   `GetGeneratedReportFileQuery`: Retrieves the file stream of a completed report.

#### 2.2.2. Key Command Handlers
-   **`GenerateReportCommandHandler`**:
    1.  Fetches the `ReportTemplate` from the repository.
    2.  Creates a new `GeneratedReport` entity and saves it with `Pending` status.
    3.  For each `DataSource` in the template:
        -   If `SourceType` is "Historical", call `IDataServiceClient.GetHistoricalDataAsync()`.
        -   If `SourceType` is "AIInsight", call `IAiServiceClient.GetAnomaliesAsync()`.
    4.  Aggregates the fetched data into a `ReportDataModel` DTO.
    5.  Invokes `IReportGenerationEngine.GenerateAsync(data, format)`.
    6.  Saves the returned file stream using `IFileStorageService`.
    7.  Updates the `GeneratedReport` entity with the storage path and `Completed` status.
    8.  Dispatches a `DistributeReportCommand`.
    9.  Handles exceptions by updating the `GeneratedReport` status to `Failed`.

#### 2.2.3. Abstractions
-   `IDataServiceClient`, `IAiServiceClient`: Decouple the application from the gRPC/REST clients in the Infrastructure layer.
-   `IReportGenerationEngine`: Decouples the application from the specific report format generation logic (PDF, Excel).
-   `IEmailService`, `IFileStorageService`: Abstract infrastructure concerns for distribution and storage.
-   `IReportTemplateRepository`, `IGeneratedReportRepository`, `IReportScheduleRepository`: Repository pattern interfaces.

#### 2.2.4. Background Services
-   **`ReportSchedulingService` (`IHostedService`):**
    -   Uses `Quartz.NET`.
    -   On startup, loads all enabled `ReportSchedule` entities.
    -   For each schedule, it registers a `ReportGenerationJob` with the scheduler, passing the `ReportTemplateId` in the job data.
-   **`ReportRetentionService` (`IHostedService`):**
    -   Runs on a periodic timer (e.g., daily).
    -   Queries for `GeneratedReport` entities that have exceeded their retention policy.
    -   For each expired report, it calls `IFileStorageService` to archive or delete the file and updates the database record accordingly.

### 2.3. Infrastructure Layer (`Reporting.Infrastructure`)

This layer contains concrete implementations of the abstractions defined in the layers above.

#### 2.3.1. Persistence
-   **`ReportingDbContext`**: An `EF Core DbContext` with `DbSet`s for `ReportTemplate`, `GeneratedReport`, and `ReportSchedule`.
-   **Repository Implementations**: E.g., `ReportTemplateRepository.cs` will use the `ReportingDbContext` to perform CRUD operations.

#### 2.3.2. Report Generation (`IReportGenerationEngine`)
-   **`ReportGenerationEngine`**: Implements the strategy pattern. It uses dependency injection to get an `IEnumerable<IReportFormatGenerator>` and selects the correct one based on the requested format.
-   **`PdfGenerator`**: Implements `IReportFormatGenerator`. Uses the **QuestPDF** library to construct the PDF document from the `ReportDataModel`.
-   **`ExcelGenerator`**: Implements `IReportFormatGenerator`. Uses the **ClosedXML** library to create an `.xlsx` workbook.
-   **`HtmlGenerator`**: Implements `IReportFormatGenerator`. Uses a simple string builder or a library like RazorEngine.Core to render an HTML document.

#### 2.3.3. Scheduling (`Quartz.NET`)
-   **`ReportGenerationJob`**: Implements `IJob`. In its `Execute` method, it retrieves the `ReportTemplateId` from the job context and sends a `GenerateReportCommand` via MediatR.

#### 2.3.4. Distribution and Storage
-   **`EmailService`**: Implements `IEmailService`. Uses a library like **MailKit** to send emails with the report as an attachment.
-   **`LocalFileStorageService`**: Implements `IFileStorageService`. Saves files to a configured local or network path.
-   **`AzureBlobStorageService` (Optional)**: Alternative implementation of `IFileStorageService` for cloud deployments.

#### 2.3.5. External Service Clients
-   **`DataServiceClient`**: Implements `IDataServiceClient`. Uses `HttpClientFactory` or a generated gRPC client to call the Data Service.
-   **`AiServiceClient`**: Implements `IAiServiceClient`. Uses `HttpClientFactory` or a generated gRPC client to call the AI Service.

### 2.4. API Layer (`Reporting.API`)

This is the entry point to the microservice.

#### 2.4.1. Controllers
-   **`ReportTemplatesController`**:
    -   `POST /api/templates`: Create a new report template.
    -   `GET /api/templates/{id}`: Get a template by ID.
    -   `PUT /api/templates/{id}`: Update a template.
-   **`ReportsController`**:
    -   `POST /api/reports`: On-demand generation from a template.
    -   `GET /api/reports/{id}/status`: Get status of a generated report.
    -   `GET /api/reports/{id}/download`: Download a completed report file.
    -   `POST /api/reports/{id}/approve`: Handle the sign-off workflow.
-   **`SchedulesController`**:
    -   `POST /api/schedules`: Create or update a schedule.

#### 2.4.2. Startup and Configuration (`Program.cs`)
-   **Dependency Injection:** All services, repositories, and handlers will be registered. E.g., `services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>()`.
-   **Authentication:** Configure JWT Bearer authentication.
-   **Authorization:** Configure policy-based or role-based authorization.
-   **Middleware:** Configure exception handling, routing, authentication, and authorization.
-   **Hosted Services:** Register `ReportSchedulingService` and `ReportRetentionService`.
-   **Configuration:** Bind `appsettings.json` sections to strongly-typed options classes.

### 2.5. Cross-Cutting Concerns

-   **Logging:** **Serilog** will be used for structured logging. A middleware will log all incoming requests and their outcomes.
-   **Validation:** **FluentValidation** will be used to validate all incoming DTOs and command objects. A validation pipeline behavior will be added to MediatR.
-   **Error Handling:** A custom exception handling middleware will catch unhandled exceptions, log them, and return a standardized JSON error response (e.g., `ProblemDetails`).

## 3. Configuration (`appsettings.json`)

json
{
  "ConnectionStrings": {
    "ReportingDb": "Server=...;Database=Reporting;User Id=...;Password=...;"
  },
  "ServiceEndpoints": {
    "DataServiceUrl": "http://localhost:5001",
    "AiServiceUrl": "http://localhost:5002"
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "user",
    "Password": "password",
    "FromAddress": "noreply@system.com"
  },
  "FileStorage": {
    "Provider": "Local", // or "AzureBlob"
    "LocalPath": "C:/Reports/Generated",
    "AzureConnectionString": "DefaultEndpointsProtocol=...",
    "AzureContainerName": "generated-reports"
  },
  "Quartz": {
    "quartz.scheduler.instanceName": "ReportScheduler"
  },
  "FeatureToggles": {
    "EnableReportSignOffWorkflow": true
  }
}
