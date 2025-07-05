# Architecture Design Specification

# 1. Patterns

## 1.1. Request-Reply
### 1.1.2. Type
Synchronous

### 1.1.3. Implementation
gRPC and HTTP/REST via API Gateway

### 1.1.4. Applicability
Essential for interactions between the Core OPC Client, Web UI, and server-side microservices where immediate responses are required.  Used for configuration retrieval, OPC data browsing, on-demand report generation, and user authentication.

## 1.2. Publish-Subscribe
### 1.2.2. Type
Asynchronous

### 1.2.3. Implementation
Message Queues (RabbitMQ/Kafka)

### 1.2.4. Applicability
Handles real-time OPC data updates, alarm/event notifications, and audit logging from the Core OPC Client to the server-side application. Decouples components and enables independent scaling. Supports buffering during temporary network interruptions.

## 1.3. Circuit Breaker
### 1.3.2. Type
Reliability

### 1.3.3. Implementation
.NET libraries (Polly)

### 1.3.4. Applicability
Protects against cascading failures by preventing repeated calls to unresponsive services (e.g., external NLP provider, IoT platforms). Improves system resilience.



---

