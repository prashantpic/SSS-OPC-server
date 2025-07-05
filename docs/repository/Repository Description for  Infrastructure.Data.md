# Repository Specification

# 1. Name
Infrastructure.Data


---

# 2. Description
A foundational data access library used by all server-side services to interact with persistence layers. It encapsulates the logic for connecting to and querying the relational database (PostgreSQL) using EF Core, the time-series database (TimescaleDB/InfluxDB), and blob storage (Azure Blob/AWS S3). It exposes repositories for each data entity, implementing the patterns defined in the database design and data access logic.


---

# 3. Type
DataAccess


---

# 4. Namespace
Opc.System.Infrastructure.Data


---

# 5. Output Path
src/Infrastructure/Data


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
Entity Framework Core, Npgsql, InfluxDB.Client


---

# 9. Thirdparty Libraries

- Npgsql.EntityFrameworkCore.PostgreSQL
- InfluxDB.Client
- Azure.Storage.Blobs


---

# 10. Dependencies

- REPO-SAP-012


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
LayeredArchitecture


---

# 16. Id
REPO-SAP-009


---

# 17. Architecture_Map

- ServerSideApplication_DataService


---

# 18. Components_Map

- RelationalDbContext
- TimeSeriesDbClient
- NoSqlBlobStorageClient
- DataQueryEndpoints
- BlockchainLogRepository


---

# 19. Requirements_Map

- REQ-DLP-001
- REQ-DLP-002
- REQ-DLP-003
- REQ-DLP-005
- REQ-DLP-008
- REQ-DLP-009
- REQ-DLP-024
- REQ-DLP-025
- REQ-CSVC-013
- REQ-CSVC-019


---

