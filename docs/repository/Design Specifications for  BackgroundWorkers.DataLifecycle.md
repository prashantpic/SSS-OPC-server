# Software Design Specification: BackgroundWorkers.DataLifecycle

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the `BackgroundWorkers.DataLifecycle` service. This service is a .NET 8 background worker responsible for enforcing data lifecycle policies across the system. It periodically archives and purges data from primary datastores to manage storage costs, ensure compliance, and maintain system performance. All significant actions performed by this service are audited.

### 1.2. Scope
The scope of this document is limited to the design of the `DataLifecycle` worker. This includes its internal architecture, job scheduling, interaction with data stores for policy retrieval, data archival and purging mechanisms, and audibility. It relies on external repositories for data access implementations (`DataService.Core`) and common utilities (`Common.Shared`).

## 2. System Overview

### 2.1. Architecture
The `DataLifecycle` service is a standalone .NET 8 application built on the Generic Host model. It operates as a continuously running background service. Its core functionality is driven by the **Quartz.NET** library, which schedules and executes a master job based on a configurable CRON expression.

The internal design heavily utilizes the **Strategy Pattern** to handle different types of data (e.g., Historical, Alarm, Audit). A factory is used to resolve the correct strategy at runtime based on the policy being processed. This approach ensures that the service is modular and easily extensible to support new data types in the future.

### 2.2. Core Responsibilities
- **Policy Enforcement:** Periodically fetches active data retention policies from a central database.
- **Data Archival:** Moves data older than its retention period from hot, primary databases (e.g., Time-Series DB) to cold, cost-effective long-term storage (e.g., AWS S3, Azure Blob Storage), as required by the policy.
- **Data Purging:** Securely deletes data from primary databases that has exceeded its retention period.
- **Auditing:** Logs every significant action (e.g., job start/end, policy execution, records archived, records purged, errors) to a central audit system for traceability and compliance.

## 3. Configuration (`appsettings.json`)
The service's behavior is controlled by the `appsettings.json` file. This allows for environment-specific configuration without code changes.

json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" }
    ]
  },
  "ConnectionStrings": {
    "ApplicationDb": "Server=...;Database=...;User Id=...;Password=...;"
  },
  "ArchiveStorage": {
    "Type": "AzureBlob", // or "S3"
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "ContainerName": "data-archive" // or "BucketName" for S3
  },
  "Scheduler": {
    "DataLifecycleJobCronExpression": "0 0 2 * * ?" // Default: Run at 2:00 AM every day
  }
}

- **ConnectionStrings:ApplicationDb**: Connection string to the primary database where `DataRetentionPolicy` entities are stored.
- **ArchiveStorage**: Configuration for the long-term archival destination.
    - `Type`: Specifies the cloud provider (`AzureBlob` or `S3`).
    - `ConnectionString`: The connection string or necessary credentials for the storage account.
    - `ContainerName` / `BucketName`: The target container or bucket for archives.
- **Scheduler:DataLifecycleJobCronExpression**: The CRON expression that defines when the main `DataLifecycleJob` will be executed.

## 4. Core Components and Logic

### 4.1. Host and Scheduler Setup (`Program.cs`, `Scheduling/QuartzStartup.cs`)
- **`Program.cs`**:
    - The entry point of the service.
    - Configures and builds an `IHost` using `Host.CreateDefaultBuilder()`.
    - Configures Serilog for structured logging, reading from `appsettings.json`.
    - Registers all services for Dependency Injection (DI): policies, strategies, factories, repositories, archivers, purgers, and loggers.
    - Configures Quartz.NET using `.AddQuartz()` and `.AddQuartzHostedService()`. The configuration is delegated to `QuartzStartup`.
- **`Scheduling/QuartzStartup.cs`**:
    - Contains a static `Configure` method that centralizes job and trigger definitions.
    - Reads the `Scheduler:DataLifecycleJobCronExpression` from `IConfiguration`.
    - Defines a single durable job of type `DataLifecycleJob`.
    - Creates a trigger for this job using the CRON expression, ensuring it runs on the configured schedule.

### 4.2. Job Orchestration (`Jobs/DataLifecycleJob.cs`)
This class is the heart of the service's workflow.

