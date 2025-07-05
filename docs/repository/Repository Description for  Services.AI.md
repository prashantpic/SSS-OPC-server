# Repository Specification

# 1. Name
Services.AI


---

# 2. Description
The AI processing microservice, which handles backend machine learning tasks. Its responsibilities include executing predictive maintenance and anomaly detection models (e.g., in ONNX format), processing natural language queries by integrating with NLP providers, and managing the AI model lifecycle (deployment, versioning, monitoring). It interacts with the data layer to fetch historical data for analysis and store model artifacts.


---

# 3. Type
Microservice


---

# 4. Namespace
Opc.System.Services.AI


---

# 5. Output Path
src/Services/AI


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
ASP.NET Core, gRPC, REST, ML.NET, ONNX Runtime


---

# 9. Thirdparty Libraries

- Microsoft.ML
- Microsoft.ML.OnnxRuntime
- Azure.AI.Language.QuestionAnswering


---

# 10. Dependencies

- REPO-SAP-009
- REPO-SAP-012


---

# 11. Layer Ids

- ServerSideApplication_AiService


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
REPO-SAP-005


---

# 17. Architecture_Map

- ServerSideApplication_AiService


---

# 18. Components_Map

- PredictiveMaintenanceEngine
- AnomalyDetectionEngine
- NlqProcessor
- ModelManagementInterface
- AiFeedbackHandler


---

# 19. Requirements_Map

- REQ-7-001
- REQ-7-002
- REQ-7-003
- REQ-7-004
- REQ-7-005
- REQ-7-006
- REQ-7-008
- REQ-7-009
- REQ-7-010
- REQ-7-011
- REQ-7-013
- REQ-7-014
- REQ-7-015
- REQ-7-016


---

