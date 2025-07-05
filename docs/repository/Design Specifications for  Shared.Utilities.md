# Software Design Specification (SDS) for Shared.Utilities

## 1. Introduction

### 1.1 Purpose
This document provides the detailed software design for the `Opc.System.Shared.Utilities` library. This library is a foundational component providing cross-cutting concerns for all services within the solution. Its purpose is to standardize and centralize logging, monitoring/observability, configuration, and common security utilities, ensuring consistency, maintainability, and adherence to best practices across the entire system.

### 1.2 Scope
The scope of this library includes:
-   **Structured Logging:** A centralized and configurable setup for Serilog.
-   **Observability:** A centralized and configurable setup for OpenTelemetry, covering distributed tracing and metrics.
-   **Configuration:** A helper to build application configuration in a standardized way.
-   **Security Utilities:** Basic, secure cryptographic helper functions.
-   **Exception Handling:** A custom base exception for the library.

This library is designed as a .NET Standard library to ensure maximum compatibility with all other .NET 8 projects in the solution.

### 1.3 Requirements Mapping
This library directly addresses the following system requirements:
-   **REQ-6-004:** Comprehensive structured logging.
-   **REQ-6-005:** Configurable log retention, KPI alerting thresholds (via observability integration), and integration with monitoring platforms.
-   **REQ-6-006:** Implementation of distributed tracing.

## 2. Architectural Design

The `Shared.Utilities` library follows a layered, utility-focused design. It exposes functionality primarily through static classes and extension methods, making it simple for consuming services to integrate during their startup and runtime.

-   **Consumption Model:** Services will typically call configuration methods from this library in their `Program.cs` file.
-   **Configuration-Driven:** The behavior of the logging and monitoring components is driven by the application's `IConfiguration` (e.g., `appsettings.json`), allowing each service to tailor its settings without code changes.
-   **Decoupling:** The library provides the tools for observability but does not mandate a specific backend. It uses standard exporters (like OTLP and Prometheus) to decouple from the final observability platform (e.g., Jaeger, Grafana, Splunk).

## 3. Component Design

This section details the design of each class within the `Opc.System.Shared.Utilities` library.

### 3.1 Logging Components

#### 3.1.1 `Logging/SerilogConfigurator.cs`
-   **Purpose:** Centralizes the configuration of Serilog for all applications.
-   **Class:** `public static class SerilogConfigurator`
-   **Method:** `public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)`
-   **Design Details:**
    -   This extension method for `IHostBuilder` ensures it's called early in the host-building process.
    -   It will use the `UseSerilog` extension method.
    -   The logger configuration will be built within the lambda provided to `UseSerilog`.
    -   It will call `loggerConfiguration.ReadFrom.Configuration(context.Configuration)` to load the bulk of the configuration from `appsettings.json`. This allows for environment-specific overrides for sinks, log levels, etc.
    -   It will enrich the logs by default using `.Enrich.FromLogContext()`, `.Enrich.WithMachineName()`, `.Enrich.WithThreadId()`, and `.Enrich.WithProcessId()`.
    -   It will configure a default console sink for development environments.
    -   The `appsettings.json` configuration it reads will define sinks (e.g., Console, File), output templates, and minimum level settings as per `REQ-6-005`.
    -   The File sink configuration will specify a path, rolling interval (e.g., `Day`), and retention policy (`retainedFileCountLimit`).

#### 3.1.2 `Logging/LogEventProperties.cs`
-   **Purpose:** Provides a single source of truth for custom property keys in structured logs.
-   **Class:** `public static class LogEventProperties`
-   **Design Details:**
    -   A static class containing `public const string` members. This avoids "magic strings" in the codebase and ensures consistency for log querying.
    -   **Members:**
        -   `public const string ApplicationName = "ApplicationName";`
        -   `public const string CorrelationId = "CorrelationId";`
        -   `public const string UserId = "UserId";`
        -   `public const string OpcServerEndpoint = "OpcServerEndpoint";`
        -   `public const string TenantId = "TenantId";`

### 3.2 Monitoring Components

#### 3.2.1 `Monitoring/OpenTelemetryConfigurator.cs`
-   **Purpose:** Centralizes the configuration of OpenTelemetry for distributed tracing and metrics.
-   **Class:** `public static class OpenTelemetryConfigurator`
-   **Method:** `public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)`
-   **Design Details:**
    -   This extension method for `IServiceCollection` integrates with the dependency injection container.
    -   It calls `services.AddOpenTelemetry()`.
    -   **Resource Building:** A `ResourceBuilder` will be created and configured with the service name, which should be read from the `IConfiguration`.
    -   **Tracing (`.WithTracing`)**:
        -   It will associate the configured `ResourceBuilder`.
        -   It will add sources using `AddSource(SystemMetrics.ActivitySource.Name)`.
        -   It will add standard instrumentation sources: `AddAspNetCoreInstrumentation()`, `AddHttpClientInstrumentation()`, and `AddEntityFrameworkCoreInstrumentation()`.
        -   It will conditionally add exporters based on configuration. For example, if `Observability:OtlpExporterEndpoint` is present, it will call `AddOtlpExporter()`. A console exporter will be added for development convenience.
    -   **Metrics (`.WithMetrics`)**:
        -   It will associate the configured `ResourceBuilder`.
        -   It will add meters using `AddMeter(SystemMetrics.Meter.Name)`.
        -   It will add standard instrumentation meters: `AddAspNetCoreInstrumentation()`, `AddHttpClientInstrumentation()`, and `AddRuntimeInstrumentation()`.
        -   It will conditionally add exporters based on configuration. `AddPrometheusExporter()` will be configured if enabled. `AddOtlpExporter()` will be configured if the endpoint is specified.

