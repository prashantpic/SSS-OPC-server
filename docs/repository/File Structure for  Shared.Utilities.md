# Specification

# 1. Files

- **Path:** src/Shared/Utilities/Logging/SerilogConfigurator.cs  
**Description:** Provides centralized configuration for the Serilog logging framework. Sets up sinks, enrichment, and minimum logging levels based on application configuration, ensuring consistent structured logging across all services.  
**Template:** C# Static Class Template  
**Dependency Level:** 1  
**Name:** SerilogConfigurator  
**Type:** Configurator  
**Relative Path:** Logging/SerilogConfigurator.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    - StrategyPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ConfigureLogging  
**Parameters:**
    
    - IHostBuilder hostBuilder
    
**Return Type:** IHostBuilder  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Structured Logging Setup
    - Configurable Log Sinks (Console, File)
    - Log Rotation and Retention Configuration
    
**Requirement Ids:**
    
    - REQ-6-004
    - REQ-6-005
    
**Purpose:** To centralize and standardize the setup of structured logging using Serilog for all applications in the ecosystem.  
**Logic Description:** This static class provides an extension method for IHostBuilder. The method reads logging configuration from 'appsettings.json', including sink types (Console, File), output templates, minimum log levels, and overrides. It configures the file sink for rolling-file behavior with specified retention policies (e.g., file size, number of files). It also adds standard enrichers like ThreadId, ProcessId, and application name.  
**Documentation:**
    
    - **Summary:** Configures and initializes the Serilog logger for an application. It is designed to be called during the application startup process to ensure consistent logging practices are applied system-wide.
    
**Namespace:** Opc.System.Shared.Utilities.Logging  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Logging/LogEventProperties.cs  
**Description:** Defines a set of standardized constant strings for custom property names used in structured logging. This ensures consistency in log analysis and querying across different services.  
**Template:** C# Static Class Template  
**Dependency Level:** 0  
**Name:** LogEventProperties  
**Type:** Constants  
**Relative Path:** Logging/LogEventProperties.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** ApplicationName  
**Type:** string  
**Attributes:** public|const  
    - **Name:** CorrelationId  
**Type:** string  
**Attributes:** public|const  
    - **Name:** UserId  
**Type:** string  
**Attributes:** public|const  
    - **Name:** OpcServerEndpoint  
**Type:** string  
**Attributes:** public|const  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Consistent Log Property Naming
    
**Requirement Ids:**
    
    - REQ-6-004
    
**Purpose:** To provide a single source of truth for custom property keys used in structured logs, preventing typos and inconsistencies.  
**Logic Description:** This is a simple static class containing public constant string fields. Each constant represents a key that will be used when enriching log events with contextual information. For example, a middleware can add a 'CorrelationId' to the logging context using the value from this class.  
**Documentation:**
    
    - **Summary:** A static container for standardized property names for structured logging. Using these constants ensures that all services use the same keys for the same type of information, simplifying log aggregation and analysis.
    
**Namespace:** Opc.System.Shared.Utilities.Logging  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Monitoring/OpenTelemetryConfigurator.cs  
**Description:** Provides centralized configuration for the OpenTelemetry framework, setting up distributed tracing and metrics collection. It configures sources, processors, and exporters for telemetry data.  
**Template:** C# Static Class Template  
**Dependency Level:** 1  
**Name:** OpenTelemetryConfigurator  
**Type:** Configurator  
**Relative Path:** Monitoring/OpenTelemetryConfigurator.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ConfigureOpenTelemetry  
**Parameters:**
    
    - IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Distributed Tracing Setup
    - Metrics Collection Setup
    - Configurable Telemetry Exporters (OTLP, Prometheus)
    
**Requirement Ids:**
    
    - REQ-6-005
    - REQ-6-006
    
