# Software Design Specification (SDS) for API Gateway

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the `gateways.api-gateway` service. This service acts as the primary entry point for all external and UI-driven HTTP requests into the system's microservices architecture. It is responsible for request routing, security enforcement, and centralizing cross-cutting concerns.

### 1.2. Scope
The scope of this document is limited to the design and implementation of the API Gateway. This includes its configuration for routing, authentication, custom transformations, and logging. It relies on downstream services (e.g., Auth, Management, AI, Reporting) being available at configurable endpoints but does not cover their internal design.

### 1.3. Technology Stack
- **Framework:** ASP.NET Core on .NET 8
- **Core Technology:** YARP (Yet Another Reverse Proxy)
- **Language:** C#
- **Authentication:** JWT Bearer Token

## 2. System Overview and Architecture

The API Gateway is a reverse proxy built using ASP.NET Core and YARP. It follows the **API Gateway pattern**, serving as a facade for the backend microservices. This design decouples clients from the internal service topology, enhances security by providing a single point of authentication and authorization, and simplifies the client-side consumption of APIs.

### 2.1. Core Responsibilities
- **Request Routing:** Dynamically route incoming requests to the appropriate downstream microservice based on path, headers, or other criteria defined in the configuration.
- **Authentication & Authorization (Security Enforcement):** Intercept all incoming requests to validate JWT Bearer tokens. Reject unauthenticated or unauthorized requests before they reach the internal network. This directly implements requirement **REQ-3-010**.
- **Centralized Cross-Cutting Concerns:** Provide a single point for implementing rate limiting, request logging, and adding correlation IDs for distributed tracing.
- **API Aggregation (Future):** While not in the initial scope, the architecture allows for future implementation of endpoints that aggregate responses from multiple downstream services.
- **SSL Termination:** Handle HTTPS traffic, decrypting it and forwarding requests to backend services over the internal network (typically HTTP for performance, within a secure network boundary like a Kubernetes cluster).

## 3. Core Components and Implementation Details

### 3.1. Project File (`gateways.api-gateway.csproj`)
This file defines the project's dependencies and target framework.

**XML Structure:**
xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Gateways.Api</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Yarp.ReverseProxy" Version="2.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.x" />
  </ItemGroup>

</Project>

**Notes:**
- `Yarp.ReverseProxy`: The core dependency for all gateway functionality.
- `Microsoft.AspNetCore.Authentication.JwtBearer`: The standard library for validating JWTs in ASP.NET Core.

### 3.2. Configuration (`appsettings.json`)
This file contains the declarative configuration for YARP's routing engine and the application's security settings.

**Structure:**
json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Authority": "https://auth-service.yourdomain.com", // URL of the Auth Service
    "Audience": "api-gateway"
  },
  "ReverseProxy": {
    "Routes": {
      "management-route": {
        "ClusterId": "management-cluster",
        "Match": {
          "Path": "/api/management/{**catch-all}"
        },
        "AuthorizationPolicy": "default"
      },
      "ai-route": {
        "ClusterId": "ai-cluster",
        "Match": {
          "Path": "/api/ai/{**catch-all}"
        },
        "AuthorizationPolicy": "default"
      }
      // ... other routes for Reporting, Data, etc.
    },
    "Clusters": {
      "management-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://management-service" // Service name in Kubernetes/Docker
          }
        }
      },
      "ai-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://ai-service"
          }
        }
      }
      // ... other clusters
    }
  }
}

**Notes:**
- The `Jwt` section provides parameters for the JWT validation handler.
- The `ReverseProxy` section is read by YARP.
- `Routes` define the public-facing endpoints of the gateway. Each route maps a path to a `ClusterId` and specifies an `AuthorizationPolicy`.
- `Clusters` define logical groups of backend services. `Destinations` contain the actual addresses of the downstream microservices.

For local development, `appsettings.Development.json` will override the destination addresses to point to localhost, for example: `"Address": "http://localhost:5001"`.

### 3.3. Application Entry Point (`src/Program.cs`)
This file bootstraps the application, configures services, and defines the request pipeline.

csharp
using Gateways.Api.Extensions;
using Gateways.Api.Transforms;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddGatewayAuthentication(builder.Configuration); // Custom extension method
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomTransformProvider>(); // Register custom transforms

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Optional: Add developer-friendly diagnostics
}

