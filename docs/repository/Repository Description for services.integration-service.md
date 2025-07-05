# Repository Specification

# 1. Name
services.integration-service


---

# 2. Description
A microservice that acts as a bridge to external and emerging technology platforms. It encapsulates the complexity of connecting to various third-party systems. Its responsibilities include: connecting to cloud IoT platforms (Azure IoT, AWS IoT) via MQTT/AMQP; streaming real-time data to Augmented Reality (AR) devices via WebSockets; logging critical transactions to a private permissioned blockchain (Hyperledger Fabric); and facilitating bi-directional data flow with Digital Twin platforms. This service handles protocol translation, data mapping, and secure credential management for all external integrations, allowing the core system to remain agnostic of these specific technologies.


---

# 3. Type
Microservice


---

# 4. Namespace
services.integration


---

# 5. Output Path
services/integration-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, MQTTnet, Nethereum, WebSockets


---

# 9. Thirdparty Libraries

- MQTTnet
- Nethereum.Web3


---

# 10. Dependencies

- REPO-SAP-007
- REPO-SAP-012


---

# 11. Layer Ids

- ServerSideApplication_ExternalIntegrationService


---

# 12. Requirements



---

# 13. Generate Tests
True


---

# 14. Generate Documentation
True


---

# 15. Architecture Style
Microservices


---

# 16. Id
REPO-SAP-009


---

# 17. Architecture_Map

- ServerSideApplication_ExternalIntegrationService


---

# 18. Components_Map

- IotPlatformConnector
- ArDataStreamer
- BlockchainAdapter
- DigitalTwinAdapter


---

# 19. Requirements_Map

- REQ-8-004
- REQ-8-006
- REQ-8-007
- REQ-8-010


---

