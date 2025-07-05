# Software Design Specification (SDS) for Opc.Client.Core

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design for the `Opc.Client.Core` library. This library serves as the primary component for all interactions with various OPC (Open Platform Communications) servers within the wider system. It is a cross-platform .NET 8 library responsible for data acquisition, historical access, alarm management, and secure communication with both OPC servers and the centralized server-side application.

### 1.2. Scope
The scope of this document is limited to the `Opc.Client.Core` repository. It covers the internal architecture, component design, data models, and interfaces required to fulfill its specified responsibilities. This includes:
- Communication with OPC DA, UA, XML-DA, HDA, and A&C servers.
- Implementation of robust subscription handling with buffering and reconnection logic.
- Secure communication based on OPC UA standards.
- Local execution of lightweight AI models using ONNX Runtime.
- Asynchronous and synchronous communication with the server-side application.

### 1.3. Acronyms and Abbreviations
- **OPC:** Open Platform Communications
- **UA:** Unified Architecture
- **DA:** Data Access (Classic)
- **HDA:** Historical Data Access
- **A&C:** Alarms & Conditions
- **SDK:** Software Development Kit
- **CQRS:** Command Query Responsibility Segregation
- **DTO:** Data Transfer Object
- **gRPC:** gRPC Remote Procedure Call
- **ONNX:** Open Neural Network Exchange
- **MQ:** Message Queue

---

## 2. System Overview

The `Opc.Client.Core` library is designed using a **Layered Architecture** combined with principles from **Domain-Driven Design (DDD)** and **CQRS**. This approach ensures a clear separation of concerns, making the library maintainable, testable, and extensible.

- **Domain Layer:** Contains the core business logic, aggregates, entities, and value objects that represent the problem domain of OPC communication. It is completely independent of any infrastructure concerns.
- **Application Layer:** Orchestrates the domain logic. It defines application services, commands, queries, and interfaces (abstractions) for infrastructure-level concerns. This layer is the primary entry point for using the library's features.
- **Infrastructure Layer:** Contains all the concrete implementations for external-facing concerns, such as specific OPC protocol clients (using the OPC Foundation SDK), message queue producers (RabbitMQ), gRPC clients, and AI model runners (ONNX Runtime).

This architecture allows the core application and domain logic to remain stable while the implementation details of how we talk to the outside world can be swapped or updated with minimal impact.

