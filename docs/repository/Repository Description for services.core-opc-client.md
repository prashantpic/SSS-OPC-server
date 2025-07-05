# Repository Specification

# 1. Name
services.core-opc-client


---

# 2. Description
Contains the core .NET 6+ cross-platform OPC client library/service. This service is a standalone application responsible for all direct communications with industrial hardware via OPC DA, OPC UA, OPC XML-DA, OPC HDA, and OPC A&C protocols. It implements the primary data acquisition logic, including real-time reads/writes, historical data access, and alarm/event monitoring. It manages subscriptions, handles connection redundancy and failover, and implements client-side buffering to prevent data loss during network interruptions. For edge deployments, it can execute lightweight AI models (ONNX) for predictive maintenance and anomaly detection locally. It securely communicates with the server-side application, pushing data and events via message queues and pulling configuration via gRPC/REST APIs. It is a critical component for bridging the OT and IT environments.


---

# 3. Type
ApplicationService


---

# 4. Namespace
services.opc.client


---

# 5. Output Path
services/core-opc-client


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
.NET 8, OPCFoundation.NetStandard.Opc.Ua, Grpc.Net.Client, RabbitMQ.Client, ONNX Runtime, Serilog


---

# 9. Thirdparty Libraries

- OPCFoundation.NetStandard.Opc.Ua
- Grpc.Net.Client
- RabbitMQ.Client
- Microsoft.ML.OnnxRuntime
- Serilog


---

# 10. Dependencies

- REPO-SAP-012


---

# 11. Layer Ids

- CoreOpcClientService


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
ClientServer


---

# 16. Id
REPO-SAP-001


---

# 17. Architecture_Map

- CoreOpcClientService


---

# 18. Components_Map

- OpcDaCommunicator
- OpcUaCommunicator
- OpcHdaCommunicator
- OpcAcCommunicator
- SubscriptionManager
- ServerAppGrpcClient
- ServerAppMessageProducer
- EdgeAiExecutor


---

# 19. Requirements_Map

- REQ-SAP-001
- REQ-CSVC-001
- REQ-CSVC-003
- REQ-CSVC-011
- REQ-CSVC-017
- REQ-CSVC-023
- REQ-7-001
- REQ-8-001
- REQ-SAP-015


---

