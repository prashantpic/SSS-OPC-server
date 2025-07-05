# Specification

# 1. Files

- **Path:** src/Services.Authentication/Services.Authentication.csproj  
**Description:** The .NET 8 project file for the Authentication microservice. Defines dependencies, target framework, and project settings.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** Services.Authentication  
**Type:** Project  
**Relative Path:**   
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the project structure and dependencies for the Authentication microservice.  
**Logic Description:** This file will list all NuGet package references, such as ASP.NET Core, EF Core, ASP.NET Core Identity, Duende IdentityServer or OpenIddict, JWT Bearer, MediatR, and any external IdP OIDC client libraries.  
**Documentation:**
    
    - **Summary:** The C# project file (.csproj) that configures the build process and manages all dependencies for the Authentication service.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** src/Services.Authentication/Program.cs  
**Description:** The main entry point for the Authentication microservice. Configures the ASP.NET Core host, registers services for dependency injection, and sets up the HTTP request pipeline.  
**Template:** C# Program  
**Dependency Level:** 4  
**Name:** Program  
**Type:** EntryPoint  
**Relative Path:**   
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - Application Bootstrap
    - Dependency Injection Setup
    - Middleware Configuration
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-3-004
    - REQ-3-005
    
**Purpose:** Initializes and runs the Authentication microservice.  
**Logic Description:** Configures services for each layer: API controllers, Application (MediatR), Infrastructure (DbContext, Repositories, JWT Generation, IdP handlers), and Domain. Sets up middleware for routing, authentication, authorization, and custom exception handling. Seeds the database with default roles and permissions on startup.  
**Documentation:**
    
    - **Summary:** Bootstraps the entire Authentication service. It wires up all the components, configures the web host, and defines the request processing pipeline.
    
**Namespace:** Opc.System.Services.Authentication  
**Metadata:**
    
    - **Category:** ApplicationHost
    
- **Path:** src/Services.Authentication/appsettings.json  
**Description:** JSON configuration file for the Authentication service. Contains settings for database connections, JWT generation, password policies, and external provider details.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:**   
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - Externalized Configuration
    
**Requirement Ids:**
    
    - REQ-3-004
    - REQ-3-005
    
**Purpose:** To provide environment-specific configuration values for the application without hardcoding them.  
**Logic Description:** Defines sections for 'ConnectionStrings', 'JwtSettings' (Secret, Issuer, Audience, ExpiryInMinutes), 'PasswordPolicy' (MinLength, RequireUppercase, etc.), and an array of 'ExternalIdpSettings' (Scheme, Authority, ClientId, ClientSecret).  
**Documentation:**
    
    - **Summary:** Provides the core configuration data for the service. Sensitive values should be overridden by environment variables or a secrets manager in production.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Services.Authentication/Domain/Entities/ApplicationUser.cs  
**Description:** Represents a user in the system. Extends the base IdentityUser to include application-specific properties.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** ApplicationUser  
**Type:** Entity  
**Relative Path:** Domain/Entities  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** FirstName  
**Type:** string  
**Attributes:** public  
    - **Name:** LastName  
**Type:** string  
**Attributes:** public  
    - **Name:** IsActive  
**Type:** bool  
**Attributes:** public  
    - **Name:** CreatedAt  
**Type:** DateTimeOffset  
**Attributes:** public  
    - **Name:** UpdatedAt  
**Type:** DateTimeOffset  
**Attributes:** public  
    - **Name:** ExternalProviderId  
**Type:** string?  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - User Profile
    
**Requirement Ids:**
    
    - REQ-3-005
    - REQ-9-001
    
**Purpose:** To model a user within the authentication and authorization bounded context, holding identity and status information.  
**Logic Description:** This class inherits from 'Microsoft.AspNetCore.Identity.IdentityUser<Guid>'. It serves as the Aggregate Root for user-related operations. It contains properties for user details and status. Logic for deactivating or anonymizing a user for DSAR purposes will be encapsulated here.  
**Documentation:**
    
    - **Summary:** The core User entity. It represents an individual who can be authenticated and authorized to access system resources.
    
**Namespace:** Opc.System.Services.Authentication.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services.Authentication/Domain/Entities/ApplicationRole.cs  
**Description:** Represents a role in the system. Extends the base IdentityRole.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** ApplicationRole  
**Type:** Entity  
**Relative Path:** Domain/Entities  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** Description  
**Type:** string  
**Attributes:** public  
    - **Name:** IsSystemRole  
