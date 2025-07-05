# Software Design Specification (SDS): Services.AI

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **Services.AI** microservice. This service is a core component of the SSS-OPC-Client system, responsible for all backend Artificial Intelligence (AI) and Machine Learning (ML) functionalities. It executes predictive and anomaly detection models, processes natural language queries, and manages the lifecycle of AI models. This specification will guide the development and implementation of the service.

### 1.2. Scope
The scope of this document is limited to the `Services.AI` microservice (`REPO-SAP-005`). It includes the design of its internal layers (API, Application, Domain, Infrastructure), its public-facing API endpoints, its interaction patterns with other services (Data Service, MLOps Platforms), and its core logic for handling AI/ML tasks.

## 2. System Overview and Architecture

The `Services.AI` microservice is designed following a **Layered Architecture** style, internally separating concerns into distinct projects: API, Application, Domain, and Infrastructure. This aligns with Domain-Driven Design (DDD) principles and CQRS (Command Query Responsibility Segregation) patterns for handling business logic.

-   **Domain Layer:** Contains the core business logic and entities, such as the `AiModel` aggregate. It is the heart of the service, with no dependencies on other layers.
-   **Application Layer:** Orchestrates the use cases of the application. It contains command and query handlers (using MediatR), defines interfaces for infrastructure dependencies, and handles application-level logic. It depends only on the Domain layer.
-   **Infrastructure Layer:** Provides concrete implementations for the interfaces defined in the Application and Domain layers. It handles all external-facing concerns like database access (via clients to the Data Service), file storage, and communication with third-party services (NLP, MLOps).
-   **API Layer:** The entry point to the microservice. It exposes RESTful API endpoints using ASP.NET Core controllers. Its role is to handle HTTP requests, perform initial validation, and delegate the work to the Application layer via MediatR.

