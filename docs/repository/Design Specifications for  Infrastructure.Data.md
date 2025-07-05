# Software Design Specification (SDS): Infrastructure.Data

## 1. Introduction

### 1.1. Purpose
This document specifies the software design for the `Opc.System.Infrastructure.Data` library. This library serves as a foundational data access layer for the entire server-side application, providing a robust, abstracted, and consistent interface for all persistence operations. It encapsulates interactions with a relational database (PostgreSQL), a time-series database (InfluxDB), and a blob storage provider (Azure Blob Storage).

### 1.2. Scope
The scope of this library includes:
- Defining and implementing repository patterns for all data entities.
- Managing database connections and sessions via Entity Framework Core (`AppDbContext`).
- Providing clients and concrete repository implementations for InfluxDB and Azure Blob Storage.
- Implementing core data lifecycle requirements, including ingestion, querying, validation, and security (encryption at rest).
- Offering services for cross-cutting data concerns like data masking.
- Providing a centralized dependency injection setup for easy integration into application services.

## 2. Overall Design

### 2.1. Architectural Style
The `Infrastructure.Data` library is designed using a **Layered Architecture**. It strictly adheres to the **Repository Pattern** and **Unit of Work Pattern** to decouple application and domain logic from data persistence concerns. This design promotes separation of concerns, testability, and flexibility in switching or extending data storage technologies.

### 2.2. Design Principles
- **Dependency Inversion:** Concrete implementations (e.g., `HistoricalDataRepository`) depend on abstractions (e.g., `IHistoricalDataRepository`). Higher-level services will only depend on these abstractions, not the concrete implementations.
- **Separation of Concerns:** The implementation is physically and logically separated into three main areas: Relational, Time-Series, and BlobStorage. This isolates technology-specific code.
- **Single Responsibility:** Each repository is responsible for the persistence of a single aggregate root or closely related set of entities (e.g., `IAlarmEventRepository` handles only alarm events).
- **Configuration over Code:** Database connection strings, bucket names, and other environmental settings are externalized into configuration files, not hardcoded.

## 3. Component Specifications

### 3.1. Core Abstractions (`/Abstractions`)
This directory contains the contracts (interfaces) that define the capabilities of the data layer.

#### 3.1.1. `IUnitOfWork.cs`
- **Description:** Defines the contract for the Unit of Work pattern, ensuring that operations within the relational database are atomic.
- **Interface:** `IUnitOfWork`
- **Methods:**
  - `Task<int> SaveChangesAsync(CancellationToken cancellationToken)`: Persists all changes made in the current `AppDbContext` transaction to the database. It will wrap the `DbContext.SaveChangesAsync()` call.

#### 3.1.2. Repository Interfaces
These interfaces define the contracts for data access, abstracting the underlying storage technology.

- **`IHistoricalDataRepository.cs`**
  - **Purpose:** Contract for managing historical time-series data (`REQ-DLP-001`, `REQ-DLP-002`, `REQ-DLP-003`, `REQ-CSVC-013`).
  - **Methods:**
    - `Task IngestDataBatchAsync(IEnumerable<HistoricalDataPoint> dataBatch, CancellationToken cancellationToken)`: Writes a batch of historical data points.
    - `Task<IEnumerable<HistoricalDataPoint>> QueryDataAsync(HistoricalDataQuery query, CancellationToken cancellationToken)`: Retrieves historical data based on a flexible query object.

- **`IAlarmEventRepository.cs`**
  - **Purpose:** Contract for managing alarm and event data (`REQ-DLP-005`, `REQ-CSVC-019`).
  - **Methods:**
    - `Task IngestAlarmEventsAsync(IEnumerable<AlarmEventPoint> alarmEvents, CancellationToken cancellationToken)`: Writes a batch of alarm/event points.
    - `Task<IEnumerable<AlarmEventPoint>> QueryAlarmsAsync(AlarmEventQuery query, CancellationToken cancellationToken)`: Retrieves alarms based on a flexible query object.

- **`IAiArtifactRepository.cs`**
  - **Purpose:** Contract for managing unstructured AI artifacts in blob storage (`REQ-DLP-024`).
  - **Methods:**
    - `Task<Uri> UploadArtifactAsync(string artifactName, Stream content, CancellationToken cancellationToken)`: Uploads a file stream as an artifact.
    - `Task<Stream> DownloadArtifactAsync(string artifactName, CancellationToken cancellationToken)`: Downloads an artifact as a stream.
    - `Task DeleteArtifactAsync(string artifactName, CancellationToken cancellationToken)`: Deletes an artifact.

