# Repository Specification

# 1. Name
services.reporting-service


---

# 2. Description
A microservice responsible for all automated and on-demand report generation. It provides APIs to create, schedule, and distribute customized reports. Users can define report templates, select data sources (including KPIs and AI-driven insights from the AI Service), and choose output formats like PDF, Excel, and HTML. The service can be triggered on a schedule or by specific events. It handles the distribution of generated reports via email or by making them available for download. This service isolates the resource-intensive process of report generation from other system components.


---

# 3. Type
Microservice


---

# 4. Namespace
services.reporting


---

# 5. Output Path
services/reporting-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, QuestPDF, ClosedXML, REST


---

# 9. Thirdparty Libraries

- QuestPDF
- ClosedXML


---

# 10. Dependencies

- REPO-SAP-005
- REPO-SAP-007
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


---

