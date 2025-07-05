# Specification

# 1. Files

- **Path:** gateways/api-gateway/gateways.api-gateway.csproj  
**Description:** The C# project file for the API Gateway. It defines the target framework (.NET 8), project dependencies including YARP (Yet Another Reverse Proxy) and JWT Bearer authentication libraries, and other project-level settings.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** gateways.api-gateway  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - APIGateway
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Project Dependencies
    - Framework Configuration
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** Defines the project structure, metadata, and dependencies required to build and run the API Gateway application.  
**Logic Description:** This XML file will contain ItemGroup sections specifying PackageReference entries for Yarp.ReverseProxy and Microsoft.AspNetCore.Authentication.JwtBearer. It will also define the TargetFramework as net8.0 and enable Nullable contexts.  
**Documentation:**
    
    - **Summary:** The .csproj file is the cornerstone of the .NET project, managing its dependencies and build configurations. It ensures that all necessary libraries, like YARP for routing and JWT libraries for security, are included during compilation.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** gateways/api-gateway/appsettings.json  
**Description:** Core configuration file for the API Gateway. It contains the YARP ReverseProxy configuration, defining all routes and clusters. This file drives the gateway's primary routing logic in a declarative way, mapping incoming requests to downstream microservices.  
**Template:** JSON Configuration  
**Dependency Level:** 1  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - APIGateway
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - API Routing Configuration
    - Downstream Service Configuration
    - Authentication Policy Mapping
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** Provides a centralized and declarative mechanism for defining the gateway's routing and load balancing behavior, decoupling the infrastructure from the application code.  
**Logic Description:** This file will contain a top-level 'ReverseProxy' object. Inside, a 'Routes' object will map path prefixes (e.g., '/api/management/{**catch-all}') to a 'ClusterId' and apply an 'AuthorizationPolicy'. A 'Clusters' object will define each 'ClusterId' with a list of 'Destinations', specifying the addresses of the downstream microservices. It will also contain JWT settings like Authority and Audience for the authentication handler.  
**Documentation:**
    
    - **Summary:** This JSON file defines the core behavior of the API Gateway. It lists all the routes the gateway exposes and maps them to clusters of backend services. It also specifies security policies like JWT authentication for each route.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** gateways/api-gateway/appsettings.Development.json  
**Description:** Environment-specific configuration for development. This file overrides settings from appsettings.json, typically providing local URLs for downstream services (e.g., 'http://localhost:5001') and potentially more verbose logging levels.  
**Template:** JSON Configuration  
**Dependency Level:** 2  
**Name:** appsettings.Development  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    - APIGateway
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Local Development Configuration
    
**Requirement Ids:**
    
    - REQ-SAP-002
    
**Purpose:** Allows developers to run the gateway and connect it to other services running locally without modifying the base production configuration.  
**Logic Description:** This file will override the 'Destinations' addresses within the 'Clusters' section of the 'ReverseProxy' configuration to point to localhost URLs. It may also adjust 'Logging' levels to 'Debug' or 'Information' for easier local troubleshooting.  
**Documentation:**
    
    - **Summary:** Contains configuration overrides for the local development environment. This allows the gateway to connect to microservices running on a developer's machine.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** gateways/api-gateway/src/Program.cs  
**Description:** The main entry point of the API Gateway application. This file is responsible for bootstrapping the ASP.NET Core host, configuring services for dependency injection, and defining the HTTP request processing pipeline.  
**Template:** C# Program  
**Dependency Level:** 3  
**Name:** Program  
**Type:** ApplicationEntry  
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
    
    - Service Configuration
    - Middleware Pipeline Configuration
    - Application Startup
    
**Requirement Ids:**
    
    - REQ-SAP-002
    - REQ-3-010
    
**Purpose:** Initializes and configures all necessary components of the API Gateway, including YARP routing and JWT authentication, and starts the web server.  
**Logic Description:** The Main method will create a WebApplicationBuilder. It will call extension methods to configure services, such as adding the reverse proxy and loading its configuration from appsettings.json. It will also call an extension method to configure JWT Bearer authentication and authorization policies. The request pipeline will be configured to use routing, authentication, authorization, and finally, map the reverse proxy to handle incoming requests. Custom middleware for logging or correlation IDs would also be added here.  
**Documentation:**
    
    - **Summary:** This file starts the API Gateway application. It sets up all required services, including the YARP reverse proxy and security handlers, and defines the order in which incoming HTTP requests are processed.
    
**Namespace:** Gateways.Api  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** gateways/api-gateway/src/Extensions/AuthConfigurationExtensions.cs  
**Description:** An extension class to cleanly encapsulate the setup of authentication and authorization services, keeping the Program.cs file tidy. It centralizes all security-related service configurations.  
**Template:** C# Extension Class  
**Dependency Level:** 2  
**Name:** AuthConfigurationExtensions  
**Type:** Extension  
**Relative Path:** Extensions/AuthConfigurationExtensions.cs  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddGatewayAuthentication  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public static  
    
**Implemented Features:**
    
    - JWT Bearer Authentication
    - Authorization Policy Configuration
    
**Requirement Ids:**
    
    - REQ-3-010
    
**Purpose:** To provide a single, reusable method for configuring JWT-based authentication and authorization policies for the gateway, directly addressing security requirements.  
**Logic Description:** The 'AddGatewayAuthentication' method will read JWT settings (Authority, Audience) from the IConfiguration object. It will call 'services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)' to configure the handler. It will then call 'services.AddAuthorization(...)' to define a default policy that requires an authenticated user, which is what YARP will use for routes marked with an authorization policy.  
**Documentation:**
    
    - **Summary:** This class contains extension methods for setting up security. It configures how the gateway validates JSON Web Tokens (JWTs) received from clients to ensure they are authentic and not tampered with, and sets up the authorization policies.
    
**Namespace:** Gateways.Api.Extensions  
**Metadata:**
    
    - **Category:** Security
    
- **Path:** gateways/api-gateway/src/Transforms/CustomTransformProvider.cs  
**Description:** A custom YARP transform provider. This class can be used to dynamically add transformations to requests, such as adding a correlation ID header to track a request across multiple services, or transforming claims from the JWT into request headers for downstream services.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** CustomTransformProvider  
**Type:** Service  
**Relative Path:** Transforms/CustomTransformProvider.cs  
**Repository Id:** REPO-SAP-002  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Apply  
**Parameters:**
    
    - TransformBuilderContext context
    
**Return Type:** void  
**Attributes:** public override  
    
**Implemented Features:**
    
    - Request Transformation
    
**Requirement Ids:**
    
    
**Purpose:** To provide a hook into the YARP pipeline for programmatically modifying requests before they are sent to downstream services, enabling patterns like distributed tracing.  
**Logic Description:** This class will inherit from 'ITransformProvider'. The 'Apply' method will be implemented to check if a specific transform is configured for the route. For example, if 'RequestHeader' transform for 'X-Correlation-Id' is requested, it will add logic to either preserve an existing correlation ID or generate a new one. It will be registered as a singleton service in Program.cs and added to YARP via '.AddTransforms<CustomTransformProvider>()'.  
**Documentation:**
    
    - **Summary:** Provides custom logic to modify HTTP requests as they pass through the gateway. This can be used to add or remove headers, change the request path, or inject information for downstream services.
    
**Namespace:** Gateways.Api.Transforms  
**Metadata:**
    
    - **Category:** Infrastructure
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - enableCustomTransforms
  - enableDownstreamHealthChecks
  
- **Database Configs:**
  
  


---

