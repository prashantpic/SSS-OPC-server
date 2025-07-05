# Software Design Specification (SDS) for Services.Authentication

## 1. Introduction

### 1.1 Purpose

This document outlines the detailed software design for the **Services.Authentication** microservice. This service is a critical component of the Opc.System platform, acting as the centralized authority for identity and access management. Its primary responsibilities include user authentication (both internal and via external providers), Role-Based Access Control (RBAC), API security through JSON Web Tokens (JWTs), and comprehensive security auditing.

### 1.2 Scope

The scope of this document is limited to the `Services.Authentication` microservice. It covers:
-   Internal architecture based on .NET 8 and ASP.NET Core.
-   Database schema for identity and audit data.
-   API endpoint definitions.
-   Integration with external Identity Providers (IdPs).
-   Implementation of RBAC and data privacy requirements.
-   Configuration and operational aspects.

### 1.3 Technology Stack

-   **Framework:** .NET 8, ASP.NET Core
-   **Identity Management:** ASP.NET Core Identity, Duende IdentityServer (or OpenIddict as an alternative)
-   **Database:** Entity Framework Core 8 with a PostgreSQL or SQL Server provider.
-   **API:** RESTful APIs built with ASP.NET Core MVC.
-   **Application Pattern:** Clean Architecture with CQRS (using MediatR).

---

## 2. System Architecture

The service will be built following the principles of **Clean Architecture** (also known as Onion Architecture). This creates a loosely coupled, maintainable, and testable system by separating concerns into distinct layers.

-   **Domain:** Contains the core business entities (`ApplicationUser`, `ApplicationRole`, `AuditLog`) and business rules. It has no dependencies on other layers.
-   **Application:** Contains the application logic, use cases (CQRS commands/queries), and interfaces for infrastructure concerns (`IUserRepository`, `IJwtGenerator`, `IAuditService`). It depends only on the Domain layer.
-   **Infrastructure:** Provides concrete implementations for the interfaces defined in the Application layer. This includes database access (`AuthDbContext`), JWT generation, external provider integration, and other services. It depends on the Application layer.
-   **API (Presentation):** The external-facing layer, consisting of ASP.NET Core controllers. It exposes the system's functionality via RESTful endpoints and depends on the Application layer (via MediatR) to execute commands and queries.

The **Command Query Responsibility Segregation (CQRS)** pattern will be implemented using the **MediatR** library to decouple API controllers from the application logic handlers.

