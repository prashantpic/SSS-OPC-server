# Software Design Specification: Shared.Kernel

## 1. Introduction

### 1.1. Purpose

This document provides the detailed software design specification for the `shared.kernel` repository (`REPO-SAP-012`). This repository serves as a foundational Shared Kernel class library for a distributed industrial data platform. Its primary purpose is to define and share common domain concepts, data contracts, API definitions, and base types across all microservices in the system. By centralizing these shared assets, it ensures consistency, reduces code duplication, and establishes a ubiquitous language for inter-service communication.

### 1.2. Scope

The scope of this library is strictly limited to shared, non-executable code. It includes:
-   **Data Contracts:** Data Transfer Objects (DTOs) for REST APIs and message queue events.
-   **API Definitions:** Protocol Buffers (`.proto`) files defining the contracts for gRPC services.
-   **Domain SeedWork:** Base classes and interfaces for implementing Domain-Driven Design (DDD) patterns (e.g., Entity, Aggregate Root, Domain Events).
-   **Shared Domain Objects:** Common enumerations and value objects used across multiple bounded contexts.
-   **Custom Exceptions:** A base exception type for domain-specific errors.
-   **Core Abstractions:** Common interfaces for cross-cutting concerns to be implemented by higher-level services.

This library **does not** contain any business logic, infrastructure implementations, or executable code. It is designed to be a lightweight, highly-stable dependency for all other service repositories.

## 2. Architectural Design

### 2.1. Architectural Style

The `shared.kernel` library is designed as a **Shared Kernel** in the context of a **Domain-Driven Design (DDD)** and **Microservices Architecture**. It acts as the central, shared part of the domain model that all services agree upon.

### 2.2. Technology Stack

-   **Framework:** .NET 8
-   **Project Type:** .NET Standard Class Library
-   **Language:** C# 12
-   **API Contract Language:** Protocol Buffers 3 (proto3)

### 2.3. Dependencies

This project has **no internal project dependencies**. It may have minimal, carefully selected third-party dependencies (e.g., Google.Protobuf libraries for tooling). All other C# projects in the solution will reference `shared.kernel`.

## 3. Detailed Component Specification

This section details the design of each file and component within the `shared.kernel` library.

### 3.1. Project Configuration

#### 3.1.1. `src/shared.kernel/SharedKernel.csproj`

-   **Description:** The .NET project file.
-   **Specification:**
    -   It will be configured as a .NET 8 class library: `<TargetFramework>net8.0</TargetFramework>`.
    -   It will enable nullable reference types: `<Nullable>enable</Nullable>`.
    -   It will include `Protobuf` item groups to compile the `.proto` files for C#, specifying `GrpcServices="Both"` to generate both client and server stubs. This allows consuming projects to choose the appropriate base class.
    -   It will reference the necessary gRPC and Protobuf packages, such as `Google.Protobuf`, `Grpc.Tools`, and `Grpc.AspNetCore`.

### 3.2. gRPC API Contracts

These files define the service contracts for synchronous, cross-service communication. They are the single source of truth for the gRPC APIs.

#### 3.2.1. `src/shared.kernel/Contracts/Grpc/management.proto`

-   **Description:** Defines the gRPC contract for the Management Service.
-   **Specification (proto3 syntax):**

protobuf
syntax = "proto3";

import "google/protobuf/empty.proto";

package Contracts.Grpc;

// Service for managing OPC Client instances and configurations.
service ManagementService {
  // Retrieves the configuration for a specific OPC client.
  rpc GetClientConfiguration(ClientConfigurationRequest) returns (ClientConfigurationResponse);
  // Updates the configuration for a specific OPC client.
  rpc UpdateClientConfiguration(UpdateClientConfigurationRequest) returns (google.protobuf.Empty);
  // Retrieves the current operational status of a client.
  rpc GetClientStatus(ClientStatusRequest) returns (ClientStatusResponse);
}

// ---- Message Definitions ----

message ClientConfigurationRequest {
  string client_id = 1; // UUID of the client
}

message ClientConfigurationResponse {
  ClientConfiguration configuration = 1;
}

message UpdateClientConfigurationRequest {
  string client_id = 1; // UUID of the client
  ClientConfiguration configuration = 2;
}

message ClientStatusRequest {
  string client_id = 1; // UUID of the client
}

message ClientStatusResponse {
  string client_id = 1;
  string status = 2; // e.g., "Connected", "Disconnected", "Error"
  int64 connected_since_ticks = 3;
  int32 subscription_count = 4;
}