app.UseHttpsRedirection();

app.UseRouting();

// These must be in the correct order: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();


### 3.4. Authentication Configuration (`src/Extensions/AuthConfigurationExtensions.cs`)
This file encapsulates security configuration to keep `Program.cs` clean.

csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Gateways.Api.Extensions;

public static class AuthConfigurationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var authority = jwtSettings["Authority"];
        var audience = jwtSettings["Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // NameClaimType and RoleClaimType can be configured if claims have different names
                    NameClaimType = ClaimTypes.NameIdentifier, 
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            // The default policy used by YARP when a route has "AuthorizationPolicy": "default"
            options.AddPolicy("default", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            
            // Example of a role-based policy
            // options.AddPolicy("AdministratorOnly", policy =>
            // {
            //     policy.RequireRole("Administrator");
            // });
        });

        return services;
    }
}


### 3.5. Custom Request Transforms (`src/Transforms/CustomTransformProvider.cs`)
This class provides a mechanism to programmatically modify requests. A primary use case is injecting a correlation ID for distributed tracing.

csharp
using Yarp.ReverseProxy.Transforms;

namespace Gateways.Api.Transforms;

/// <summary>
/// Provides custom request transformations, such as adding a correlation ID.
/// </summary>
public class CustomTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No validation needed for this simple transform
    }

    public void Apply(TransformBuilderContext context)
    {
        // Add a transform that runs for every request
        context.AddRequestTransform(transformContext =>
        {
            const string correlationIdHeader = "X-Correlation-ID";
            
            // Check if a correlation ID already exists from the incoming request
            var correlationId = transformContext.HttpContext.Request.Headers[correlationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                // If not, generate a new one
                correlationId = Guid.NewGuid().ToString();
            }

            // Set the header on the downstream request, ensuring it's propagated
            transformContext.ProxyRequest.Headers.Add(correlationIdHeader, correlationId);
            
            return ValueTask.CompletedTask;
        });
    }
}



## 4. Security Considerations

- **JWT Validation:** As per **REQ-3-010**, the gateway is the primary enforcer of API security. The `AuthConfigurationExtensions` class configures the `JwtBearer` handler to validate the token's signature, issuer (`Authority`), intended audience (`Audience`), and expiry. Invalid tokens will result in a `401 Unauthorized` response.
- **HTTPS Enforcement:** The gateway should be configured to run on HTTPS in all production environments. `app.UseHttpsRedirection()` will redirect any HTTP traffic to HTTPS.
- **Policy-Based Authorization:** YARP integrates with ASP.NET Core's authorization system. By specifying `"AuthorizationPolicy": "default"` on a route in `appsettings.json`, we ensure that only requests with a valid, authenticated token can access that route. More granular, role-based policies can be defined in `AuthConfigurationExtensions.cs` and applied to specific routes.

## 5. Logging and Error Handling

- **Logging:** Standard ASP.NET Core logging is used. Structured logging (e.g., via Serilog) should be configured to output JSON logs, which can be easily ingested by log aggregation systems (ELK, Splunk). Key information to log includes:
    - YARP's own diagnostic logs (request start/stop, forwarding failures).
    - Authentication success/failure events.
    - Status codes of both incoming and downstream responses.
    - The `X-Correlation-ID` for tracing requests across services.
- **Error Handling:**
    - **Authentication/Authorization Errors:** The JWT handler will automatically return `401 Unauthorized` or `403 Forbidden` responses.
    - **Routing/Downstream Service Errors:** If a downstream service is unavailable or returns a 5xx error, YARP will propagate a `502 Bad Gateway` or `503 Service Unavailable` response to the client. This behavior is configurable.

## 6. Testing Strategy

- **Unit Tests:**
    - Custom logic, such as in `AuthConfigurationExtensions` or `CustomTransformProvider`, should have dedicated unit tests to verify its behavior in isolation.
- **Integration/E2E Tests:**
    - The most critical testing for the gateway is at the integration level. A suite of tests (e.g., using `WebApplicationFactory` or a tool like `k6`/`Playwright`) should be created to:
        - Verify that requests to defined routes are correctly forwarded to a mock downstream service.
        - Verify that requests without a valid JWT to a protected route are rejected with a `401`.
        - Verify that requests with a valid JWT are accepted.
        - Verify that the `X-Correlation-ID` header is correctly added to downstream requests.