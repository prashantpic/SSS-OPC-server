# Software Design Specification (SDS) for Management Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Management Service** (`services.management-service`). This microservice is a core component of the server-side application, responsible for the centralized configuration, management, and monitoring of a distributed fleet of OPC Client instances. It serves as the backend for the centralized management dashboard and provides programmatic interfaces for clients to register and report their status.

### 1.2. Scope
The scope of this document is limited to the design of the Management Service. This includes its internal architecture, API endpoints (REST and gRPC), domain logic, application features, and interactions with other services, specifically the **Data Service** for persistence and a **Message Broker** for asynchronous communication.

### 1.3. Audience
This document is intended for software developers, architects, and testers involved in the development and maintenance of the Management Service.

## 2. System Overview

### 2.1. Architectural Style
The Management Service is designed as a .NET 8 microservice following a **Clean Architecture** (or Onion Architecture) pattern. This approach separates concerns into distinct layers (Domain, Application, Infrastructure, Presentation/API) to promote maintainability, testability, and independence from external frameworks.

Key patterns employed include:
- **CQRS (Command Query Responsibility Segregation):** Using the MediatR library, the service separates write operations (Commands) from read operations (Queries), allowing for focused, single-responsibility handlers.
- **Repository Pattern:** The application layer defines interfaces for data persistence (`IOpcClientInstanceRepository`), which are implemented in the infrastructure layer. This decouples the application from the specific data storage mechanism (i.e., the Data Service).
- **RESTful API:** A REST API is exposed for synchronous, request-response interactions, primarily consumed by the frontend management dashboard.
- **gRPC API:** A high-performance gRPC API is exposed for service-to-service communication, specifically for OPC Client instances to register and report health status.
- **Event-Driven Communication (Publish-Subscribe):** The service publishes events (e.g., configuration updates) to a message broker (e.g., RabbitMQ) to asynchronously notify client instances, ensuring loose coupling and resilience.

### 2.2. Core Responsibilities
- **Client Instance Lifecycle:** Manage the registration of new OPC client instances.
- **Configuration Management:** Store, retrieve, and push configuration updates to single or multiple client instances (bulk operations).
- **Health Monitoring:** Receive and aggregate health status and Key Performance Indicators (KPIs) from all managed clients.
- **API Provisioning:** Provide robust REST and gRPC APIs to support the centralized dashboard and the OPC clients themselves.

### 2.3. Technology Stack
- **Framework:** ASP.NET Core 8
- **Language:** C# 12
- **APIs:** REST, gRPC
- **Application Logic:** MediatR (for CQRS)
- **Data Persistence:** Client for the central **Data Service** (via HTTP or gRPC). No direct database connection.
- **Messaging:** Client for a message broker like RabbitMQ or Kafka.

---

## 3. API Design

### 3.1. REST API (for Frontend Dashboard)
**Controller:** `ClientsController.cs`
**Base Path:** `/api/clients`

| Endpoint | Method | Description | Request Body | Response | Requirement(s) |
|---|---|---|---|---|---|
| `/` | `GET` | Retrieves a list of all managed OPC client instances with summary information. | - | `200 OK` with `List<ClientSummaryDto>` | REQ-6-001 |
| `/{id}` | `GET` | Retrieves detailed information for a specific client instance. | - | `200 OK` with `ClientDetailsDto`, `404 Not Found` | REQ-6-001 |
| `/{id}/configuration` | `PUT` | Updates the configuration for a single client instance. | `UpdateConfigurationRequest` | `204 No Content`, `404 Not Found` | REQ-6-001 |
| `/bulk/configuration` | `PUT` | Applies a single configuration to multiple client instances simultaneously. | `BulkConfigurationRequest` | `202 Accepted` | REQ-6-002 |
| `/kpis/aggregated` | `GET` | Retrieves aggregated KPIs for the entire fleet of clients (e.g., total connected, average throughput). | - | `200 OK` with `AggregatedKpisDto` | REQ-6-002 |

