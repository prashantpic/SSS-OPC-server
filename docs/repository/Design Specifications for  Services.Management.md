# Software Design Specification: Management Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the **Management Service** (`Opc.System.Services.Management`). This microservice is a core component of the server-side application, responsible for the centralized configuration, monitoring, and administration of all deployed OPC Client instances. It serves as the backend for the central management dashboard, providing the necessary APIs for all administrative functions.

### 1.2. Scope
The scope of this document is limited to the `Services.Management` microservice. This includes:
-   The domain model for OPC client instances and configuration migration strategies.
-   Application logic for handling client configuration, health monitoring, and bulk operations.
-   Persistence mechanisms for storing management-related data.
-   RESTful and gRPC APIs exposed for consumption by the front-end (UI) and other services.

This service **does not** directly handle real-time OPC data streams or user authentication tokens, which are the responsibilities of other dedicated services. It does, however, store the configurations that enable those operations.

## 2. System Overview

### 2.1. Architectural Style
The Management Service is designed as a .NET 8 microservice following a **Clean Architecture** (also known as Onion Architecture). This approach ensures a clear separation of concerns, high testability, and independence from external frameworks and technologies.

The core architectural patterns employed are:
-   **CQRS (Command Query Responsibility Segregation):** The application logic is split into Commands (state-changing operations) and Queries (data-retrieval operations). This separation simplifies the models and allows for independent optimization of read and write paths.
-   **Domain-Driven Design (DDD):** The core of the service is the domain model, which encapsulates complex business logic and rules. We use DDD tactical patterns like **Aggregates**, **Entities**, and **Repositories**.
-   **Repository Pattern:** Data access is abstracted through repository interfaces defined in the Domain layer, with implementations in the Infrastructure layer.
-   **Dependency Injection (DI):** The built-in .NET DI container is used to manage dependencies and decouple components.

### 2.2. Service Responsibilities
This service is the authoritative source for:
-   **REQ-SAP-002, REQ-6-001, REQ-9-004:** Providing all functionalities for centralized management.
-   **REQ-6-002, REQ-9-005:** Supporting bulk operations (e.g., applying a configuration template to multiple clients).
-   **REQ-6-002, REQ-9-005:** Aggregating and providing health and performance KPIs for managed clients.
-   **REQ-SAP-009:** Storing and managing strategies for migrating configurations from legacy systems.

## 3. Layered Architecture

The service is structured into four distinct layers: Domain, Application, Infrastructure, and Presentation (API).

### 3.1. Domain Layer
This layer contains the core business logic and models, with no dependencies on other layers.
**Namespace:** `Opc.System.Services.Management.Domain`

#### 3.1.1. Aggregates and Entities

-   **`OpcClientInstance` (Aggregate Root):**
    -   **Description:** Represents a single managed OPC Client instance. It is the consistency boundary for all operations on a client.
    -   **Properties:**
        -   `Id` (ClientInstanceId): Unique identifier.
        -   `Name` (string): A user-friendly name for the instance.
        -   `Configuration` (ClientConfiguration): The complete configuration object for the client.
        -   `LastSeen` (DateTimeOffset): Timestamp of the last heartbeat received.
        -   `HealthStatus` (HealthStatus): An enum representing the last reported health (e.g., Healthy, Degraded, Unhealthy).
        -   `IsActive` (bool): A flag to enable or disable the client instance.
    -   **Behavior:**
        -   `UpdateConfiguration(ClientConfiguration newConfig)`: Validates and applies a new configuration. Raises a `ClientConfigurationUpdated` domain event.
        -   `ReportHeartbeat(HealthStatus status)`: Updates `LastSeen` and `HealthStatus`.
        -   `Deactivate()`: Sets `IsActive` to false.

-   **`ClientConfiguration` (Entity/Value Object):**
    -   **Description:** A complex value object owned by `OpcClientInstance` that encapsulates all its settings.
    -   **Properties:**
        -   `TagConfigurations` (IReadOnlyCollection<TagConfiguration>): List of tag settings.
        -   `ServerConnections` (IReadOnlyCollection<ServerConnectionSetting>): OPC Server connection details.
        -   `PollingInterval` (TimeSpan): The client's main polling interval.
    -   **Logic:** Can contain validation in its constructor to ensure internal consistency.