- **`IBlockchainLogRepository.cs`**
  - **Purpose:** Contract for managing off-chain metadata of blockchain transactions (`REQ-DLP-025`).
  - **Methods:**
    - `Task LogTransactionAsync(BlockchainTransaction logEntry, CancellationToken cancellationToken)`: Persists a new blockchain transaction record.
    - `Task<BlockchainTransaction?> GetTransactionByHashAsync(string dataHash, CancellationToken cancellationToken)`: Retrieves a transaction record by its data hash.

### 3.2. Relational Persistence (`/Relational`)

#### 3.2.1. `AppDbContext.cs`
- **Description:** The Entity Framework Core DbContext for the PostgreSQL database.
- **Inheritance:** `public class AppDbContext : DbContext, IUnitOfWork`
- **DbSets:**
  - `DbSet<User> Users { get; set; }`
  - `DbSet<Role> Roles { get; set; }`
  - `DbSet<Permission> Permissions { get; set; }`
  - `DbSet<UserRole> UserRoles { get; set; }`
  - `DbSet<RolePermission> RolePermissions { get; set; }`
  - `DbSet<OpcServer> OpcServers { get; set; }`
  - `DbSet<OpcTag> OpcTags { get; set; }`
  - `DbSet<Dashboard> Dashboards { get; set; }`
  - `DbSet<BlockchainTransaction> BlockchainTransactions { get; set; }`
- **`OnModelCreating(ModelBuilder modelBuilder)` Logic:**
  - This method will configure all entity relationships, primary keys, foreign keys, and unique constraints as defined in the database design.
  - **Example Constraint:** `modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();`
  - **Encryption at Rest (`REQ-DLP-008`):** For designated sensitive fields (e.g., a specific field in a configuration-related entity), an EF Core `ValueConverter` will be applied. This converter will use an injected `IEncryptionService` (defined in a shared/core project) to encrypt data before writing and decrypt it after reading, ensuring application-level encryption for data at rest.

#### 3.2.2. `Relational/Repositories/BlockchainLogRepository.cs`
- **Description:** Implements `IBlockchainLogRepository` using `AppDbContext`.
- **Constructor:** Injects `AppDbContext`.
- **Method Logic:**
  - `LogTransactionAsync`: Adds the `BlockchainTransaction` entity to the `_context.BlockchainTransactions` DbSet.
  - `GetTransactionByHashAsync`: Queries the DbSet using `FirstOrDefaultAsync(t => t.DataHash == dataHash)`.

### 3.3. Time-Series Persistence (`/TimeSeries`)

#### 3.3.1. `PersistenceModels/`
- **Purpose:** These are POCOs specifically for interacting with the InfluxDB client, decoupling the persistence model from the domain model.
- **`HistoricalDataPoint.cs`:**
  - **Attributes:** Will be decorated with `[Measurement("historical_data")]`, `[Column("tagId", IsTag = true)]`, `[Column("value")]`, `[Column("quality")]`, `[Column("timestamp", IsTimestamp = true)]`.
- **`AlarmEventPoint.cs`:**
  - **Attributes:** Will be decorated with `[Measurement("alarm_events")]`, `[Column("sourceNode", IsTag = true)]`, `[Column("eventType", IsTag = true)]`, etc., matching the schema from `REQ-DLP-005`.

#### 3.3.2. `TimeSeries/Repositories/HistoricalDataRepository.cs`
- **Description:** Implements `IHistoricalDataRepository` using `InfluxDB.Client`.
- **Constructor:** Injects `InfluxDBClient` and `IOptions<TimeSeriesDbOptions>`.
- **Method Logic:**
  - `IngestDataBatchAsync`:
    1. **Validation (`REQ-DLP-002`):** Before writing, it will perform basic sanity checks (e.g., ensuring timestamps are within a reasonable range). More complex validation rules will be handled by the calling service.
    2. **Batching:** Uses `_influxDbClient.GetWriteApiAsync().WritePointsAsync(...)` to send the entire batch in a single, optimized network request.
  - `QueryDataAsync`:
    1. **Query Building (`REQ-DLP-003`):** Dynamically constructs a Flux query string from the `HistoricalDataQuery` object. This includes `range`, `filter` (on tags like `tagId`), `aggregateWindow`, etc.
    2. **Execution:** Executes the query using `_influxDbClient.GetQueryApi().QueryAsync(...)`.
    3. **Error Handling:** Wraps the query execution in a try-catch block to handle `InfluxDBException` and other connection issues, re-throwing as a more specific custom exception (e.g., `TimeSeriesQueryException`) with an informative message.

