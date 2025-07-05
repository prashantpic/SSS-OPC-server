# Specification

# 1. Files

- **Path:** src/Gateways/ApiGateway/ApiGateway.csproj  
**Description:** The .NET 8 project file for the API Gateway. It defines the project type, target framework, and lists all necessary NuGet package dependencies, including Yarp.ReverseProxy for routing, Microsoft.AspNetCore.Authentication.JwtBearer for security, and packages for logging and observability like Serilog and OpenTelemetry.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** ApiGateway  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Dependency Management
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** To manage project dependencies and build configurations for the API Gateway.  
**Logic Description:** This file will contain PackageReference items for Yarp.ReverseProxy, Serilog.AspNetCore, OpenTelemetry.Extensions.Hosting, Microsoft.AspNetCore.Authentication.JwtBearer, and other core ASP.NET libraries. It targets the net8.0 framework.  
**Documentation:**
    
    - **Summary:** Defines the API Gateway project's dependencies and framework settings.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Gateways/ApiGateway/Program.cs  
**Description:** The main entry point for the API Gateway application. This file configures and launches the Kestrel web server, sets up the ASP.NET Core dependency injection container, and defines the request processing pipeline.  
**Template:** C# Application EntryPoint  
**Dependency Level:** 1  
**Name:** Program  
**Type:** Main  
**Relative Path:** Program.cs  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - APIGateway
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Service Registration
    - Middleware Configuration
    - Reverse Proxy Initialization
    - JWT Authentication Setup
    - Authorization Policy Setup
    - Structured Logging and Tracing Setup
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** To bootstrap the API Gateway, configure all services and middleware, and start the application.  
**Logic Description:** The logic will create a WebApplicationBuilder, configure logging with Serilog, and set up OpenTelemetry for tracing. It will add services for authentication (AddAuthentication with AddJwtBearer, configured from appsettings) and authorization. Crucially, it will register the YARP reverse proxy service (AddReverseProxy) and load its configuration from the appsettings file. The middleware pipeline will be configured to use authentication, authorization, and then map the reverse proxy endpoints.  
**Documentation:**
    
    - **Summary:** Initializes and configures the API Gateway application. Sets up JWT authentication, YARP reverse proxy, logging, and the middleware pipeline.
    
**Namespace:** Opc.System.ApiGateway  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Gateways/ApiGateway/appsettings.json  
**Description:** The primary configuration file for the API Gateway. It contains routing rules for the YARP reverse proxy, JWT settings for authentication, logging configurations, and CORS policies. This file is central to the gateway's operation, defining how it routes traffic to downstream microservices.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** appsettings.json  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - ExternalConfigurationStore
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Route Definition
    - Cluster Definition
    - Authentication Configuration
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** To provide a declarative, external configuration for all aspects of the API Gateway, including routing and security.  
**Logic Description:** This JSON file will have a top-level 'ReverseProxy' object containing 'Routes' and 'Clusters' arrays. 'Routes' will define the public endpoints, matching paths and methods, and specifying the 'ClusterId' to forward to, along with the required 'AuthorizationPolicy'. 'Clusters' will define the downstream services by 'ClusterId', listing their destination addresses. A separate 'JwtSettings' object will contain the Authority, Audience, and other parameters for token validation. Logging and AllowedHosts will also be configured.  
**Documentation:**
    
    - **Summary:** Contains all routing, cluster, and security configuration for the YARP-based API Gateway.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Gateways/ApiGateway/appsettings.Development.json  
**Description:** Environment-specific configuration file that overrides settings in appsettings.json for the development environment. It is primarily used to specify local addresses for downstream microservices.  
**Template:** JSON Configuration  
**Dependency Level:** 1  
**Name:** appsettings.Development  
**Type:** Configuration  
**Relative Path:** appsettings.Development.json  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - ExternalConfigurationStore
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Environment-specific Overrides
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** To enable local development by pointing the gateway to locally running instances of backend services.  
**Logic Description:** This file will contain a 'ReverseProxy' object with a 'Clusters' array. The clusters defined here will override the destination addresses from the main appsettings.json, pointing to localhost URLs (e.g., https://localhost:7001, https://localhost:7002) for the various backend microservices. This allows developers to run the entire system on their local machine.  
**Documentation:**
    
    - **Summary:** Overrides upstream cluster addresses for local development environments.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Gateways/ApiGateway/Dockerfile  
**Description:** A multi-stage Dockerfile used to build a container image for the API Gateway. It defines the steps to compile the .NET application, publish the release artifacts, and create a final, optimized runtime image.  
**Template:** Dockerfile  
**Dependency Level:** 1  
**Name:** Dockerfile  
**Type:** Build  
**Relative Path:** Dockerfile  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Containerization
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** To create a standardized, portable, and production-ready container for deploying the API Gateway.  
**Logic Description:** The Dockerfile will use a multi-stage build approach. The first stage ('build') will use the .NET SDK image to restore dependencies, build the project, and publish it. The second stage ('final') will start from the smaller ASP.NET runtime image, copy the published artifacts from the 'build' stage, expose port 80/443, and set the entry point to run the ApiGateway.dll.  
**Documentation:**
    
    - **Summary:** Defines the instructions for building a production-ready Docker container for the API Gateway application.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Deployment
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableResponseCaching
  - EnableCustomRequestLogging
  
- **Database Configs:**
  
  


---