#### 3.2.2 `Monitoring/SystemMetrics.cs`
-   **Purpose:** Provides global, statically accessible instances of `ActivitySource` and `Meter`, and defines core metric instruments.
-   **Class:** `public static class SystemMetrics`
-   **Design Details:**
    -   **Static Members:**
        -   `public static readonly ActivitySource ActivitySource = new ActivitySource(MonitoringConstants.ActivitySourceName);` - Provides the single source for creating activities (spans) for distributed tracing.
        -   `public static readonly Meter Meter = new Meter(MonitoringConstants.MeterName);` - Provides the single source for creating metric instruments.
    -   **Metric Instruments (examples):**
        -   `public static readonly Counter<long> ApiRequestCounter = Meter.CreateCounter<long>("api_requests_total", "requests", "Total number of API requests.");`
        -   `public static readonly Histogram<double> ApiRequestDuration = Meter.CreateHistogram<double>("api_requests_duration_ms", "ms", "Duration of API requests in milliseconds.");`
        -   `public static readonly Counter<long> OpcWriteOperationsCounter = Meter.CreateCounter<long>("opc_write_ops_total", "operations", "Total number of OPC write operations performed.");`
    -   This design ensures that any part of the application can create a trace or record a metric using a consistent source (e.g., `using var activity = SystemMetrics.ActivitySource.StartActivity("MyOperation");`).

#### 3.2.3 `Monitoring/MonitoringConstants.cs`
-   **Purpose:** Provides a single source of truth for names and keys related to observability.
-   **Class:** `public static class MonitoringConstants`
-   **Design Details:**
    -   A static class containing `public const string` members.
    -   **Members:**
        -   `public const string ActivitySourceName = "Opc.System";`
        -   `public const string MeterName = "Opc.System.Metrics";`
        -   `public const string ApiEndpointTag = "api.endpoint";`
        -   `public const string OpcOperationTag = "opc.operation";`
        -   `public const string OpcStatusTag = "opc.status_code";`

### 3.3 Configuration Components

#### 3.3.1 `Configuration/ConfigurationHelper.cs`
-   **Purpose:** Ensures all applications build their configuration stack consistently.
-   **Class:** `public static class ConfigurationHelper`
-   **Method:** `public static IConfiguration BuildConfiguration(string[] args)`
-   **Design Details:**
    -   The method creates a `ConfigurationBuilder`.
    -   It adds sources in the following order of precedence (later sources override earlier ones):
        1.  `AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)`
        2.  `AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)`
        3.  `AddEnvironmentVariables()`
        4.  `AddCommandLine(args)`
    -   It returns the result of `builder.Build()`.

### 3.4 Security Components

#### 3.4.1 `Security/CryptographyHelper.cs`
-   **Purpose:** Provides centralized, secure implementations of common cryptographic functions.
-   **Class:** `public static class CryptographyHelper`
-   **Design Details:**
    -   **Method:** `public static string ComputeSha256Hash(string input)`
        -   Validates that input is not null.
        -   Uses `System.Security.Cryptography.SHA256.HashData()` on the UTF-8 bytes of the input string.
        -   Returns the resulting byte array as a lowercase hexadecimal string.
    -   **Method:** `public static byte[] GenerateSecureRandomBytes(int byteCount)`
        -   Validates that `byteCount` is positive.
        -   Uses `System.Security.Cryptography.RandomNumberGenerator.GetBytes(byteCount)` to fill a new byte array with cryptographically strong random values.
        -   Returns the byte array.

### 3.5 Exception Handling Components

#### 3.5.1 `Exceptions/UtilitiesException.cs`
-   **Purpose:** To provide a specific, catchable exception type for errors originating within this library.
-   **Class:** `public class UtilitiesException : Exception`
-   **Design Details:**
    -   The class inherits from `System.Exception`.
    -   It implements the three standard exception constructors:
        -   `public UtilitiesException()`
        -   `public UtilitiesException(string message)`
        -   `public UtilitiesException(string message, Exception innerException)`

## 4. Configuration Model

To support the configurable nature of this library, consumer applications should have the following structure in their `appsettings.json`.

json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
  },
  "Observability": {
    "ServiceName": "MyApplication.Api",
    "OtlpExporterEndpoint": "http://localhost:4317",
    "EnablePrometheusExporter": true
  }
}


## 5. Usage Example

A consuming service (`Program.cs`) would integrate the utilities as follows:

csharp
// 1. Build configuration using the helper
var configuration = ConfigurationHelper.BuildConfiguration(args);

var builder = WebApplication.CreateBuilder(args);

// 2. Configure Serilog using the host builder
builder.Host.ConfigureSerilog();

// 3. Configure OpenTelemetry using the service collection
builder.Services.ConfigureOpenTelemetry(builder.Configuration);

// ... other service configurations

var app = builder.Build();

// ... application pipeline

app.Run();