### 3.4. Blob Storage Persistence (`/BlobStorage`)

#### 3.4.1. `BlobStorage/Repositories/AiArtifactRepository.cs`
- **Description:** Implements `IAiArtifactRepository` using `Azure.Storage.Blobs`.
- **Constructor:** Injects `BlobServiceClient` and `IOptions<BlobStorageOptions>`.
- **Method Logic:**
  - A private helper method `GetContainerClient()` will be used to get a reference to the configured container, creating it if it doesn't exist.
  - `UploadArtifactAsync`: Gets a `BlobClient` for the `artifactName` and calls `UploadAsync(content)`. Returns the blob's `Uri`.
  - `DownloadArtifactAsync`: Gets a `BlobClient` and returns the result of `OpenReadAsync()`.
  - `DeleteArtifactAsync`: Gets a `BlobClient` and calls `DeleteIfExistsAsync()`.

### 3.5. Cross-Cutting Services (`/Services`)

#### 3.5.1. `DataMaskingService.cs`
- **Description:** Implements data masking and anonymization logic (`REQ-DLP-009`).
- **Methods:**
  - `User MaskUserData(User user)`: Returns a new `User` object with sensitive fields (e.g., `Email`, `Username`) replaced with non-sensitive placeholders.
  - `IEnumerable<HistoricalDataPoint> AnonymizeHistoricalData(...)`: Iterates through the data, applying a random jitter to numerical values to obscure them while preserving trends.

## 4. Configuration and Dependency Injection

#### 4.1. `DependencyInjection.cs`
- **Description:** Contains the `AddInfrastructureData` extension method for `IServiceCollection`.
- **Logic:**
  1. Reads connection strings and options from `IConfiguration`.
  2. Registers `AppDbContext` with `services.AddDbContext<AppDbContext>(options => options.UseNpgsql(...))`.
  3. Registers `IUnitOfWork` to resolve to the `AppDbContext` instance: `services.AddScoped<IUnitOfWork, AppDbContext>();`.
  4. Registers `BlobServiceClient` and `InfluxDBClient` as singletons.
  5. Binds configuration sections (`TimeSeriesDbOptions`, `BlobStorageOptions`) using `services.Configure<T>()`.
  6. Registers all repositories with a scoped lifetime, e.g., `services.AddScoped<IHistoricalDataRepository, HistoricalDataRepository>();`.
  7. Registers `DataMaskingService`.

#### 4.2. Configuration Settings (`appsettings.json`)
The library will expect the following configuration structure:
json
{
  "ConnectionStrings": {
    "PostgresConnection": "...",
    "InfluxDBConnection": "http://localhost:8086",
    "AzureBlobStorage": "..."
  },
  "TimeSeriesDbOptions": {
    "Token": "...",
    "BucketName": "opc_data",
    "Organization": "my_org"
  },
  "BlobStorageOptions": {
    "ContainerName": "ai-artifacts"
  }
}


## 5. Security Considerations

- **Encryption at Rest (`REQ-DLP-008`):** As detailed in section 3.2.1, sensitive relational data will be encrypted at the application level using EF Core Value Converters and an injected encryption service.
- **Connection Security:** All database connections (PostgreSQL, InfluxDB, Azure Blob) will be configured to use SSL/TLS where supported by the underlying provider and environment.
- **Credential Management:** Connection strings and access keys will be managed via the .NET configuration system, allowing for secure storage using tools like Azure Key Vault, AWS Secrets Manager, or user-secrets in development. This library will not contain any hardcoded credentials.

## 6. Error Handling Strategy
- The data access layer will use a set of custom exceptions to communicate specific failure modes to the calling service layer.
- **`DataNotFoundException`**: Thrown when a query for a specific entity by its ID yields no result.
- **`ConcurrencyException`**: Thrown by the `UnitOfWork` if a concurrency conflict is detected during `SaveChangesAsync`.
- **`TimeSeriesQueryException` / `StorageException`**: Thrown by non-relational repositories to wrap provider-specific exceptions, providing a consistent error contract.
- Repositories will not handle business logic errors but will propagate exceptions related to data access failures. All public methods will be designed with `CancellationToken` parameters to support cancellation of long-running database operations.