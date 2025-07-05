# Repository Specification

# 1. Name
services.management-service


---

# 2. Description
A microservice dedicated to the centralized management and monitoring of all deployed OPC Client instances. It provides APIs for registering new clients, pushing configuration updates, and performing bulk operations across multiple clients. It collects and aggregates health status and Key Performance Indicators (KPIs) from each client, such as connection status and data throughput, making this information available to the centralized dashboard. This service is essential for maintaining the operational health and configuration consistency of a distributed fleet of OPC clients. It stores client configuration and status data using the Data Service.


---

# 3. Type
Microservice


---

# 4. Namespace
services.management


---

# 5. Output Path
services/management-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, gRPC, REST


---

# 9. Thirdparty Libraries



---

# 10. Dependencies

- REPO-SAP-007
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


---

# 19. Requirements_Map

- REQ-6-001
- REQ-6-002


---

