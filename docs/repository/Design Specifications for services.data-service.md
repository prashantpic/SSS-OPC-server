# Software Design Specification (SDS) for Data Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Data Service**, a microservice within the SSS-OPC-Client ecosystem. This service acts as a centralized and abstracted persistence layer, managing all data storage and retrieval operations for the entire system. It is designed to be a robust, scalable, and maintainable data hub, decoupling business logic from the underlying storage technologies.

### 1.2. Scope
The scope of this SDS covers the design of the following components:
- A RESTful API for synchronous data access and configuration management.
- Asynchronous data ingestion consumers for high-throughput data streams.
- A background worker process for automated data lifecycle management.
- Data access implementations for PostgreSQL (relational) and InfluxDB (time-series).
- Core domain logic and business rules for data entities and retention policies.

This service is responsible for fulfilling the following key requirements: `REQ-DLP-001` (Historical Data Storage), `REQ-DLP-008` (Secure Configuration Storage), and `REQ-DLP-017` (Data Retention Policies).

## 2. System Overview and Architecture

The Data Service adopts a **Clean Architecture** style, promoting a clear separation of concerns, dependency inversion, and testability. The architecture is composed of several projects, each representing a distinct layer.

mermaid
graph TD
    subgraph DataService.Api [Presentation Layer]
        A[Controllers]
        B[Messaging Consumers]
    end

    subgraph DataService.Application [Application Layer]
        C[Commands & Handlers]
        D[Queries & Handlers]
        E[Interfaces (Repositories, etc.)]
        F[DTOs]
    end

    subgraph DataService.Domain [Domain Layer]
        G[Entities]
        H[Domain Events]
        I[Repository Interfaces]
    end

    subgraph DataService.Infrastructure [Infrastructure Layer]
        J[Relational DB Context (EF Core)]
        K[Time-Series Client (InfluxDB)]
        L[Repository Implementations]
    end
    
    subgraph DataService.Worker [Background Process]
        M[Hosted Services (e.g., RetentionWorker)]
    end

    A --> C & D
    B --> C & D
    C & D --> E
    M --> E
    
    L --implements--> I
    J & K --> L

    subgraph Dependencies
        direction LR
        Application --> Domain
        Api --> Application
        Infrastructure --> Application
        Worker --> Application
    end



- **DataService.Domain:** The core of the service. Contains enterprise-wide business rules, entities, and repository interfaces. It has no dependencies on other layers.
- **DataService.Application:** Contains application-specific business logic. It orchestrates the domain entities to perform use cases, defined by CQRS commands and queries. It depends on the Domain layer.
- **DataService.Infrastructure:** Provides implementations for the interfaces defined in the Application and Domain layers. It contains all data access code, interactions with external services, and other technology-specific details. It depends on the Application layer.
- **DataService.Api:** The entry point for the service. It exposes RESTful endpoints and hosts message queue consumers. It handles requests, dispatches them to the Application layer (via MediatR), and returns responses.
- **DataService.Worker:** A separate, long-running background process host. It triggers periodic jobs, such as enforcing data retention policies, by interacting with the Application layer.

## 3. Domain Layer Design (`DataService.Domain`)

This layer contains the core business models and rules.

### 3.1. Entities

#### 3.1.1. `UserConfiguration.cs`
Represents a key-value configuration setting.
- **Purpose:** Fulfills `REQ-DLP-008` for storing user and system configurations.
- **Properties:**
  - `Guid Id`: Primary Key.
  - `string Key`: The unique configuration key.
  - `string Value`: The configuration value.
  - `string? DataType`: Information about the value's type (e.g., "string", "int", "json").
  - `bool IsEncrypted`: A flag indicating if the `Value` field is encrypted in the database.

#### 3.1.2. `DataRetentionPolicy.cs`
Represents a rule for data lifecycle management.
- **Purpose:** Fulfills `REQ-DLP-017`.
- **Properties:**
  - `Guid Id`: Primary Key.
  - `DataType DataType`: An enum (`Historical`, `Alarm`, `Audit`, `AI`) specifying the data type this policy applies to.
  - `int RetentionPeriodDays`: The number of days to retain the data.
  - `RetentionAction Action`: An enum (`Purge`, `Archive`) specifying the action to take after the retention period.
  - `string? ArchiveLocation`: The location for archiving data (e.g., an S3 bucket path or connection string).
  - `bool IsActive`: Whether the policy is currently enforced.