-   **`MigrationStrategy` (Aggregate Root):**
    -   **Description:** Models a plan for migrating configurations from a legacy system.
    -   **Properties:**
        -   `Id` (MigrationStrategyId): Unique identifier.
        -   `SourceSystem` (string): Name of the legacy system (e.g., "LegacyKepwareCsv").
        -   `MappingRules` (string - JSON): JSON defining how to map legacy fields to the new `ClientConfiguration` model.
        -   `ValidationScript` (string): A script or description of validation steps.

#### 3.1.2. Repository Interfaces

-   **`IOpcClientInstanceRepository`:**
    -   **Description:** Defines the persistence contract for the `OpcClientInstance` aggregate.
    -   **Methods:**
        -   `Task<OpcClientInstance?> GetByIdAsync(ClientInstanceId id, CancellationToken ct)`
        -   `Task<IEnumerable<OpcClientInstance>> GetAllAsync(CancellationToken ct)`
        -   `Task<IEnumerable<OpcClientInstance>> GetByIdsAsync(IEnumerable<ClientInstanceId> ids, CancellationToken ct)`
        -   `Task AddAsync(OpcClientInstance clientInstance, CancellationToken ct)`
        -   `Task UpdateAsync(OpcClientInstance clientInstance, CancellationToken ct)`

### 3.2. Application Layer
This layer orchestrates domain logic to implement application use cases using the CQRS pattern. It depends only on the Domain layer.
**Namespace:** `Opc.System.Services.Management.Application`

#### 3.2.1. Commands

-   **`UpdateClientConfigurationCommand`**:
    -   **Handler Logic:**
        1.  Retrieve `OpcClientInstance` aggregate from the repository.
        2.  If not found, return a failure result.
        3.  Map the command's DTO to a `ClientConfiguration` domain object.
        4.  Call `aggregate.UpdateConfiguration(newConfig)`.
        5.  Call `repository.UpdateAsync(aggregate)`.
        6.  Commit the unit of work.
        7.  Return a success result.

-   **`ExecuteBulkUpdateCommand`**:
    -   **Handler Logic:**
        1.  Retrieve all specified `OpcClientInstance` aggregates using `repository.GetByIdsAsync`.
        2.  Initialize a results DTO.
        3.  For each retrieved aggregate:
            a. Map the command's DTO to a `ClientConfiguration` domain object.
            b. Call `aggregate.UpdateConfiguration(newConfig)`.
            c. Call `repository.UpdateAsync(aggregate)`.
            d. Record success for this ID.
        4.  Commit the unit of work.
        5.  Return the results DTO.

-   **`ImportConfigurationCommand`**:
    -   **Handler Logic:**
        1.  Use `IConfigurationParserFactory` to get the appropriate parser based on file type/source system.
        2.  Invoke the parser to get a `ClientConfiguration` domain object.
        3.  Create a new `OpcClientInstance` aggregate.
        4.  Call `repository.AddAsync(newInstance)`.
        5.  Commit the unit of work and return the new instance's ID.

#### 3.2.2. Queries

-   **`GetAllClientsQuery`**:
    -   **Handler Logic:**
        1.  Call `repository.GetAllAsync()`.
        2.  Map the list of `OpcClientInstance` aggregates to a list of `ClientSummaryDto` (containing Id, Name, HealthStatus, LastSeen).
        3.  Return the list of DTOs.

-   **`GetClientDetailsQuery`**:
    -   **Handler Logic:**
        1.  Call `repository.GetByIdAsync()`.
        2.  If found, map the `OpcClientInstance` to a detailed `ClientDetailsDto` (including the full configuration).
        3.  Return the DTO or a not-found result.

### 3.3. Infrastructure Layer
This layer contains implementations for abstractions defined in the layers above.
**Namespace:** `Opc.System.Services.Management.Infrastructure`

-   **`ManagementDbContext` (EF Core `DbContext`):**
    -   Implements the Unit of Work pattern.
    -   Contains `DbSet<OpcClientInstance>` and `DbSet<MigrationStrategy>`.
    -   **`OnModelCreating` Configuration:**
        -   Configures the `OpcClientInstance` entity.
        -   The `ClientConfiguration` property will be configured to be stored as a JSONB column using `OwnsOne` and `ToJson()`, simplifying the schema and improving flexibility.
        -   Sets up primary keys, value converters for strongly-typed IDs, and indexes.