**Type:** bool  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Role Definition
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-9-002
    
**Purpose:** To model a collection of permissions that can be assigned to users, defining their access level.  
**Logic Description:** This class inherits from 'Microsoft.AspNetCore.Identity.IdentityRole<Guid>'. The 'IsSystemRole' property will prevent deletion of essential, predefined roles.  
**Documentation:**
    
    - **Summary:** The core Role entity. It groups permissions together, forming the basis of Role-Based Access Control (RBAC).
    
**Namespace:** Opc.System.Services.Authentication.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services.Authentication/Domain/Entities/AuditLog.cs  
**Description:** Represents a single, immutable entry in the security audit trail.  
**Template:** C# Class  
**Dependency Level:** 0  
**Name:** AuditLog  
**Type:** Entity  
**Relative Path:** Domain/Entities  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Timestamp  
**Type:** DateTimeOffset  
**Attributes:** public  
    - **Name:** EventType  
**Type:** string  
**Attributes:** public  
    - **Name:** ActingUserId  
**Type:** Guid?  
**Attributes:** public  
    - **Name:** SubjectId  
**Type:** string?  
**Attributes:** public  
    - **Name:** Details  
**Type:** string  
**Attributes:** public  
    - **Name:** Outcome  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Security Auditing
    
**Requirement Ids:**
    
    - REQ-3-012
    - REQ-9-003
    
**Purpose:** To capture and persist security-relevant events for compliance and investigation purposes.  
**Logic Description:** A plain entity to hold audit data. The 'Details' property will be a JSON string to flexibly store event-specific information. It has no update methods to ensure immutability once created.  
**Documentation:**
    
    - **Summary:** An entity representing an audit log entry, capturing who did what, when, to what, and what the result was.
    
**Namespace:** Opc.System.Services.Authentication.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Services.Authentication/Application/Interfaces/IUserRepository.cs  
**Description:** Interface defining the contract for user data persistence operations.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IUserRepository  
**Type:** RepositoryInterface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByIdAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<ApplicationUser?>  
**Attributes:**   
    - **Name:** GetByUsernameAsync  
**Parameters:**
    
    - string username
    
**Return Type:** Task<ApplicationUser?>  
**Attributes:**   
    - **Name:** AnonymizeUserAsync  
**Parameters:**
    
    - Guid id
    
**Return Type:** Task<bool>  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-3-014
    
**Purpose:** To abstract the data access logic for user entities from the application layer, adhering to the Dependency Inversion Principle.  
**Logic Description:** Defines methods for standard CRUD operations and any specific queries needed by the application layer, such as finding a user by username or email. Includes a method for user anonymization to support DSAR.  
**Documentation:**
    
    - **Summary:** A contract for the user repository, defining all required interactions with the user data store.
    
**Namespace:** Opc.System.Services.Authentication.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Interfaces/IJwtGenerator.cs  
**Description:** Interface for a service that creates and validates JSON Web Tokens (JWT).  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IJwtGenerator  
**Type:** ServiceInterface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GenerateToken  
**Parameters:**
    
    - ApplicationUser user
    - IEnumerable<string> roles
    
**Return Type:** string  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To abstract the token generation logic, allowing for different implementations or configurations.  
**Logic Description:** Defines a single method, 'GenerateToken', that takes user information and their roles, and returns a signed JWT string.  
**Documentation:**
    
    - **Summary:** A contract for the JWT generation service, which is a core part of the 'TokenGenerationService' component.
    
**Namespace:** Opc.System.Services.Authentication.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Interfaces/IAuditService.cs  
**Description:** Interface for a service that logs security-relevant events.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** IAuditService  
**Type:** ServiceInterface  
**Relative Path:** Application/Interfaces  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** LogEventAsync  
**Parameters:**
    
    - string eventType
    - string outcome
    - string details
    - Guid? actingUserId = null
    - string? subjectId = null
    
**Return Type:** Task  
**Attributes:**   
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-3-012
    - REQ-9-003
    
**Purpose:** To provide a centralized, asynchronous way for the application to record audit trail entries.  
**Logic Description:** Defines a method to log an event. The implementation of this interface will create an 'AuditLog' entity and persist it.  
**Documentation:**
    
    - **Summary:** A contract for the audit logging service, ensuring all security events are captured consistently.
    
