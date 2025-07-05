# Repository Specification

# 1. Name
Opc.Client.Core


---

# 2. Description
The core OPC client library/service, developed in .NET 8 for cross-platform compatibility (Windows, Linux, macOS). This component is responsible for all direct communication with various OPC servers (DA, UA, XML-DA, HDA, A&C), handling real-time data access, historical data queries, and alarm/event management. It implements robust subscription handling with buffering, failover logic for server redundancy, and security features as per OPC UA standards. It also includes capabilities for executing lightweight AI models on the edge and communicates with the centralized server-side application via gRPC and message queues.


---

# 3. Type
ApplicationCore


---

# 4. Namespace
Opc.Client.Core


---

# 5. Output Path
src/Core/Opc.Client.Core


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
.NET 8, gRPC, RabbitMQ.Client, Serilog, OpenTelemetry, ONNX Runtime


---

# 9. Thirdparty Libraries

- OPCFoundation.NetStandard.Opc.Ua
- Grpc.Net.Client
- RabbitMQ.Client
- Serilog
- OpenTelemetry
- Microsoft.ML.OnnxRuntime


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
Hybrid


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
- OpcXmlDaCommunicator
- OpcHdaCommunicator
- OpcAcCommunicator
- NamespaceBrowser
- TagConfigurationImporter
- CriticalWriteLogger
- WriteOperationLimiter
- ClientSideValidator
- ServerAppGrpcClient
- ServerAppMessageProducer
- EdgeAiExecutor
- LocalDataBufferer


---

# 19. Requirements_Map

- REQ-SAP-001
- REQ-SAP-003
- REQ-CSVC-001
- REQ-CSVC-002
- REQ-CSVC-003
- REQ-CSVC-004
- REQ-CSVC-005
- REQ-CSVC-006
- REQ-CSVC-007
- REQ-CSVC-008
- REQ-CSVC-009
- REQ-CSVC-010
- REQ-CSVC-011
- REQ-CSVC-012
- REQ-CSVC-014
- REQ-CSVC-015
- REQ-CSVC-017
- REQ-CSVC-018
- REQ-CSVC-020
- REQ-CSVC-023
- REQ-CSVC-024
- REQ-CSVC-026
- REQ-CSVC-028
- REQ-3-001
- REQ-DLP-014
- REQ-DLP-015
- REQ-7-001
- REQ-8-001


---