**Purpose:** To standardize the setup of observability using OpenTelemetry, enabling distributed tracing and metrics across all microservices.  
**Logic Description:** This static class provides an extension method for IServiceCollection. It reads configuration to determine which exporters to enable (e.g., OTLP for traces, Prometheus for metrics). It sets up the TracerProvider with sources for ASP.NET Core, HttpClient, and custom sources defined in MonitoringConstants. It also sets up the MeterProvider with sources for runtime metrics and custom meters. The exporter endpoints are read from configuration.  
**Documentation:**
    
    - **Summary:** Configures and registers OpenTelemetry services for tracing and metrics. This method should be called in the application's startup code to enable observability features.
    
**Namespace:** Opc.System.Shared.Utilities.Monitoring  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Monitoring/SystemMetrics.cs  
**Description:** Defines and exposes the core OpenTelemetry Meter and ActivitySource for the system. It also defines specific metric instruments (counters, histograms) for tracking key performance indicators (KPIs) as required.  
**Template:** C# Static Class Template  
**Dependency Level:** 1  
**Name:** SystemMetrics  
**Type:** MetricsProvider  
**Relative Path:** Monitoring/SystemMetrics.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** ActivitySource  
**Type:** ActivitySource  
**Attributes:** public|static|readonly  
    - **Name:** Meter  
**Type:** Meter  
**Attributes:** public|static|readonly  
    - **Name:** ApiRequestCounter  
**Type:** Counter<long>  
**Attributes:** public|static|readonly  
    - **Name:** ApiRequestDuration  
**Type:** Histogram<double>  
**Attributes:** public|static|readonly  
    - **Name:** OpcWriteOperationsCounter  
**Type:** Counter<long>  
**Attributes:** public|static|readonly  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Centralized KPI Metric Definitions
    - ActivitySource for Distributed Tracing
    - Meter for Metrics
    
**Requirement Ids:**
    
    - REQ-6-004
    - REQ-6-006
    
**Purpose:** To provide a single, consistent source for creating traces and recording business-critical metrics throughout the application.  
**Logic Description:** This static class initializes a single ActivitySource and a single Meter using names from MonitoringConstants. It then uses the Meter to create specific, publicly accessible, static instruments like Counters and Histograms. Other services will reference these static instruments (e.g., SystemMetrics.ApiRequestCounter.Add(1)) to record measurements without needing to manage their own Meter instances.  
**Documentation:**
    
    - **Summary:** Provides global, statically accessible OpenTelemetry ActivitySource and Meter instances, along with predefined instruments for common system KPIs. This ensures all parts of the application use the same source for traces and metrics.
    
**Namespace:** Opc.System.Shared.Utilities.Monitoring  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Monitoring/MonitoringConstants.cs  
**Description:** Defines constant strings for monitoring, including names for Meters, ActivitySources, and common tag/attribute keys. This ensures consistency across the codebase when creating traces and metrics.  
**Template:** C# Static Class Template  
**Dependency Level:** 0  
**Name:** MonitoringConstants  
**Type:** Constants  
**Relative Path:** Monitoring/MonitoringConstants.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** ActivitySourceName  
**Type:** string  
**Attributes:** public|const  
    - **Name:** MeterName  
**Type:** string  
**Attributes:** public|const  
    - **Name:** ApiEndpointTag  
**Type:** string  
**Attributes:** public|const  
    - **Name:** OpcOperationTag  
**Type:** string  
**Attributes:** public|const  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Consistent Naming for Observability
    
**Requirement Ids:**
    
    - REQ-6-004
    - REQ-6-006
    
**Purpose:** To prevent magic strings and enforce consistent naming for all observability components and attributes.  
**Logic Description:** A simple static class containing public constant strings. These constants are used to initialize the ActivitySource and Meter in SystemMetrics and are also used by consuming services when adding tags to activities or attributes to metrics.  
**Documentation:**
    
    - **Summary:** A static container for standardized names and keys used in OpenTelemetry tracing and metrics. This ensures that all telemetry data is consistently named and tagged, making it easier to correlate, query, and visualize.
    
