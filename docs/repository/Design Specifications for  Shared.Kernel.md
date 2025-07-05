# Software Design Specification (SDS) for Shared.Kernel

## 1. Introduction

### 1.1. Purpose
This document provides the detailed software design specification for the `Opc.System.Shared.Kernel` library. This library is a foundational component, acting as a "Shared Kernel" in a distributed system. Its primary purpose is to define and provide a single, version-controlled source of truth for data models, communication contracts, and common domain abstractions used across multiple microservices and the core OPC client application. This enforces consistency, reduces code duplication, and decouples services by providing shared, stable interfaces.

### 1.2. Scope
The scope of this document is limited to the `Opc.System.Shared.Kernel` repository (`REPO-SAP-012`). It covers the design of:
- **Project Configuration:** The `.csproj` file setup.
- **gRPC Service Contracts:** Protocol Buffer (`.proto`) definitions for inter-service communication.
- **Messaging Contracts:** C# classes representing events for the event bus.
- **API Data Transfer Objects (DTOs):** Common DTOs used in RESTful API responses.
- **Domain-Driven Design (DDD) SeedWork:** Base classes and interfaces to support domain modeling.
- **Infrastructure Abstractions:** Core interfaces for infrastructure components like the event bus.

This library directly addresses requirement `REQ-SAP-005`: "Data models for inter-service communication, including message schemas for queues and contract definitions for APIs, will be clearly defined."

## 2. Overall Design & Architecture

The `Opc.System.Shared.Kernel` will be implemented as a **.NET Standard 2.1 library**. This target framework ensures maximum compatibility, allowing the library to be consumed by both the .NET 6+ based `CoreOpcClientService` and the .NET 8 based server-side microservices.

The architecture of this library is based on providing clear, immutable contracts. Key design principles include:
- **Immutability:** DTOs and event message classes will be designed as immutable records or classes with `init`-only properties to prevent unintended state changes after creation.
- **Explicit Contracts:** gRPC `.proto` files will serve as the strict, versionable contract for all synchronous RPC-style communication.
- **Abstraction:** Interfaces like `IEventBus` will abstract concrete infrastructure implementations, promoting loose coupling.

## 3. Project Configuration (`Opc.System.Shared.Kernel.csproj`)

The project file will be configured to support the library's goals.

- **Target Framework:** `netstandard2.1`
- **NuGet Package References:**
  - `Google.Protobuf`: Provides the core libraries for working with Protocol Buffers.
  - `Grpc.Tools`: Contains the `protoc` compiler and gRPC C# plugin, essential for generating C# code from `.proto` files. This should be marked with `PrivateAssets="all"`.
  - `Grpc.Core.Api`: Provides the core, non-implementation-specific gRPC APIs.
- **Build Configuration for gRPC:**
  An `<ItemGroup>` will be configured to identify all `.proto` files within the `Protos` directory. This group will instruct the build process to use the `Grpc.Tools` package to compile these contracts into C# code.

xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Opc.System.Shared.Kernel</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Google.Protobuf" Version="3.25.1" />
    <PackageReference Include="Grpc.Tools" Version="2.60.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Grpc.Core.Api" Version="2.60.0" />
  </ItemGroup>

  <ItemGroup>
    <Protobuf Include="Protos\*.proto" GrpcServices="Client" />
  </ItemGroup>

</Project>


## 4. Detailed Component Design

### 4.1. gRPC Service Contracts

#### 4.1.1. `Protos/common.proto`
This file defines message types that are reused across multiple gRPC services.

protobuf
syntax = "proto3";

package opc.system.shared.kernel.protos;

option csharp_namespace = "Opc.System.Shared.Kernel.Protos.Common";

// A wrapper for a standard GUID/UUID, represented as a string.
message GuidValue {
    string value = 1;
}

// Represents a request for a paginated list of resources.
message PagedRequest {
    int32 page_number = 1;
    int32 page_size = 2;
}

// A standard response message indicating the status of an operation.
message StatusResponse {
    bool success = 1;
    string error_message = 2;
}


#### 4.1.2. `Protos/management_api.proto`
This file defines the gRPC contract for the `ManagementApiService`, which is responsible for the centralized administration of OPC client instances.

protobuf
syntax = "proto3";

package opc.system.shared.kernel.protos;
import "Protos/common.proto";

option csharp_namespace = "Opc.System.Shared.Kernel.Protos.Management";

// Service for managing OPC client instances remotely.
service ManagementApiService {
    // Retrieves the current health status of a specific client.
    rpc GetClientStatus(GetClientStatusRequest) returns (ClientStatusResponse);
    // Updates the configuration of a specific client.
    rpc UpdateClientConfiguration(ClientConfiguration) returns (StatusResponse);
}

// Request to get a client's status.
message GetClientStatusRequest {
    GuidValue client_id = 1;
}

