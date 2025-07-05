# Software Design Specification (SDS) for AI Service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the **AI Service (`services.ai-service`)**. This microservice is a core component of the SSS-OPC-Client system, responsible for all backend Artificial Intelligence (AI) and Machine Learning (ML) functionalities. It provides a centralized, scalable, and manageable platform for executing AI models, processing natural language, and managing the model lifecycle.

### 1.2. Scope
The scope of this document is limited to the design of the `services.ai-service` microservice. This includes:
-   **Predictive Maintenance:** Executing models to forecast equipment maintenance needs.
-   **Anomaly Detection:** Analyzing data streams to identify unusual patterns.
-   **Natural Language Query (NLQ):** Processing user queries in natural language to retrieve data.
-   **AI Model Management:** Providing capabilities for model deployment, versioning, and monitoring, integrating with MLOps platforms.

This service interacts with the **Data Service** to fetch historical data and the **API Gateway** to expose its functionalities. It stores model artifacts in a configured **Blob Storage**.

## 2. System Architecture

The AI Service adopts a **Clean Architecture** (also known as Onion or Hexagonal Architecture) to ensure a high degree of separation of concerns, testability, and maintainability. The architecture is composed of the following layers (projects):

-   **`AiService.Domain`**: The core of the application. It contains all domain models, entities, aggregates, value objects, and business rules. It is completely independent of any technology-specific implementation details.
-   **`AiService.Application`**: This layer orchestrates the use cases of the application. It contains application logic, commands, queries (following the CQRS pattern), and interfaces for infrastructure-level concerns (e.g., repositories, external service clients). It depends only on the Domain layer.
-   **`AiService.Infrastructure`**: This layer provides concrete implementations for the interfaces defined in the Application layer. It handles all external concerns such as database access, file storage, AI model execution (ONNX Runtime), and communication with other services or third-party APIs. It depends on the Application layer.
-   **`AiService.Api`**: The presentation layer. It exposes the service's functionality to the outside world via RESTful APIs and gRPC endpoints. It depends on both the Application and Infrastructure layers to wire everything together and handle requests.

Communication within the application layer is orchestrated using the **Mediator pattern** (with the MediatR library) to decouple command/query senders from their handlers.