**Namespace:** Opc.System.Services.Authentication.Application.Interfaces  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Features/Users/Commands/CreateUser/CreateUserCommand.cs  
**Description:** A CQRS command and its handler for creating a new user in the internal user store.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** CreateUserCommand  
**Type:** Command  
**Relative Path:** Application/Features/Users/Commands/CreateUser  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** Username  
**Type:** string  
**Attributes:** public  
    - **Name:** Email  
**Type:** string  
**Attributes:** public  
    - **Name:** Password  
**Type:** string  
**Attributes:** public  
    - **Name:** FirstName  
**Type:** string  
**Attributes:** public  
    - **Name:** LastName  
**Type:** string  
**Attributes:** public  
    - **Name:** Roles  
**Type:** List<string>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - User Creation
    
**Requirement Ids:**
    
    - REQ-3-005
    - REQ-9-001
    
**Purpose:** To encapsulate the data and logic required to execute the user creation use case.  
**Logic Description:** The command class is a simple DTO. The handler will receive this command, use 'UserManager' from ASP.NET Core Identity to create the 'ApplicationUser', validate the password against configured policies, assign the specified roles, and log the successful creation event using 'IAuditService'.  
**Documentation:**
    
    - **Summary:** Represents the intent to create a new user. The handler contains the orchestration logic for this process.
    
**Namespace:** Opc.System.Services.Authentication.Application.Features.Users.Commands.CreateUser  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Features/Auth/Commands/Login/LoginCommand.cs  
**Description:** A CQRS command and handler for authenticating a user and generating a JWT.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** LoginCommand  
**Type:** Command  
**Relative Path:** Application/Features/Auth/Commands/Login  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** Username  
**Type:** string  
**Attributes:** public  
    - **Name:** Password  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - User Authentication
    - JWT Generation
    
**Requirement Ids:**
    
    - REQ-3-005
    
**Purpose:** To handle the user login use case, validating credentials and returning an access token.  
**Logic Description:** The handler will use 'SignInManager' from ASP.NET Core Identity to validate credentials. On success, it will retrieve the user and their roles, then use the 'IJwtGenerator' service to create a JWT. Both successful and failed login attempts will be logged via 'IAuditService'.  
**Documentation:**
    
    - **Summary:** Represents the intent to log in. The handler validates user credentials and issues a token upon success.
    
**Namespace:** Opc.System.Services.Authentication.Application.Features.Auth.Commands.Login  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Features/Roles/Commands/UpdateRolePermissions/UpdateRolePermissionsCommand.cs  
**Description:** A CQRS command and handler for updating the set of permissions associated with a role.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** UpdateRolePermissionsCommand  
**Type:** Command  
**Relative Path:** Application/Features/Roles/Commands/UpdateRolePermissions  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** RoleId  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Permissions  
**Type:** List<string>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - Role Customization
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-9-002
    
**Purpose:** To handle the use case of modifying a role's permissions, enabling role customization.  
**Logic Description:** The handler will receive the command, find the role using 'RoleManager', and manage the role's claims (representing permissions). It will remove existing permission claims and add the new ones. The action will be logged via 'IAuditService'.  
**Documentation:**
    
    - **Summary:** Represents the intent to change the permissions for a specific role. The handler orchestrates the update.
    
**Namespace:** Opc.System.Services.Authentication.Application.Features.Roles.Commands.UpdateRolePermissions  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Features/AccessReview/Queries/GenerateAccessReviewReport/GenerateAccessReviewReportQuery.cs  
**Description:** A CQRS query and handler for generating a report of all users and their assigned roles/permissions.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** GenerateAccessReviewReportQuery  
**Type:** Query  
**Relative Path:** Application/Features/AccessReview/Queries/GenerateAccessReviewReport  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - User Access Reporting
    
**Requirement Ids:**
    
    - REQ-9-018
    
**Purpose:** To provide data for periodic user access reviews, supporting the principle of least privilege.  
**Logic Description:** The handler for this query will access the user and role repositories to fetch all users, their assigned roles, and the permissions for each of those roles. It will then format this data into a structured report DTO, which can be easily consumed by an API endpoint to generate a file (e.g., CSV).  
**Documentation:**
    
    - **Summary:** Represents a request to generate a user access review report. The handler gathers and structures the necessary data.
    
