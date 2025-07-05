# Repository Specification

# 1. Name
services.licensing-service


---

# 2. Description
A microservice that manages all software licensing aspects. It is responsible for issuing, activating, and validating licenses based on various models (per-user, per-site, subscription). It provides APIs for other services to check feature enablement based on the current license tier. This service supports both online and offline activation mechanisms to accommodate air-gapped environments. It handles grace periods for temporary validation failures and provides administrative endpoints for managing license keys. Centralizing licensing logic ensures consistent enforcement of commercial terms across the entire platform.


---

# 3. Type
Microservice


---

# 4. Namespace
services.licensing


---

# 5. Output Path
services/licensing-service


---

# 6. Framework
ASP.NET Core


---

# 7. Language
C#


---

# 8. Technology
.NET 8, REST


---

# 9. Thirdparty Libraries



---

# 10. Dependencies

- REPO-SAP-007
- REPO-SAP-012


---

# 11. Layer Ids



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
REPO-SAP-011


---

# 17. Architecture_Map



---

# 18. Components_Map

- LicenseValidationEndpoint
- LicenseActivationEndpoint
- FeatureEntitlementChecker


---

# 19. Requirements_Map

- REQ-9-006
- REQ-9-007
- REQ-9-008


---

