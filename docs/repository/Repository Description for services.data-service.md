# Repository Specification

# 1. Name
services.data-service


---

# 2. Description
A microservice that acts as a data abstraction layer, responsible for all interactions with the system's various persistence stores. It provides a unified API for other services to access data without needing to know the underlying database technology. Its responsibilities include: ingesting and storing real-time and alarm data into a time-series database (TimescaleDB/InfluxDB); managing user configurations, roles, and audit logs in a relational database (PostgreSQL); storing unstructured data like AI models in blob storage; and implementing data retention policies for archiving and purging data. It exposes data access endpoints and consumes data from the message queue.


---

# 3. Type
Microservice


---

# 4. Namespace
services.data


---

# 5. Output Path
services/data-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, Entity Framework Core, Npgsql, InfluxDB.Client


---

# 9. Thirdparty Libraries

- EntityFrameworkCore
- Npgsql
- InfluxDB.Client


---

# 10. Dependencies

- REPO-SAP-012
- REPO-SAP-013


---

# 11. Layer Ids

- ServerSideApplication_DataService


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
REPO-SAP-007


---

# 17. Architecture_Map

- ServerSideApplication_DataService


---

# 18. Components_Map

- RelationalDbContext
- TimeSeriesDbClient
- NoSqlBlobStorageClient
- DataIngestionEndpoints
- DataQueryEndpoints
- DataRetentionPolicyManager


---

# 19. Requirements_Map

- REQ-DLP-001
- REQ-DLP-008
- REQ-DLP-017


---

