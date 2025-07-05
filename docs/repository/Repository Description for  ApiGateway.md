# Repository Specification

# 1. Name
ApiGateway


---

# 2. Description
An API Gateway built with ASP.NET Core, serving as the single entry point for all external requests to the server-side application. It routes incoming HTTP/gRPC requests to the appropriate backend microservices. This component is responsible for cross-cutting concerns such as request authentication (JWT validation), authorization, rate limiting, logging, and potentially response caching and aggregation. It decouples the web frontend and external clients from the internal microservice architecture.


---

# 3. Type
ApiGateway


---

# 4. Namespace
Opc.System.ApiGateway


---

# 5. Output Path
src/Gateways/ApiGateway


---

# 6. Framework
.NET 8


---

# 7. Language
C#


---

# 8. Technology
ASP.NET Core, YARP (or Ocelot), JWT Bearer Authentication


---

# 9. Thirdparty Libraries

- Yarp.ReverseProxy
- Serilog.AspNetCore
- OpenTelemetry.Extensions.Hosting
- Microsoft.AspNetCore.Authentication.JwtBearer


---

# 10. Dependencies

- REPO-SAP-004
- REPO-SAP-005
- REPO-SAP-006
- REPO-SAP-007
- REPO-SAP-008


---

# 11. Layer Ids

- ServerSideApplication_Presentation


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
REPO-SAP-002


---

# 17. Architecture_Map

- ServerSideApplication_Presentation


---

# 18. Components_Map

- ApiGatewayComponent


---

# 19. Requirements_Map

- REQ-SAP-002
- REQ-3-010


---