-   **`OpcClientInstanceRepository`:**
    -   Implements `IOpcClientInstanceRepository`.
    -   Uses `ManagementDbContext` to perform database operations (e.g., `_context.OpcClientInstances.FindAsync(id)`).
    -   Leverages EF Core's change tracking for update operations.

### 3.4. Presentation (API) Layer
This layer exposes the service's functionality via APIs.
**Namespace:** `Opc.System.Services.Management.Api`

-   **`ClientInstancesController` (`[ApiController]`)**:
    -   **`GET /api/client-instances`**: Dispatches `GetAllClientsQuery`.
    -   **`GET /api/client-instances/{id}`**: Dispatches `GetClientDetailsQuery`.
    -   **`PUT /api/client-instances/{id}/configuration`**: Creates and dispatches `UpdateClientConfigurationCommand`.

-   **`BulkOperationsController` (`[ApiController]`)**:
    -   **`POST /api/bulk/update-configuration`**: Creates and dispatches `ExecuteBulkUpdateCommand`.

-   **`MigrationController` (`[ApiController]`)**:
    -   **`POST /api/migrations/import`**: Creates and dispatches `ImportConfigurationCommand` from a file upload.

-   **`Program.cs`:**
    -   Configures the ASP.NET Core pipeline.
    -   Registers all services with the DI container (Scoped for repositories and DbContext, Transient for handlers).
    -   Adds MediatR for CQRS implementation.
    -   Configures structured logging with Serilog.
    -   Adds global exception handling middleware.

## 4. API Specification

### `ClientInstancesController`
-   **`GET /api/client-instances`**
    -   **Description:** Retrieves a summary list of all managed clients.
    -   **Response `200 OK`:** `application/json` - `IEnumerable<ClientSummaryDto>`
    -   `ClientSummaryDto`: `{ "id": "guid", "name": "string", "healthStatus": "string", "lastSeen": "datetime" }`

-   **`PUT /api/client-instances/{id}/configuration`**
    -   **Description:** Updates the configuration for a specific client.
    -   **Request Body:** `application/json` - `UpdateConfigurationRequest` (maps to `ClientConfigurationDto`)
    -   **Response `204 No Content`:** On success.
    -   **Response `404 Not Found`:** If client with `{id}` does not exist.

### `BulkOperationsController`
-   **`POST /api/bulk/update-configuration`**
    -   **Description:** Applies a single configuration to multiple clients.
    -   **Request Body:** `application/json` - `BulkUpdateRequest`: `{ "clientInstanceIds": ["guid"], "configurationToApply": { ... } }`
    -   **Response `200 OK`:** `application/json` - `BulkUpdateResultDto`: `{ "successfulIds": ["guid"], "failedIds": ["guid"] }`

## 5. Data Model
The service will manage the following primary tables in a relational database (PostgreSQL/SQL Server).

-   **`OpcClientInstances`**
    -   `Id` (uuid, PK)
    -   `Name` (varchar(100), not null)
    -   `Configuration` (jsonb, not null) - *Contains the entire ClientConfiguration object*
    -   `LastSeen` (timestamp with time zone)
    -   `HealthStatus` (varchar(20), not null)
    -   `IsActive` (boolean, not null)
    -   `CreatedAt` (timestamp with time zone, not null)
    -   `UpdatedAt` (timestamp with time zone, not null)

-   **`MigrationStrategies`**
    -   `Id` (uuid, PK)
    -   `SourceSystem` (varchar(50), not null)
    -   `MappingRules` (jsonb, not null)
    -   `ValidationScript` (text)

## 6. Cross-Cutting Concerns

-   **Logging:** Serilog will be configured in `Program.cs` to write structured JSON logs to the console and/or a configured log sink. A middleware will log every incoming HTTP request and its response status code.
-   **Error Handling:** A custom exception handling middleware will be implemented. It will catch any unhandled exceptions, log them, and return a standardized `500 Internal Server Error` response with a problem details JSON payload.
-   **Configuration:** Standard .NET configuration providers will be used, reading from `appsettings.json`, environment variables, and command-line arguments. Key configurations include database connection strings.
-   **Validation:** FluentValidation will be used to validate incoming command and query objects at the application layer boundary, ensuring all requests are well-formed before processing.