# Repository Specification

# 1. Name
services.ai-service


---

# 2. Description
A microservice that encapsulates all backend Artificial Intelligence and Machine Learning logic. Its responsibilities include: executing predictive maintenance and anomaly detection models on data streams, processing natural language queries by integrating with NLP providers, and managing the AI model lifecycle (versioning, deployment, monitoring for drift) through MLOps platform integration. It exposes APIs for getting predictions, detecting anomalies, and processing user feedback to retrain models. It fetches historical data from the Data Service for model training/inference and stores model artifacts in blob storage.


---

# 3. Type
Microservice


---

# 4. Namespace
services.ai


---

# 5. Output Path
services/ai-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, ML.NET, ONNX Runtime, REST, gRPC


---

# 9. Thirdparty Libraries

- ML.NET
- Microsoft.ML.OnnxRuntime


---

# 10. Dependencies

- REPO-SAP-007
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


---

# 19. Requirements_Map

- REQ-7-001
- REQ-7-008
- REQ-7-013
- REQ-7-004


---