#### 3.1.3. `HistoricalDataPoint.cs`
Represents a single time-series data point. This is a conceptual entity for transfer and logic, not necessarily an EF Core entity.
- **Purpose:** Fulfills `REQ-DLP-001`.
- **Properties:**
  - `string Measurement`: The source measurement or tag group.
  - `Guid TagId`: Foreign key to the configured OPC tag.
  - `DateTimeOffset Timestamp`: The time of the data point.
  - `object Value`: The value of the data point.
  - `string Quality`: The OPC quality string.
  - `Dictionary<string, string> Tags`: Additional metadata tags for InfluxDB.

### 3.2. Repository Interfaces

#### 3.2.1. `IRelationalRepository<T>`
A generic repository interface for entities stored in the relational database.
- **Methods:**
  - `Task<T?> GetByIdAsync(Guid id)`
  - `Task<IEnumerable<T>> GetAllAsync()`
  - `Task AddAsync(T entity)`
  - `void Update(T entity)`
  - `void Delete(T entity)`

#### 3.2.2. `ITimeSeriesRepository.cs`
Defines the contract for interacting with the time-series database.
- **Methods:**
  - `Task AddHistoricalDataBatchAsync(IEnumerable<HistoricalDataPoint> dataPoints, CancellationToken cancellationToken)`: Writes a batch of historical data points.
  - `Task AddAlarmEventsBatchAsync(IEnumerable<AlarmEvent> alarmEvents, CancellationToken cancellationToken)`: Writes a batch of alarm events.
  - `Task<IEnumerable<T>> QueryAsync<T>(string fluxQuery, CancellationToken cancellationToken)`: Executes a generic Flux query.
  - `Task DeleteDataBeforeAsync(string bucket, DateTimeOffset cutoff, CancellationToken cancellationToken)`: Deletes data older than a specified timestamp from a bucket.

#### 3.2.3. `IUnitOfWork.cs`
Represents the Unit of Work pattern for the relational database.
- **Methods:**
  - `Task<int> SaveChangesAsync(CancellationToken cancellationToken)`: Persists all changes made in a single transaction.
  - **Repositories:** Properties for each specific relational repository, e.g., `IRelationalRepository<DataRetentionPolicy> Policies { get; }`.

## 4. Application Layer Design (`DataService.Application`)

This layer implements the CQRS pattern using the `MediatR` library.

### 4.1. Features

#### 4.1.1. Historical Data Management
- **Command:** `StoreHistoricalDataBatchCommand(IEnumerable<HistoricalDataDto> DataPoints)`
- **Handler:** `StoreHistoricalDataBatchCommandHandler`
  - **Dependencies:** `ITimeSeriesRepository`
  - **Logic:**
    1. Maps the incoming `IEnumerable<HistoricalDataDto>` to a collection of `HistoricalDataPoint` domain objects.
    2. Calls `_timeSeriesRepository.AddHistoricalDataBatchAsync()` to persist the data.
    3. Handles potential exceptions from the repository and logs errors.
- **Query:** `GetHistoricalDataQuery(Guid TagId, DateTimeOffset StartTime, DateTimeOffset EndTime, string? Aggregate)`
- **Handler:** `GetHistoricalDataQueryHandler`
  - **Dependencies:** `ITimeSeriesRepository`
  - **Logic:**
    1. Constructs a Flux query string based on the query parameters (tag, time range, optional aggregation).
    2. Calls `_timeSeriesRepository.QueryAsync()` to retrieve data.
    3. Maps the results to `HistoricalDataDto` and returns the collection.

#### 4.1.2. Data Retention Policy Management
- **Command:** `UpdatePolicyCommand(Guid? PolicyId, string DataType, int RetentionDays, string Action, bool IsActive)`
- **Handler:** `UpdatePolicyCommandHandler`
  - **Dependencies:** `IUnitOfWork`
  - **Logic:**
    1. If `PolicyId` is provided, fetches the `DataRetentionPolicy` entity using `_unitOfWork.Policies.GetByIdAsync()`. If not found, throws `NotFoundException`.
    2. If `PolicyId` is null, creates a new `DataRetentionPolicy` instance.
    3. Updates the entity's properties from the command's data.
    4. Performs validation (e.g., RetentionDays > 0).
    5. Calls `_unitOfWork.SaveChangesAsync()` to commit the transaction.
- **Query:** `GetPoliciesQuery()`
- **Handler:** `GetPoliciesQueryHandler`
  - **Dependencies:** `IUnitOfWork`
  - **Logic:**
    1. Calls `_unitOfWork.Policies.GetAllAsync()`.
    2. Maps the entities to `DataRetentionPolicyDto` objects and returns the list.

## 5. Infrastructure Layer Design (`DataService.Infrastructure`)

This layer provides concrete implementations of the domain interfaces.

### 5.1. Persistence

