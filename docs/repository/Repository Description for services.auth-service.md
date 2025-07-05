# Repository Specification

# 1. Name
services.auth-service


---

# 2. Description
A dedicated microservice for handling all authentication and authorization concerns. It is the central authority for system security. Responsibilities include: managing users, roles, and permissions (RBAC); integrating with external Identity Providers (IdPs) like Azure AD or Okta via OAuth 2.0/OIDC; providing an internal user store with secure password policies as a fallback; and issuing, validating, and refreshing JSON Web Tokens (JWTs) used to secure API endpoints across the entire microservices ecosystem. By centralizing security logic, it ensures consistent application of security policies and simplifies the implementation of other services.


---

# 3. Type
AuthenticationService


---

# 4. Namespace
services.auth


---

# 5. Output Path
services/auth-service


---

# 6. Framework
ASP.NET Core Identity


---

# 7. Language
C#


---

# 8. Technology
.NET 8, ASP.NET Core Identity, OpenID Connect


---

# 9. Thirdparty Libraries

- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Identity.EntityFrameworkCore


---

# 10. Dependencies

- REPO-SAP-007
- REPO-SAP-012


---

# 11. Layer Ids

- ServerSideApplication_AuthService


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
REPO-SAP-008


---

# 17. Architecture_Map

- ServerSideApplication_AuthService


---

# 18. Components_Map

- UserManagementApiEndpoints
- RoleManagementApiEndpoints
- IdpIntegrationHandler
- TokenGenerationService


---

# 19. Requirements_Map

- REQ-3-003
- REQ-3-004
- REQ-9-001


---