**Namespace:** Opc.System.Shared.Utilities.Monitoring  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Configuration/ConfigurationHelper.cs  
**Description:** A helper class to provide a standardized way of building the application's configuration from multiple sources like JSON files, environment variables, and command-line arguments.  
**Template:** C# Static Class Template  
**Dependency Level:** 0  
**Name:** ConfigurationHelper  
**Type:** Helper  
**Relative Path:** Configuration/ConfigurationHelper.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** BuildConfiguration  
**Parameters:**
    
    - string[] args
    
**Return Type:** IConfiguration  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Standardized Configuration Building
    
**Requirement Ids:**
    
    
**Purpose:** To ensure all applications in the system load their configuration in a consistent and predictable order of precedence.  
**Logic Description:** The BuildConfiguration method creates a new ConfigurationBuilder. It adds various configuration sources in a specific order: first 'appsettings.json', then the environment-specific 'appsettings.{Environment}.json', then environment variables, and finally command-line arguments. This establishes a clear hierarchy for configuration overrides.  
**Documentation:**
    
    - **Summary:** Provides a static method to create a standard IConfiguration object for an application. It aggregates settings from JSON files, environment variables, and command-line arguments.
    
**Namespace:** Opc.System.Shared.Utilities.Configuration  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Security/CryptographyHelper.cs  
**Description:** Provides common cryptographic utility functions, such as hashing and random number generation, in a standardized and secure manner.  
**Template:** C# Static Class Template  
**Dependency Level:** 0  
**Name:** CryptographyHelper  
**Type:** Helper  
**Relative Path:** Security/CryptographyHelper.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ComputeSha256Hash  
**Parameters:**
    
    - string input
    
**Return Type:** string  
**Attributes:** public static  
    - **Name:** GenerateSecureRandomBytes  
**Parameters:**
    
    - int byteCount
    
**Return Type:** byte[]  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Secure Hashing
    - Cryptographically Strong Random Number Generation
    
**Requirement Ids:**
    
    
**Purpose:** To offer a centralized, easy-to-use API for basic cryptographic operations, ensuring that best practices are followed.  
**Logic Description:** The ComputeSha256Hash method takes a string, converts it to bytes, computes the SHA256 hash, and returns it as a hex string. The GenerateSecureRandomBytes method uses the 'System.Security.Cryptography.RandomNumberGenerator' to produce a cryptographically secure array of random bytes, suitable for generating salts or keys.  
**Documentation:**
    
    - **Summary:** A utility class for performing common cryptographic tasks. Includes methods for creating SHA256 hashes and generating secure random data.
    
**Namespace:** Opc.System.Shared.Utilities.Security  
**Metadata:**
    
    - **Category:** CrossCutting
    
- **Path:** src/Shared/Utilities/Exceptions/UtilitiesException.cs  
**Description:** A custom base exception class for all exceptions originating from the Shared.Utilities library. This allows consumers to catch a single exception type for errors from this library.  
**Template:** C# Class Template  
**Dependency Level:** 0  
**Name:** UtilitiesException  
**Type:** Exception  
**Relative Path:** Exceptions/UtilitiesException.cs  
**Repository Id:** REPO-SAP-013  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** UtilitiesException  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UtilitiesException  
**Parameters:**
    
    - string message
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** UtilitiesException  
**Parameters:**
    
    - string message
    - Exception innerException
    
**Return Type:** void  
**Attributes:** public  
    
**Implemented Features:**
    
    - Custom Base Exception
    
**Requirement Ids:**
    
    
**Purpose:** To create a specific exception type for this library, enabling more precise error handling by consuming services.  
**Logic Description:** This is a standard custom exception class that inherits from 'System.Exception'. It provides the three common constructors for creating an exception with no message, with a message, and with a message and an inner exception.  
**Documentation:**
    
    - **Summary:** The base exception for errors that occur within the Shared.Utilities library. It allows for differentiated error handling.
    
**Namespace:** Opc.System.Shared.Utilities.Exceptions  
**Metadata:**
    
    - **Category:** CrossCutting
    


---

# 2. Configuration

- **Feature Toggles:**
  
  
- **Database Configs:**
  
  


---