// Response containing a client's health and performance metrics.
message ClientStatusResponse {
    GuidValue client_id = 1;
    string status = 2; // e.g., "Connected", "Disconnected", "Error"
    int32 connected_servers = 3;
    int64 total_tags_monitored = 4;
    double cpu_usage_percent = 5;
    double memory_usage_mb = 6;
}

// Represents the full configuration for an OPC client instance.
message ClientConfiguration {
    GuidValue client_id = 1;
    string client_name = 2;
    repeated OpcServerConfig servers = 3;
}

// Configuration for a single OPC server connection.
message OpcServerConfig {
    GuidValue server_id = 1;
    string endpoint_url = 2;
    // Further details like security policy can be added here.
}


### 4.2. Messaging Contracts

#### 4.2.1. `Messaging/IIntegrationEvent.cs`
A marker interface to provide a common structure and type constraint for all events published to the event bus.

csharp
namespace Opc.System.Shared.Kernel.Messaging;

/// <summary>
/// Defines a base contract for integration events, ensuring essential metadata.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the unique identifier for the event instance.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the UTC date and time the event was created.
    /// </summary>
    public DateTimeOffset CreationDate { get; }
}


#### 4.2.2. `Messaging/Events/OpcDataReceivedEvent.cs`
The message contract for publishing batches of real-time OPC data.

csharp
namespace Opc.System.Shared.Kernel.Messaging.Events;

/// <summary>
/// Represents a batch of real-time data points received from an OPC client instance.
/// This event is published for asynchronous ingestion and processing.
/// </summary>
public record OpcDataReceivedEvent(
    Guid EventId,
    DateTimeOffset CreationDate,
    Guid ClientId,
    Guid ServerId,
    IReadOnlyList<DataPointDto> DataPoints) : IIntegrationEvent;

/// <summary>
/// DTO representing a single OPC tag data point.
/// </summary>
public record DataPointDto(
    string TagIdentifier,
    string Value,
    string Quality,
    DateTimeOffset Timestamp);


#### 4.2.3. `Messaging/Events/AlarmTriggeredEvent.cs`
The message contract for publishing OPC A&E (Alarms & Events).

csharp
namespace Opc.System.Shared.Kernel.Messaging.Events;

/// <summary>
/// Represents an alarm or condition event triggered by an OPC A&E server and captured by a client instance.
/// </summary>
public record AlarmTriggeredEvent(
    Guid EventId,
    DateTimeOffset CreationDate,
    Guid ClientId,
    string SourceNode,
    string Message,
    int Severity,
    bool Acknowledged,
    DateTimeOffset OccurrenceTime) : IIntegrationEvent;


### 4.3. API Data Transfer Objects (DTOs)

#### 4.3.1. `Contracts/Common/PagedResultDto.cs`
A generic DTO to standardize paginated responses from any RESTful API endpoint.

csharp
namespace Opc.System.Shared.Kernel.Contracts.Common;

/// <summary>
/// A generic DTO to encapsulate a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of the items in the list.</typeparam>
public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; }
    public long TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResultDto(IReadOnlyList<T> items, long totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}


### 4.4. Domain-Driven Design (DDD) SeedWork

#### 4.4.1. `Domain/SeedWork/ValueObject.cs`
An abstract base class to facilitate the implementation of the Value Object pattern from DDD.

csharp
namespace Opc.System.Shared.Kernel.Domain.SeedWork;

/// <summary>
/// Base class for Value Objects. Provides implementation for structural equality.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// When overridden in a derived class, returns all components of the value object
    /// that are used for equality comparison.
    /// </summary>
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate(1, (current, next) =>
            {
                unchecked
                {
                    return current * 23 + next;
                }
            });
    }

    public static bool operator ==(ValueObject? a, ValueObject? b)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(ValueObject? a, ValueObject? b)
    {
        return !(a == b);
    }
}


### 4.5. Infrastructure Abstractions

#### 4.5.1. `Infrastructure/Abstractions/IEventBus.cs`
An interface that abstracts the underlying message bus technology, providing a clean contract for publishing and subscribing to `IIntegrationEvent`s.

csharp
namespace Opc.System.Shared.Kernel.Infrastructure.Abstractions;

using Opc.System.Shared.Kernel.Messaging;

/// <summary>
/// Defines the contract for an event bus to enable event-driven communication between services.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the message bus.
    /// </summary>
    /// <param name="integrationEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to an integration event of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the integration event.</typeparam>
    /// <typeparam name="TH">The type of the event handler.</typeparam>
    /// <param name="cancellationToken">A token to cancel the subscription setup.</param>
    Task SubscribeAsync<T, TH>(CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>;
}

/// <summary>
/// Base handler interface for integration events.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent> where TIntegrationEvent : IIntegrationEvent
{
    Task Handle(TIntegrationEvent @event);
}
