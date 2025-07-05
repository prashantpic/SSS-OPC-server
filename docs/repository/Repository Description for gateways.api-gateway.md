# Repository Specification

# 1. Name
gateways.api-gateway


---

# 2. Description
This is the master API Gateway repository, serving as the single, unified entry point for all external and UI-driven traffic to the server-side microservices architecture. Its primary responsibility is to coordinate and route incoming requests to the appropriate downstream service. It decouples clients from the internal service topology, providing a consistent API surface. Key coordination functions include: request routing to services like Management, AI, Reporting, and Data; centralizing cross-cutting concerns such as authentication and authorization by validating JWTs (in coordination with the Auth Service); rate limiting to protect backend services; and response aggregation from multiple services if needed. This gateway is critical for system security, manageability, and scalability. It depends on all backend business services to fulfill its routing and coordination duties.


---

# 3. Type
ApiGateway


---

# 4. Namespace
gateways.api


---

# 5. Output Path
gateways/api-gateway


---

# 6. Framework
ASP.NET Core with YARP


---

# 7. Language
C#


---

# 8. Technology
.NET 8, YARP (Yet Another Reverse Proxy)


---

# 9. Thirdparty Libraries

- Yarp.ReverseProxy


---

# 10. Dependencies

- REPO-SAP-004
- REPO-SAP-005
- REPO-SAP-006
- REPO-SAP-007
- REPO-SAP-008
- REPO-SAP-009
- REPO-SAP-010
- REPO-SAP-011


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
APIGateway


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

