# Software Design Specification (SDS) for Reporting Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Reporting Service**. This microservice is a core component of the SSS-OPC-Client system, responsible for generating, managing, scheduling, and distributing reports. It operates as an independent, scalable service that consumes data from other system components (like the AI Service and Data Service) to produce reports in various formats (PDF, Excel, HTML).

### 1.2. Scope
The scope of this document is limited to the `services.reporting-service` repository. It covers the design of its REST API, internal architecture, domain logic, data persistence, and integration with other services. The design adheres to the principles of a layered, Domain-Driven Design (DDD) and CQRS architecture.

### 1.3. Technologies
- **Framework:** .NET 8, ASP.NET Core
- **Language:** C#
- **API Style:** RESTful HTTP
- **Report Generation:**
    - **PDF:** QuestPDF
    - **Excel:** ClosedXML
- **Data Persistence:** Entity Framework Core with a PostgreSQL database
- **Scheduling:** Hangfire
- **Messaging/Integration:** MediatR (in-process), `HttpClientFactory` with Polly (for inter-service communication)
- **Logging:** Serilog

---

## 2. Architectural Design

The Reporting Service follows a clean, layered architecture combined with the CQRS (Command Query Responsibility Segregation) pattern.

-   **API Layer (`Reporting.API`):** The outermost layer. It exposes RESTful endpoints, handles HTTP requests/responses, and performs authentication/authorization. It depends on the Application layer to execute actions.
-   **Application Layer (`Reporting.Application`):** The core orchestration layer. It contains the application logic (use cases) implemented as command and query handlers (using MediatR). It defines contracts (interfaces) for infrastructure dependencies but does not implement them. It depends on the Domain layer.
-   **Domain Layer (`Reporting.Domain`):** The heart of the service. It contains the business entities (Aggregates), value objects, and domain logic, completely isolated from infrastructure concerns. It has no external dependencies.
-   **Infrastructure Layer (`Reporting.Infrastructure`):** The implementation layer. It provides concrete implementations for the contracts defined in the Application layer, such as repositories (using EF Core), report generators, schedulers (Hangfire), and clients for external services.