- **`DataLifecycleJob : IJob`**: Implements the Quartz.NET `IJob` interface.
- **Dependencies (injected via constructor)**:
    - `IPolicyProvider _policyProvider`: To get the policies.
    - `IDataLifecycleStrategyFactory _strategyFactory`: To get the right execution logic.
    - `ILogger<DataLifecycleJob> _logger`: For operational logging.
- **`Execute(IJobExecutionContext context)` Logic**:
    1. Log the start of the job run.
    2. In a `try...catch` block:
        a. Retrieve all active policies: `var policies = await _policyProvider.GetActivePoliciesAsync();`
        b. If no policies are found, log a message and exit gracefully.
        c. For each `policy` in `policies`:
            i. Log the execution of the specific policy.
            ii. Get the appropriate strategy: `var strategy = _strategyFactory.GetStrategy(policy.DataType);`
            iii. If a strategy is found, execute it: `await strategy.ExecuteAsync(policy, context.CancellationToken);`
            iv. If no strategy is found, log a warning.
    3. `catch (Exception ex)`: Log any unhandled exception that occurs during the orchestration.
    4. `finally`: Log the completion of the job run.

### 4.3. Lifecycle Strategies (Strategy Pattern)

#### 4.3.1. Factory (`Application/Factories/DataLifecycleStrategyFactory.cs`)
- **`IDataLifecycleStrategyFactory`**: Interface with a single method: `IDataLifecycleStrategy GetStrategy(DataType dataType);`
- **`DataLifecycleStrategyFactory`**:
    - **Dependency**: Injected with `IEnumerable<IDataLifecycleStrategy>`.
    - **Constructor**: Populates a `IReadOnlyDictionary<DataType, IDataLifecycleStrategy>` using the injected strategies, mapping each strategy's `DataType` property to the instance.
    - **`GetStrategy` method**: Performs a lookup in the dictionary. Throws an exception if no strategy is registered for the given `DataType`.

#### 4.3.2. Concrete Strategies (`Application/Strategies/`)
Each strategy implements `IDataLifecycleStrategy` and encapsulates the logic for a specific data type.

- **`HistoricalDataLifecycleStrategy`**:
    - **`DataType` Property**: Returns `DataType.Historical`.
    - **Dependencies**: `IArchiver`, `IPurger`, `IAuditLogger`, `ISourceDataRepository<HistoricalData>`.
    - **`ExecuteAsync` Logic**:
        1. Calculate the `retentionThreshold` date from `policy.RetentionPeriodDays`.
        2. **Archival**:
            - If `policy.ArchiveLocation` is not null/empty:
                - Log the start of the archival action via `IAuditLogger`.
                - Call `_archiver.ArchiveAsync(...)` for `DataType.Historical`.
                - Log the outcome (success/fail, records moved) via `IAuditLogger`.
        3. **Purging**:
            - Log the start of the purge action via `IAuditLogger`.
            - Call `_purger.PurgeAsync(...)` for `DataType.Historical`. This should delete data that has been successfully archived or data that is past its retention without an archive policy.
            - Log the outcome (success/fail, records deleted) via `IAuditLogger`.
        4. All steps should be wrapped in `try...catch` blocks to ensure failures are audited.

- **`AlarmDataLifecycleStrategy` / `AuditDataLifecycleStrategy`**: These will follow the exact same logical pattern as `HistoricalDataLifecycleStrategy` but will operate on `DataType.Alarm` and `DataType.Audit` respectively, and use their corresponding data repositories.

### 4.4. Data Operations

#### 4.4.1. Archival (`Infrastructure/Archiving/BlobStorageArchiver.cs`)
- **`IArchiver`**: Interface defining the `ArchiveAsync` method.
- **`BlobStorageArchiver : IArchiver`**:
    - **Dependencies**: Cloud storage client (`IAzureBlobClient` or `IAwsS3Client`), `ISourceDataRepository` for the relevant data type.
    - **`ArchiveAsync` Logic**:
        1. Determine the source repository based on `DataType`.
        2. Query the source repository for records older than the `threshold` in manageable batches (e.g., 10,000 records at a time).
        3. For each batch:
            a. Serialize the batch of records into a structured format (e.g., compressed CSV or Parquet).
            b. Generate a unique file name (e.g., `{dataType}/{year}/{month}/{day}/{guid}.parquet.gz`).
            c. Upload the serialized file to the specified `archiveLocation` (container/bucket).
            d. Keep a running total of archived records.
        4. Return an `ArchiveResult` containing the final count and status.