#### 5.1.1. Relational Persistence (`AppDbContext.cs`)
- **Technology:** Entity Framework Core with `Npgsql` provider for PostgreSQL.
- **Responsibilities:**
  - Defines `DbSet<UserConfiguration>` and `DbSet<DataRetentionPolicy>`.
  - Implements the `IUnitOfWork` interface.
  - In `OnModelCreating`, it configures entity mappings: table names, primary keys, constraints, and unique indexes.
  - It will contain logic to use a value converter for transparently encrypting/decrypting the `UserConfiguration.Value` field if `IsEncrypted` is true, fulfilling the encryption part of `REQ-DLP-008`.

#### 5.1.2. Time-Series Persistence (`TimeSeriesRepository.cs`)
- **Technology:** `InfluxDB.Client` library.
- **Dependencies:** `IInfluxDBClient` injected via DI.
- **Responsibilities:** Implements `ITimeSeriesRepository`.
- **Method Implementation:**
  - `AddHistoricalDataBatchAsync`:
    1. Creates a `WriteApi` instance from the client.
    2. Iterates through the `HistoricalDataPoint` collection.
    3. For each point, creates an InfluxDB `PointData` object, mapping domain properties to InfluxDB concepts (`.Measurement()`, `.Tag()`, `.Field()`, `.Timestamp()`).
    4. Calls `writeApi.WritePointsAsync()` with the batch of points.
  - `DeleteDataBeforeAsync`:
    1. Creates a `DeleteApi` instance.
    2. Calls `deleteApi.Delete()` with the specified bucket, time range, and a predicate (if needed).

### 5.2. Dependency Injection (`DependencyInjection.cs`)
- An extension method `AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)` will be created.
- **Logic:**
  1. Registers `AppDbContext` with the connection string from `IConfiguration`.
  2. Registers `IInfluxDBClient` as a singleton, configured with URL, token, and org from `IConfiguration`.
  3. Scopes repository implementations: `services.AddScoped<ITimeSeriesRepository, TimeSeriesRepository>();`
  4. Scopes the `IUnitOfWork`: `services.AddScoped<IUnitOfWork, UnitOfWork>();`

## 6. API Layer Design (`DataService.Api`)

### 6.1. Controllers
- All controllers will be lean, containing minimal logic. They will depend on `ISender` (MediatR) and dispatch commands/queries.
- **`HistoricalDataController`:**
  - `GET /api/historical-data`: Accepts query parameters, creates a `GetHistoricalDataQuery`, sends it via MediatR, and returns the result.
- **`DataRetentionController`:**
  - `GET /api/retention-policies`: Sends `GetPoliciesQuery`.
  - `POST /api/retention-policies`: Accepts a command DTO, creates an `UpdatePolicyCommand`, sends it, and returns `Ok` or `NoContent`.
  - `PUT /api/retention-policies/{id}`: Similar to POST but includes the ID in the command.

### 6.2. Messaging (`DataIngestionConsumer.cs`)
- **Type:** A class inheriting from `Microsoft.Extensions.Hosting.BackgroundService`.
- **Dependencies:** `ISender` (MediatR), and a message bus client interface (e.g., `IMessageBusConsumer`).
- **Logic in `ExecuteAsync`:**
  1. Enters a `while` loop that runs as long as `stoppingToken` is not cancelled.
  2. Subscribes to the "historical-data-topic" on the message bus.
  3. When a message is received:
     a. Deserializes the JSON payload into a `StoreHistoricalDataBatchCommand`.
     b. Dispatches the command using `_mediator.Send()`.
     c. Handles success/failure: Acknowledges the message on success, or sends it to a dead-letter queue/logs the error on failure.

## 7. Worker Service Design (`DataService.Worker`)

### 7.1. `DataRetentionWorker.cs`
- **Type:** A class inheriting from `Microsoft.Extensions.Hosting.BackgroundService`.
- **Dependencies:** `IServiceProvider` to create a new scope for each run.
- **Logic in `ExecuteAsync`:**
  1. Enters a `while` loop controlled by a `PeriodicTimer` (e.g., running every 24 hours).
  2. Inside the loop, it creates a new DI scope to resolve services.
  3. Resolves `IUnitOfWork` and `ITimeSeriesRepository` from the scope.
  4. Fetches all active `DataRetentionPolicy` entities via `_unitOfWork.Policies.GetAllAsync()`.
  5. Iterates through each policy:
     a. Calculates the cutoff timestamp (`DateTimeOffset.UtcNow.AddDays(-policy.RetentionPeriodDays)`).
     b. Determines the target InfluxDB bucket based on `policy.DataType`.
     c. Calls `_timeSeriesRepository.DeleteDataBeforeAsync()` with the bucket and cutoff time.
     d. Logs the action, including the policy name and number of records affected (if possible).
  6. Handles exceptions gracefully within the loop to prevent the worker from crashing.