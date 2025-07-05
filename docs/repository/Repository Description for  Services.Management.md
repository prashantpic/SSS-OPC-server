# Repository Specification

# 1. Name
Services.Management


---

# 2. Description
A microservice responsible for the centralized management of OPC client instances. It provides APIs to configure clients, monitor their health and performance KPIs, and support bulk operations for configuration deployment and software updates. It persists client configurations and state in the relational database via the data access layer.


---

# 3. Type
Microservice


---

# 4. Namespace
Opc.System.Services.Management


---

# 5. Output Path
src/Services/Management


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
ASP.NET Core, gRPC, REST, Entity Framework Core


---

# 9. Thirdparty Libraries



---

# 10. Dependencies

- REPO-SAP-009
- REPO-SAP-012


---

# 11. Layer Ids

- ServerSideApplication_ManagementService


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
REPO-SAP-004


---

# 17. Architecture_Map

- ServerSideApplication_ManagementService


---

# 18. Components_Map

- ClientConfigurationApiEndpoints
- ClientMonitoringApiEndpoints
- BulkOperationHandler
- ClientHealthAggregator


---

# 19. Requirements_Map

- REQ-SAP-002
- REQ-6-001
- REQ-6-002
- REQ-9-004
- REQ-9-005
- REQ-SAP-009


---

