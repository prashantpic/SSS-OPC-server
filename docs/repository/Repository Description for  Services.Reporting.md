# Repository Specification

# 1. Name
Services.Reporting


---

# 2. Description
A microservice dedicated to the generation and distribution of reports. It provides APIs to create reports on-demand or on a schedule, based on customizable templates. It can fetch data from the data layer and request insights from the AI service to include in reports. Generated reports can be exported to various formats (PDF, Excel, HTML) and distributed via email or download.


---

# 3. Type
Microservice


---

# 4. Namespace
Opc.System.Services.Reporting


---

# 5. Output Path
src/Services/Reporting


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
ASP.NET Core, gRPC, REST


---

# 9. Thirdparty Libraries

- QuestPDF
- ClosedXML


---

# 10. Dependencies

- REPO-SAP-005
- REPO-SAP-009
- REPO-SAP-012


---

# 11. Layer Ids

- ServerSideApplication_ReportingService


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
REPO-SAP-006


---

# 17. Architecture_Map

- ServerSideApplication_ReportingService


---

# 18. Components_Map

- ReportGenerationEngine
- ReportTemplatingManager
- ReportScheduler
- ReportDistributionManager


---

# 19. Requirements_Map

- REQ-7-018
- REQ-7-019
- REQ-7-020
- REQ-7-021
- REQ-7-022


---