#### 3.1.1. Request/Response DTOs

- **`ClientSummaryDto`**: `{ guid id, string name, string site, string healthStatus, DateTimeOffset lastSeen }`
- **`ClientDetailsDto`**: `{ guid id, string name, string site, ClientConfigurationDto configuration, HealthStatusDto healthStatus, DateTimeOffset lastSeen }`
- **`UpdateConfigurationRequest`**: `{ ClientConfigurationDto configuration }`
- **`BulkConfigurationRequest`**: `{ List<Guid> clientIds, ClientConfigurationDto configuration }`
- **`AggregatedKpisDto`**: `{ int totalClients, int healthyClients, int unhealthyClients, double averageCpuUsage, double totalDataThroughput }`
- **`ClientConfigurationDto`**: `{ int pollingIntervalSeconds, List<TagConfigDto> tagConfigurations }`
- **`HealthStatusDto`**: `{ bool isConnected, double dataThroughput, double cpuUsagePercent }`

### 3.2. gRPC API (for OPC Client Instances)
**Proto File:** `protos/management.proto`
**Service:** `ClientLifecycleService`

protobuf
syntax = "proto3";
package management.v1;

// Service for OPC clients to register and report health
service ClientLifecycleService {
  // Registers a new client instance with the management service
  rpc RegisterClient (RegisterClientRequest) returns (RegisterClientReply);
  
  // Reports ongoing health status and KPIs from a client
  rpc ReportHealth (ReportHealthRequest) returns (ReportHealthReply);
}

message RegisterClientRequest {
  string name = 1;
  string site = 2;
}

message RegisterClientReply {
  string client_id = 1; // The new UUID for the client
  ClientConfigurationMessage configuration = 2;
}

message ReportHealthRequest {
  string client_id = 1;
  HealthStatusMessage health_status = 2;
}

message ReportHealthReply {
  // Can be empty or contain new commands/configurations in the future
}

// Data Structures
message ClientConfigurationMessage {
  // ... fields matching ClientConfigurationDto
}

message HealthStatusMessage {
  // ... fields matching HealthStatusDto
}


---

## 4. Domain Model (`ManagementService.Domain`)

The domain model is the core of the service, containing business logic and state, independent of any infrastructure concerns.

### 4.1. Aggregates
- **`OpcClientInstance` (Aggregate Root)**
    - **Purpose:** Represents a single managed OPC client. It is the consistency boundary for all operations related to a client.
    - **Properties:**
        - `Id` (Guid): The unique identifier.
        - `Name` (string): A user-friendly name.
        - `Site` (string): The physical or logical location of the client.
        - `LastSeen` (DateTimeOffset): Timestamp of the last health report.
        - `HealthStatus` (HealthStatus): The last reported health status (value object).
        - `Configuration` (ClientConfiguration): The current configuration (value object).
    - **Methods:**
        - `public static OpcClientInstance Register(Guid id, string name, string site)`: Factory method to create a new instance, ensuring initial state is valid.
        - `public void UpdateConfiguration(ClientConfiguration newConfiguration)`: Applies a new configuration. It will validate the configuration and raise a `ClientConfigurationUpdated` domain event.
        - `public void ReportHealth(HealthStatus newStatus, DateTimeOffset reportTime)`: Updates the health status and `LastSeen` timestamp. Raises a `ClientHealthReported` domain event.

### 4.2. Value Objects
- **`ClientConfiguration`**
    - **Purpose:** An immutable representation of a client's settings.
    - **Properties:** `PollingIntervalSeconds`, `IReadOnlyList<TagConfig>`.
    - **Behavior:** Implements value-based equality.
- **`HealthStatus`**
    - **Purpose:** An immutable snapshot of a client's health KPIs.
    - **Properties:** `IsConnected`, `DataThroughput`, `CpuUsagePercent`.
    - **Behavior:** Implements value-based equality.