message ClientConfiguration {
  string id = 1;
  string name = 2;
  string endpoint_url = 3;
  string security_policy = 4;
  bool is_active = 5;
}


#### 3.2.2. `src/shared.kernel/Contracts/Grpc/ai_processing.proto`

-   **Description:** Defines the gRPC contract for the AI Processing Service.
-   **Specification (proto3 syntax):**

protobuf
syntax = "proto3";

package Contracts.Grpc;

// Service for AI-driven insights like predictive maintenance and anomaly detection.
service AiProcessingService {
  // Requests a maintenance prediction based on historical data.
  rpc GetMaintenancePrediction(MaintenancePredictionRequest) returns (MaintenancePredictionResponse);
  // Streams real-time data for anomaly detection and receives anomaly events back.
  rpc DetectAnomalies(stream AnomalyDetectionRequest) returns (stream AnomalyDetectionResponse);
}

// ---- Message Definitions ----

message DataPoint {
  string tag_identifier = 1;
  double value = 2;
  int64 timestamp_ticks = 3;
}

message MaintenancePredictionRequest {
  string model_id = 1;
  repeated DataPoint historical_data = 2;
}

message MaintenancePredictionResponse {
  int64 predicted_failure_time_ticks = 1;
  double confidence_score = 2;
  string details = 3;
}

message AnomalyDetectionRequest {
  DataPoint data_point = 1;
}

message AnomalyDetectionResponse {
  DataPoint data_point = 1;
  bool is_anomaly = 2;
  double anomaly_score = 3;
  string reason = 4;
}


### 3.3. Message Queue Contracts

These are immutable C# `record` types that define the schema for events published to the message bus (e.g., RabbitMQ, Kafka).

#### 3.3.1. `src/shared.kernel/Contracts/Dtos/OpcDataPointDto.cs`

-   **Description:** A fundamental DTO for a single OPC tag reading.
-   **Specification:**

csharp
namespace SharedKernel.Contracts.Dtos;

/// <summary>
/// Represents a single data point reading from an OPC server.
/// This is an immutable data transfer object.
/// </summary>
/// <param name="TagIdentifier">The unique identifier for the OPC tag.</param>
/// <param name="Value">The value of the tag, serialized as a string for universal compatibility.</param>
/// <param name="Quality">The quality status of the reading (e.g., 'Good', 'Bad').</param>
/// <param name="Timestamp">The timestamp of when the value was recorded by the server.</param>
public record OpcDataPointDto(
    string TagIdentifier,
    string Value,
    string Quality,
    DateTimeOffset Timestamp);


#### 3.3.2. `src/shared.kernel/Contracts/Messages/Events/OpcDataReceivedEvent.cs`

-   **Description:** DTO for an event carrying a batch of OPC data.
-   **Specification:**

csharp
using SharedKernel.Contracts.Dtos;

namespace SharedKernel.Contracts.Messages.Events;

/// <summary>
/// Represents an event published when a batch of OPC data is received.
/// </summary>
/// <param name="ClientId">The unique identifier of the OPC client that sourced the data.</param>
/// <param name="DataPoints">A collection of OPC data points in this batch.</param>
/// <param name="EventTimestamp">The timestamp when the event was created.</param>
public record OpcDataReceivedEvent(
    Guid ClientId,
    IReadOnlyList<OpcDataPointDto> DataPoints,
    DateTimeOffset EventTimestamp);


#### 3.3.3. `src/shared.kernel/Contracts/Messages/Events/AlarmTriggeredEvent.cs`

-   **Description:** DTO for an OPC A&C alarm event.
-   **Specification:**

csharp
namespace SharedKernel.Contracts.Messages.Events;

/// <summary>
/// Represents an event published when an alarm is triggered or its state changes.
/// </summary>
/// <param name="ClientId">The unique identifier of the OPC client that sourced the alarm.</param>
/// <param name="SourceNode">The OPC node that the alarm is associated with.</param>
/// <param name="EventType">The type of the event (e.g., 'HighHigh', 'DeviceFailure').</param>
/// <param name="Severity">The severity of the alarm, typically a numerical value.</param>
/// <param name="Message">The descriptive message associated with the alarm.</param>
/// <param name="Timestamp">The timestamp of the alarm occurrence.</param>
public record AlarmTriggeredEvent(
    Guid ClientId,
    string SourceNode,
    string EventType,
    int Severity,
    string Message,
    DateTimeOffset Timestamp);