**Namespace:** Opc.System.Services.Authentication.Application.Features.AccessReview.Queries.GenerateAccessReviewReport  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Application/Features/DataPrivacy/Commands/AnonymizeUser/AnonymizeUserCommand.cs  
**Description:** A CQRS command and handler to handle a 'right to be forgotten' request under GDPR/CCPA by anonymizing user data.  
**Template:** C# Class  
**Dependency Level:** 2  
**Name:** AnonymizeUserCommand  
**Type:** Command  
**Relative Path:** Application/Features/DataPrivacy/Commands/AnonymizeUser  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - CQRS
    - Mediator
    
**Members:**
    
    - **Name:** UserId  
**Type:** Guid  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - DSAR Right to Erasure
    
**Requirement Ids:**
    
    - REQ-3-014
    
**Purpose:** To fulfill data privacy requests for user data erasure by removing or scrambling PII.  
**Logic Description:** The handler will find the user by ID. It will then replace personally identifiable information (like FirstName, LastName, Email, Username) with anonymized or placeholder values. It will also disable the account. The event is logged to the audit trail. This is a soft delete approach to maintain referential integrity in logs.  
**Documentation:**
    
    - **Summary:** Represents the intent to anonymize a user's data to comply with data privacy regulations.
    
**Namespace:** Opc.System.Services.Authentication.Application.Features.DataPrivacy.Commands.AnonymizeUser  
**Metadata:**
    
    - **Category:** Application
    
- **Path:** src/Services.Authentication/Infrastructure/Persistence/AuthDbContext.cs  
**Description:** The Entity Framework Core DbContext for the Authentication service. Manages connections and transactions for identity and audit data.  
**Template:** C# DbContext  
**Dependency Level:** 1  
**Name:** AuthDbContext  
**Type:** DbContext  
**Relative Path:** Infrastructure/Persistence  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - UnitOfWork
    
**Members:**
    
    - **Name:** AuditLogs  
**Type:** DbSet<AuditLog>  
**Attributes:** public  
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder builder
    
**Return Type:** void  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - Data Persistence
    - Internal User Store
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-3-005
    - REQ-3-012
    
**Purpose:** To define the database schema and provide a session for querying and saving identity and audit entities.  
**Logic Description:** Inherits from 'IdentityDbContext<ApplicationUser, ApplicationRole, Guid>'. Defines the 'AuditLogs' DbSet. The 'OnModelCreating' method is used to apply entity configurations from separate classes and to seed the database with initial data (predefined roles/permissions).  
**Documentation:**
    
    - **Summary:** The EF Core database context that represents the session with the database, allowing entities to be queried and saved.
    
**Namespace:** Opc.System.Services.Authentication.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Services.Authentication/Infrastructure/Identity/JwtGenerator.cs  
**Description:** Implements the IJwtGenerator interface to create signed JWTs based on user identity and roles.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** JwtGenerator  
**Type:** Service  
**Relative Path:** Infrastructure/Identity  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _jwtSettings  
**Type:** JwtSettings  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateToken  
**Parameters:**
    
    - ApplicationUser user
    - IEnumerable<string> roles
    
**Return Type:** string  
**Attributes:** public  
    
**Implemented Features:**
    
    - Token Generation Service
    
**Requirement Ids:**
    
    
**Purpose:** To centralize the logic for creating standards-compliant JSON Web Tokens.  
**Logic Description:** This service reads JWT settings (secret key, issuer, audience) from configuration. The 'GenerateToken' method builds a list of claims (sub, jti, email, name, roles), creates a symmetric security key from the secret, defines signing credentials, and uses 'JwtSecurityTokenHandler' to create and write the token string.  
**Documentation:**
    
    - **Summary:** A concrete implementation of the token generation service, responsible for creating secure, signed access tokens for clients.
    
**Namespace:** Opc.System.Services.Authentication.Infrastructure.Identity  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services.Authentication/Infrastructure/ExternalProviders/OidcHandler.cs  
**Description:** Contains logic for handling OpenID Connect authentication flows with external Identity Providers.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** OidcHandler  
**Type:** Service  
**Relative Path:** Infrastructure/ExternalProviders  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** HandleExternalLoginCallbackAsync  
**Parameters:**
    
    
**Return Type:** Task<ApplicationUser>  
**Attributes:** public  
    