## 5. Application Layer (`ManagementService.Application`)

This layer orchestrates the use of domain objects and defines the application's features. It is independent of the UI and the database implementation.

### 5.1. CQRS Commands and Handlers
- **`RegisterClientCommand`**: Handles registration of a new client.
    - **Handler Logic:** Calls `OpcClientInstance.Register()`, persists the new aggregate using `IOpcClientInstanceRepository`, and returns the new client ID and initial configuration.
- **`UpdateClientConfigurationCommand`**: Handles updating a single client's config.
    - **Handler Logic:** Fetches the aggregate, calls `aggregate.UpdateConfiguration()`, persists the change, and publishes an event to the message bus.
- **`ExecuteBulkConfigurationCommand`**: Handles the bulk update feature.
    - **Handler Logic:** Iterates through client IDs. For each ID, it executes the same logic as `UpdateClientConfigurationCommand`. It should handle transactions and partial failures gracefully.
- **`ReportHealthCommand`**: Handles incoming health reports.
    - **Handler Logic:** Fetches the aggregate, calls `aggregate.ReportHealth()`, and persists the change.

### 5.2. CQRS Queries and Handlers
- **`GetAllClientsQuery`**: Fetches all client summaries.
    - **Handler Logic:** Calls `_repository.GetAllAsync()`, maps the results to `List<ClientSummaryDto>`.
- **`GetClientDetailsQuery`**: Fetches details for one client.
    - **Handler Logic:** Calls `_repository.GetByIdAsync()`, maps the result to `ClientDetailsDto`. Handles "not found" cases.
- **`GetAggregatedKpisQuery`**: Calculates and returns fleet-wide KPIs.
    - **Handler Logic:** Fetches all clients, performs in-memory aggregation (e.g., counting statuses, averaging CPU), and returns the `AggregatedKpisDto`.

### 5.3. Contracts
- **`IOpcClientInstanceRepository`**:
    - **Purpose:** Defines the contract for persistence.
    - **Methods:** `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`.
- **`IConfigurationUpdatePublisher`**:
    - **Purpose:** Defines the contract for publishing configuration update events.
    - **Method:** `PublishUpdateAsync(Guid clientId, ClientConfiguration newConfiguration)`.

## 6. Infrastructure Layer (`ManagementService.Infrastructure`)

This layer contains implementations for external-facing concerns.

### 6.1. Persistence
- **`OpcClientInstanceRepository`**:
    - **Implementation:** Implements `IOpcClientInstanceRepository`.
    - **Logic:** It will **not** use an ORM like Entity Framework Core. Instead, it will inject an `HttpClient` or a gRPC client for the **Data Service**. Each method will make a corresponding API call to the Data Service to perform the CRUD operation. It is responsible for serializing/deserializing DTOs for the Data Service communication.

### 6.2. Messaging
- **`ConfigurationUpdatePublisher`**:
    - **Implementation:** Implements `IConfigurationUpdatePublisher`.
    - **Logic:** Injects a RabbitMQ/Kafka client. The `PublishUpdateAsync` method will serialize the configuration data and publish it to a specific topic/exchange (e.g., `client.config.updates`) with a routing key based on the `clientId`.

## 7. Cross-Cutting Concerns

- **`Program.cs`:**
    - Configures Kestrel for both HTTP/1.1 (REST) and HTTP/2 (gRPC).
    - Registers all services with the DI container: MediatR, repositories, controllers, gRPC services, publishers, and the `DataServiceClient`.
    - Configures structured logging with Serilog.
    - Adds global exception handling middleware to return standardized error responses.
    - Adds ASP.NET Core Health Checks endpoint (`/health`).
- **`appsettings.json`:**
    - `DataServiceUrl`: The base URL for the Data Service.
    - `MessageBroker`: Connection details (hostname, user, password) for RabbitMQ/Kafka.
    - `Serilog`: Configuration for logging levels and sinks.