#### 4.4.2. Purging (`Infrastructure/Purging/DatabasePurger.cs`)
- **`IPurger`**: Interface defining the `PurgeAsync` method.
- **`DatabasePurger : IPurger`**:
    - **Dependencies**: `ISourceDataRepository` for the relevant data type.
    - **`PurgeAsync` Logic**:
        1. Determine the source repository based on `DataType`.
        2. Execute a single, efficient bulk-delete operation on the repository for all records older than the `threshold`.
        3. The repository method should return the number of rows affected.
        4. Return a `PurgeResult` containing the count and status.

### 4.5. Auditing
- **`IAuditLogger`**: Interface defining `LogDataLifecycleEventAsync`.
- **Implementation (`Infrastructure/Logging/AuditLogger.cs`)**:
    - This implementation will not log to a local file. Instead, it will be responsible for sending a structured audit event to the central system.
    - **Logic**: It will likely use a message queue client (e.g., RabbitMQ) or an HTTP client to send a JSON payload to a dedicated audit service endpoint or message topic. The payload will contain all parameters from the method call. This decouples the worker from the audit storage mechanism.

## 5. Data Models and Persistence

### 5.1. Domain Models (`Domain/`)
- **`DataRetentionPolicy`**: A POCO class representing the configuration for a policy.
    csharp
    public class DataRetentionPolicy
    {
        public Guid PolicyId { get; set; }
        public DataType DataType { get; set; }
        public int RetentionPeriodDays { get; set; }
        public string? ArchiveLocation { get; set; }
        public bool IsActive { get; set; }
    }
    
- **`DataType`**: A public enum.
    csharp
    public enum DataType { Historical, Alarm, Audit, AI }
    

### 5.2. Persistence (`Infrastructure/Persistence/`)
- **`IDataRetentionPolicyRepository`**:
    - Defines the contract for data access: `Task<IEnumerable<DataRetentionPolicy>> GetActivePoliciesAsync();`
- The concrete implementation of this repository is expected to be in the `DataService.Core` shared project, using Entity Framework Core to query the `ApplicationDb`. This worker service will consume the interface and register the implementation via DI.

## 6. Error Handling and Resilience
- **Job Level**: The main `Execute` method in `DataLifecycleJob` will have a top-level `try...catch` block. Any failure here will be logged, but it will not stop the Quartz scheduler from attempting the next run.
- **Strategy Level**: Each `IDataLifecycleStrategy` implementation will have its own `try...catch` blocks around its archival and purging operations. A failure in one strategy (e.g., for `Historical` data) should be audited but must not prevent the processing of policies for other data types (e.g., `Alarm` data).
- **Quartz.NET Configuration**: The job will be configured as `Durable` and non-concurrent (`[DisallowConcurrentExecution]`). Retries for failed jobs can be configured within Quartz if necessary, but the primary logic is to succeed or fail the run and wait for the next scheduled execution.

## 7. Dependency Injection Registration Summary
The following services will be registered in `Program.cs`:

csharp
// Hosting & Scheduling
services.AddQuartz(...); // Configured via QuartzStartup
services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Application Services & Factories
services.AddSingleton<IDataLifecycleStrategyFactory, DataLifecycleStrategyFactory>();
services.AddSingleton<IPolicyProvider, PolicyProvider>();
services.AddSingleton<IArchiver, BlobStorageArchiver>();
services.AddSingleton<IPurger, DatabasePurger>();
services.AddSingleton<IAuditLogger, AuditLogger>();

// Strategies (registered as a collection for the factory)
services.AddSingleton<IDataLifecycleStrategy, HistoricalDataLifecycleStrategy>();
services.AddSingleton<IDataLifecycleStrategy, AlarmDataLifecycleStrategy>();
services.AddSingleton<IDataLifecycleStrategy, AuditDataLifecycleStrategy>();
// services.AddSingleton<IDataLifecycleStrategy, AiDataLifecycleStrategy>(); // Future

// Infrastructure / Data Access
// These are assumed to come from a shared DataService project
services.AddScoped<IDataRetentionPolicyRepository, DataRetentionPolicyRepository>();
services.AddScoped<ISourceDataRepository<HistoricalData>, HistoricalDataRepository>();
services.AddScoped<ISourceDataRepository<AlarmEvent>, AlarmEventRepository>();
// ... other repositories

// Cloud clients
// Logic to register IAzureBlobClient or IAwsS3Client based on config
