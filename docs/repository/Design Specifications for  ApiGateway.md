# Software Design Specification (SDS) for ApiGateway

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **ApiGateway** service. The ApiGateway acts as the single, secure entry point for all external client requests to the system's backend microservices. Its primary responsibilities are to route requests, enforce security policies, and handle cross-cutting concerns like logging and rate limiting. This design is based on the system architecture which specifies a microservices pattern, with the ApiGateway serving as the facade.

### 1.2. Scope
This SDS covers the design and implementation of the ApiGateway, built with .NET 8 and using YARP (Yet Another Reverse Proxy) as its core proxy engine. The design includes:
- Dynamic request routing based on configuration.
- JWT-based authentication and authorization for securing API endpoints.
- Structured logging and distributed tracing for observability.
- Containerization for deployment.

### 1.3. Requirements Mapping
This component directly addresses the following system requirements:
- **REQ-SAP-002:** Exposing RESTful APIs or gRPC endpoints for inter-service and external client communication.
- **REQ-3-010:** Protecting server-side application endpoints using robust authentication methods, specifically JSON Web Tokens (JWT).

## 2. System Overview and Architecture

The ApiGateway implements the **API Gateway** pattern. It sits at the edge of the server-side application, abstracting the internal microservice composition from external clients (like the Blazor WebAssembly UI or third-party applications).

**Architectural Responsibilities:**
- **Routing:** Dynamically forwards incoming HTTP/gRPC requests to the appropriate downstream microservice based on configurable rules.
- **Security:** Acts as the primary security checkpoint. It authenticates every request by validating a JWT bearer token before forwarding it to a trusted downstream service.
- **Decoupling:** Clients only need to know the address of the gateway, not the individual addresses of the dozen-plus microservices. This simplifies client-side configuration and allows the backend architecture to evolve without impacting clients.
- **Cross-Cutting Concerns:** Centralizes logic for logging, tracing, rate limiting, and CORS policies, preventing code duplication in downstream services.