**Implemented Features:**
    
    - IdP Integration Handler
    
**Requirement Ids:**
    
    - REQ-3-004
    
**Purpose:** To manage the interaction with an external IdP after the user authenticates and is redirected back to the application.  
**Logic Description:** This class will be responsible for processing the callback from an external IdP. It retrieves the external identity claims (like subject id, email), checks if a local user exists for this external identity, and creates one if not (user provisioning). It then signs the user into the local system.  
**Documentation:**
    
    - **Summary:** Orchestrates the process of mapping an external identity from an IdP to a local application user.
    
**Namespace:** Opc.System.Services.Authentication.Infrastructure.ExternalProviders  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services.Authentication/Infrastructure/Services/UserMigrationService.cs  
**Description:** Implements the logic for user data migration strategies.  
**Template:** C# Service  
**Dependency Level:** 2  
**Name:** UserMigrationService  
**Type:** Service  
**Relative Path:** Infrastructure/Services  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** MigrateLocalUsersToIdpAsync  
**Parameters:**
    
    - IEnumerable<ApplicationUser> users
    - string idpName
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - User Data Migration
    
**Requirement Ids:**
    
    - REQ-3-016
    
**Purpose:** To provide concrete implementations for migrating user accounts, for example from the internal store to an external IdP.  
**Logic Description:** Contains methods to execute migration tasks. For example, 'MigrateLocalUsersToIdpAsync' would iterate through local users, create corresponding users in the external IdP via its API (if available), and then update the local 'ApplicationUser' record to link to the new external identity.  
**Documentation:**
    
    - **Summary:** A service that provides functionality to support user migration between different identity systems.
    
**Namespace:** Opc.System.Services.Authentication.Infrastructure.Services  
**Metadata:**
    
    - **Category:** Infrastructure
    
- **Path:** src/Services.Authentication/Api/Controllers/UsersController.cs  
**Description:** API endpoint for managing user accounts.  
**Template:** C# Controller  
**Dependency Level:** 3  
**Name:** UsersController  
**Type:** Controller  
**Relative Path:** Api/Controllers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** CreateUser  
**Parameters:**
    
    - CreateUserCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - User Management API
    
**Requirement Ids:**
    
    - REQ-3-005
    - REQ-9-001
    
**Purpose:** To expose user management functionality over HTTP, enabling creation, retrieval, update, and deletion of users.  
**Logic Description:** This class is an ASP.NET Core API Controller. Each action method (e.g., 'CreateUser') receives a request, maps it to a CQRS command or query, sends it to the MediatR pipeline, and returns the result as an appropriate HTTP response (e.g., 201 Created, 400 Bad Request). Endpoints are protected with authorization attributes.  
**Documentation:**
    
    - **Summary:** Provides the public RESTful API for all user-related operations.
    
**Namespace:** Opc.System.Services.Authentication.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Services.Authentication/Api/Controllers/RolesController.cs  
**Description:** API endpoint for managing roles and permissions.  
**Template:** C# Controller  
**Dependency Level:** 3  
**Name:** RolesController  
**Type:** Controller  
**Relative Path:** Api/Controllers  
**Repository Id:** REPO-SAP-007  
**Pattern Ids:**
    
    - Microservices
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** CreateRole  
**Parameters:**
    
    - CreateRoleCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** UpdatePermissionsForRole  
**Parameters:**
    
    - Guid roleId
    - UpdateRolePermissionsCommand command
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Role Management API
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-9-002
    
**Purpose:** To expose role and permission management functionality over HTTP.  
**Logic Description:** An ASP.NET Core API Controller that handles creating roles, listing roles, and updating the permissions associated with a role. It uses MediatR to dispatch commands/queries to the application layer. Endpoints require administrative privileges.  
**Documentation:**
    
    - **Summary:** Provides the public RESTful API for all role and permission-related operations.
    
**Namespace:** Opc.System.Services.Authentication.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableExternalIdpLogin
  - EnforceStrictPasswordPolicy
  - EnableUserMigrationFeatures
  
- **Database Configs:**
  
  - ConnectionStrings:AuthDatabase
  - JwtSettings:Key
  - JwtSettings:Issuer
  - JwtSettings:Audience
  - PasswordPolicy:RequiredLength
  - ExternalIdps:0:Scheme
  - ExternalIdps:0:Authority
  


---