### 3.4. Domain SeedWork

These components provide the foundational building blocks for implementing DDD in the microservices.

#### 3.4.1. `src/shared.kernel/SeedWork/Entity.cs`

-   **Description:** Base class for domain entities.
-   **Specification:**

csharp
namespace SharedKernel.SeedWork;

public abstract class Entity
{
    public virtual Guid Id { get; protected set; }

    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetRealType() != other.GetRealType())
            return false;
            
        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetRealType().ToString() + Id).GetHashCode();
    }

    private Type GetRealType()
    {
        Type type = GetType();
        if (type.ToString().Contains("Castle.Proxies."))
            return type.BaseType!;

        return type;
    }
}


#### 3.4.2. `src/shared.kernel/SeedWork/IDomainEvent.cs`

-   **Description:** Marker interface for domain events.
-   **Specification:**

csharp
namespace SharedKernel.SeedWork;

/// <summary>
/// Represents a marker interface for a domain event,
/// signifying something that happened in the domain that other parts
/// of the same or different bounded contexts might be interested in.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}


#### 3.4.3. `src/shared.kernel/SeedWork/AggregateRoot.cs`

-   **Description:** Base class for aggregate roots.
-   **Specification:**

csharp
namespace SharedKernel.SeedWork;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(Guid id) : base(id) { }
    
    protected AggregateRoot() { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}


### 3.5. Shared Domain Components

#### 3.5.1. `src/shared.kernel/Exceptions/DomainException.cs`

-   **Description:** Custom base exception for domain logic errors.
-   **Specification:**

csharp
namespace SharedKernel.Exceptions;

/// <summary>
/// Base exception for errors that occur within the domain layer,
/// representing a violation of a business rule or invariant.
/// </summary>
public class DomainException : Exception
{
    public DomainException() { }

    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}


#### 3.5.2. `src/shared.kernel/Domain/Enums/OpcStandard.cs`

-   **Description:** Enum for supported OPC standards.
-   **Specification:**

csharp
namespace SharedKernel.Domain.Enums;

/// <summary>
/// Defines the set of supported OPC communication standards.
/// </summary>
public enum OpcStandard
{
    /// <summary>
    /// OPC Data Access (Classic)
    /// </summary>
    DA,
    
    /// <summary>
    /// OPC Unified Architecture
    /// </summary>
    UA,
    
    /// <summary>
    /// OPC XML-DA
    /// </summary>
    XmlDa,
    
    /// <summary>
    /// OPC Historical Data Access
    /// </summary>
    Hda,
    
    /// <summary>
    /// OPC Alarms & Conditions
    /// </summary>
    Ac
}


### 3.6. Core Abstractions

#### 3.6.1. `src/shared.kernel/Abstractions/ICurrentUser.cs`

-   **Description:** Defines a contract for accessing the current user's context.
-   **Specification:**

csharp
namespace SharedKernel.Abstractions;

/// <summary>
/// Provides an abstraction to access the current user's information
/// from the execution context, decoupling services from HttpContext.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    Guid? UserId { get; }
    
    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the roles assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }
}


### 3.7. Reusable Utilities

#### 3.7.1. `src/shared.kernel/Utils/Guard.cs`

- **Description:** Provides static helper methods for validating method arguments and object states.
- **Specification:** A static class with methods to enforce preconditions.

csharp
using System.Runtime.CompilerServices;

namespace SharedKernel.Utils;

/// <summary>
/// Provides helper methods for argument and state validation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws an ArgumentNullException if the input argument is null.
    /// </summary>
    public static void AgainstNull(object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an ArgumentException if the input string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static void AgainstNullOrWhiteSpace(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("Argument cannot be null, empty, or whitespace.", paramName);
        }
    }
}


## 4. Testing Strategy

Unit tests shall be created for all public components within this library.
-   **DDD SeedWork:** Tests for the `Entity` base class will verify that `Equals` and `GetHashCode` work correctly based on the `Id`. Tests for `AggregateRoot` will verify that domain events are correctly added and cleared.
-   **DTOs and Enums:** Simple tests to ensure properties are correctly assigned and accessible.
-   **Exceptions:** Tests to confirm that custom exceptions can be thrown and caught correctly.
-   **Utilities:** Tests for the `Guard` class will verify that it throws the correct exceptions under failure conditions and does nothing under valid conditions.