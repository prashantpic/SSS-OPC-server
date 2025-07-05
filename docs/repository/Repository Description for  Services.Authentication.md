# Repository Specification

# 1. Name
Services.Authentication


---

# 2. Description
The security microservice responsible for user identity and access management. It implements Role-Based Access Control (RBAC), manages users and roles, and can integrate with external Identity Providers (IdP) like Azure AD or Keycloak via OAuth 2.0/OIDC. It is responsible for issuing, validating, and revoking JWTs used to secure API endpoints across the system. All security-relevant actions are audited.


---

# 3. Type
Microservice


---

# 4. Namespace
Opc.System.Services.Authentication


---

# 5. Output Path
src/Services/Authentication


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
ASP.NET Core, ASP.NET Core Identity, OpenID Connect


---

# 9. Thirdparty Libraries

- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Duende.IdentityServer


---

# 10. Dependencies

- REPO-SAP-009
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
REPO-SAP-007


---

# 17. Architecture_Map

- ServerSideApplication_AuthService


---

# 18. Components_Map

- UserManagementApiEndpoints
- RoleManagementApiEndpoints
- IdpIntegrationHandler
- InternalUserStore
- TokenGenerationService
- AccessReviewReportGenerator


---

# 19. Requirements_Map

- REQ-3-003
- REQ-3-004
- REQ-3-005
- REQ-3-012
- REQ-3-014
- REQ-3-016
- REQ-9-001
- REQ-9-002
- REQ-9-003
- REQ-9-018


---

