# Specification

# 1. Files

- **Path:** src/services.auth-service.csproj  
**Description:** The C# project file for the Authentication Service. Defines the target framework (.NET 8), project dependencies including ASP.NET Core Identity, JWT Bearer authentication, OIDC clients, and Entity Framework Core.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** services.auth-service  
**Type:** Project  
**Relative Path:** services.auth-service.csproj  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ProjectDefinition
    
**Requirement Ids:**
    
    
**Purpose:** Defines project metadata and dependencies required to build the authentication microservice.  
**Logic Description:** This file lists all NuGet packages like Microsoft.AspNetCore.Identity.EntityFrameworkCore, Microsoft.AspNetCore.Authentication.JwtBearer, and OIDC client libraries for external IdP integration. It also specifies the target framework as net8.0.  
**Documentation:**
    
    - **Summary:** The primary build artifact definition for the .NET compiler, listing all necessary packages and project settings.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Web/Program.cs  
**Description:** The main entry point for the Authentication Service application. This file configures and bootstraps the ASP.NET Core host, registers services in the DI container, and sets up the middleware pipeline.  
**Template:** C# Microservice Template  
**Dependency Level:** 4  
**Name:** Program  
**Type:** ApplicationEntry  
**Relative Path:** Web/Program.cs  
**Repository Id:** REPO-SAP-008  
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
    
    - ServiceBootstrap
    - MiddlewareConfiguration
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-3-004
    - REQ-9-001
    
**Purpose:** Initializes the web application, wires up all dependencies, and defines the request processing pipeline.  
**Logic Description:** Configures Kestrel web server. Reads configuration from appsettings.json. Calls extension methods to register application services (e.g., Identity, Authentication, Application services). Configures middleware for routing, authentication, authorization, and error handling. Runs the application.  
**Documentation:**
    
    - **Summary:** The composition root of the application, responsible for assembling the application's components and starting the web host.
    
**Namespace:** AuthService.Web  
**Metadata:**
    
    - **Category:** ApplicationHost
    
- **Path:** src/Web/appsettings.json  
**Description:** Primary configuration file for the service. Contains non-sensitive settings like logging levels, allowed hosts, and connection strings for local development. Sensitive settings are overridden by environment variables or a secret manager.  
**Template:** JSON Configuration  
**Dependency Level:** 0  
**Name:** appsettings  
**Type:** Configuration  
**Relative Path:** Web/appsettings.json  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    - ConfigurationManagement
    
**Requirement Ids:**
    
    - REQ-3-004
    
**Purpose:** Provides default and environment-agnostic configuration for the application.  
**Logic Description:** Defines JSON objects for 'ConnectionStrings', 'Logging', 'JwtSettings' (Issuer, Audience, Key), and 'ExternalIdpSettings' (e.g., AzureAd:ClientId, Authority). This structure is bound to strongly-typed configuration classes at startup.  
**Documentation:**
    
    - **Summary:** A JSON file containing configuration data for the application. It's the base of a hierarchical configuration system.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Web/Extensions/IdentityServiceExtensions.cs  
**Description:** Extension method for IServiceCollection to encapsulate the registration of ASP.NET Core Identity services, including user and role stores, password policies, and the DbContext.  
**Template:** C# Microservice Template  
**Dependency Level:** 3  
**Name:** IdentityServiceExtensions  
**Type:** ServiceExtension  
**Relative Path:** Web/Extensions/IdentityServiceExtensions.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddIdentityServices  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - InternalUserStoreSetup
    
**Requirement Ids:**
    
    - REQ-9-001
    - REQ-3-003
    
**Purpose:** To keep Program.cs clean by centralizing the setup logic for the internal user identity system.  
**Logic Description:** Configures the Entity Framework Core store for Identity using the connection string from IConfiguration. Adds Identity services for ApplicationUser and ApplicationRole. Configures password complexity requirements, lockout settings, and other security-related identity options.  
**Documentation:**
    
    - **Summary:** Provides an extension method to register and configure all services related to ASP.NET Core Identity.
    
**Namespace:** AuthService.Web.Extensions  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Web/Extensions/AuthenticationServiceExtensions.cs  
**Description:** Extension method for IServiceCollection to encapsulate the configuration of JWT Bearer authentication, setting up validation parameters like issuer, audience, and signing key.  
**Template:** C# Microservice Template  
**Dependency Level:** 3  
**Name:** AuthenticationServiceExtensions  
**Type:** ServiceExtension  
**Relative Path:** Web/Extensions/AuthenticationServiceExtensions.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddJwtAuthentication  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - JwtAuthenticationSetup
    
**Requirement Ids:**
    
    
**Purpose:** Centralizes JWT authentication setup, ensuring consistent token validation logic across the application.  
**Logic Description:** Reads JWT settings (Key, Issuer, Audience) from IConfiguration. Configures the JWT Bearer authentication handler with token validation parameters, including validating the issuer signing key, issuer, audience, and token lifetime.  
**Documentation:**
    
    - **Summary:** Provides an extension method to register and configure JWT Bearer authentication services.
    
**Namespace:** AuthService.Web.Extensions  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Web/Extensions/ExternalAuthServiceExtensions.cs  
**Description:** Extension method for IServiceCollection to configure OpenID Connect handlers for external identity providers like Azure AD, Okta, etc., based on settings in configuration.  
**Template:** C# Microservice Template  
**Dependency Level:** 3  
**Name:** ExternalAuthServiceExtensions  
**Type:** ServiceExtension  
**Relative Path:** Web/Extensions/ExternalAuthServiceExtensions.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - DependencyInjection
    
**Members:**
    
    
**Methods:**
    
    - **Name:** AddExternalIdentityProviders  
**Parameters:**
    
    - this IServiceCollection services
    - IConfiguration configuration
    
**Return Type:** IServiceCollection  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - ExternalIdPIntegration
    
**Requirement Ids:**
    
    - REQ-3-004
    - REQ-9-001
    
**Purpose:** To abstract and conditionally enable OIDC authentication providers based on application configuration.  
**Logic Description:** Checks configuration for enabled external providers. For each enabled provider (e.g., 'AzureAd'), it adds an OpenID Connect authentication handler, configuring it with the appropriate ClientId, Authority, and other required parameters from IConfiguration.  
**Documentation:**
    
    - **Summary:** Provides an extension method to register and configure authentication handlers for external OpenID Connect identity providers.
    
**Namespace:** AuthService.Web.Extensions  
**Metadata:**
    
    - **Category:** Configuration
    
- **Path:** src/Api/Controllers/AuthController.cs  
**Description:** API Controller for core authentication operations such as user login (credential exchange for a token), token refresh, and logout.  
**Template:** C# Microservice Template  
**Dependency Level:** 4  
**Name:** AuthController  
**Type:** Controller  
**Relative Path:** Api/Controllers/AuthController.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    - **Name:** _mediator  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Login  
**Parameters:**
    
    - LoginRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** RefreshToken  
**Parameters:**
    
    - RefreshTokenRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** Logout  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - UserLogin
    - TokenManagement
    
**Requirement Ids:**
    
    - REQ-9-001
    
**Purpose:** Exposes HTTP endpoints for client applications to authenticate users and manage tokens.  
**Logic Description:** Receives HTTP requests with DTOs (e.g., LoginRequest). Creates corresponding commands (e.g., LoginCommand) and dispatches them using MediatR. Maps the command handler's result to an appropriate HTTP response (e.g., 200 OK with token, 401 Unauthorized).  
**Documentation:**
    
    - **Summary:** Defines RESTful endpoints for user authentication and token lifecycle management.
    
**Namespace:** AuthService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Api/Controllers/UsersController.cs  
**Description:** API Controller for managing user accounts. Implements the UserManagementApiEndpoints component. Provides endpoints for creating, retrieving, updating, and deleting users.  
**Template:** C# Microservice Template  
**Dependency Level:** 4  
**Name:** UsersController  
**Type:** Controller  
**Relative Path:** Api/Controllers/UsersController.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    
**Methods:**
    
    - **Name:** CreateUser  
**Parameters:**
    
    - CreateUserRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetUserById  
**Parameters:**
    
    - string userId
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** AssignRoleToUser  
**Parameters:**
    
    - string userId
    - AssignRoleRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - UserManagement
    
**Requirement Ids:**
    
    - REQ-3-003
    
**Purpose:** Exposes administrative HTTP endpoints for user lifecycle and role management.  
**Logic Description:** Provides RESTful endpoints for user management. Endpoints are protected with authorization policies (e.g., require 'Administrator' role). It delegates the actual work to the application layer by sending commands or queries via MediatR.  
**Documentation:**
    
    - **Summary:** Defines RESTful endpoints for performing CRUD operations on user accounts and their role assignments.
    
**Namespace:** AuthService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Api/Controllers/RolesController.cs  
**Description:** API Controller for managing roles and permissions. Implements the RoleManagementApiEndpoints component. Provides endpoints for creating roles and assigning permissions to them.  
**Template:** C# Microservice Template  
**Dependency Level:** 4  
**Name:** RolesController  
**Type:** Controller  
**Relative Path:** Api/Controllers/RolesController.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Microservices Architecture
    
**Members:**
    
    
**Methods:**
    
    - **Name:** CreateRole  
**Parameters:**
    
    - CreateRoleRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetAllRoles  
**Parameters:**
    
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** AssignPermissionToRole  
**Parameters:**
    
    - string roleId
    - AssignPermissionRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - RoleManagement
    - PermissionManagement
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-9-001
    
**Purpose:** Exposes administrative HTTP endpoints for managing the RBAC system.  
**Logic Description:** Provides RESTful endpoints for managing roles and their associated permissions. Access is restricted to authorized administrators. All operations are delegated to the application layer through commands and queries.  
**Documentation:**
    
    - **Summary:** Defines RESTful endpoints for creating roles and managing the permissions granted to each role.
    
**Namespace:** AuthService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** src/Application/Features/Authentication/Commands/Login/LoginCommand.cs  
**Description:** Represents the command to authenticate a user. It contains the user's credentials. This is part of the CQS/CQRS pattern implementation.  
**Template:** C# Microservice Template  
**Dependency Level:** 1  
**Name:** LoginCommand  
**Type:** Command  
**Relative Path:** Application/Features/Authentication/Commands/Login/LoginCommand.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** Username  
**Type:** string  
**Attributes:** public  
    - **Name:** Password  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To encapsulate all necessary data to perform a user login operation.  
**Logic Description:** A plain C# record or class that implements IRequest<TokenResponse> from MediatR. It holds the username and password provided by the user.  
**Documentation:**
    
    - **Summary:** A data structure representing a request to log in, containing the user's credentials.
    
**Namespace:** AuthService.Application.Features.Authentication.Commands.Login  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Application/Features/Authentication/Commands/Login/LoginCommandHandler.cs  
**Description:** The handler for the LoginCommand. It contains the business logic to validate credentials, and if successful, generate JWTs.  
**Template:** C# Microservice Template  
**Dependency Level:** 2  
**Name:** LoginCommandHandler  
**Type:** CommandHandler  
**Relative Path:** Application/Features/Authentication/Commands/Login/LoginCommandHandler.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _userManager  
**Type:** UserManager<ApplicationUser>  
**Attributes:** private|readonly  
    - **Name:** _tokenService  
**Type:** ITokenService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - LoginCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<TokenResponse>  
**Attributes:** public  
    
**Implemented Features:**
    
    - CredentialValidation
    - TokenIssuance
    
**Requirement Ids:**
    
    - REQ-9-001
    
**Purpose:** To process a login request, validate the user, and return authentication tokens.  
**Logic Description:** Uses UserManager to find the user by username. Validates the provided password using UserManager.CheckPasswordAsync. If validation is successful, it calls the ITokenService to generate an access token and a refresh token. Returns a TokenResponse DTO with the tokens.  
**Documentation:**
    
    - **Summary:** Contains the core logic for handling a user login request. It validates credentials and orchestrates token generation.
    
**Namespace:** AuthService.Application.Features.Authentication.Commands.Login  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Application/Services/ITokenService.cs  
**Description:** Interface for the token generation service, abstracting the creation of JWT access tokens and refresh tokens.  
**Template:** C# Microservice Template  
**Dependency Level:** 1  
**Name:** ITokenService  
**Type:** Interface  
**Relative Path:** Application/Services/ITokenService.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Interface
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GenerateAccessToken  
**Parameters:**
    
    - ApplicationUser user
    
**Return Type:** Task<string>  
**Attributes:** public  
    - **Name:** GenerateRefreshToken  
**Parameters:**
    
    
**Return Type:** string  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To define a contract for creating security tokens, decoupling the token creation logic from its consumers.  
**Logic Description:** Defines methods for generating different types of tokens. The GenerateAccessToken method takes a user object to include user-specific claims in the token.  
**Documentation:**
    
    - **Summary:** An abstraction for services that handle the creation of security tokens.
    
**Namespace:** AuthService.Application.Services  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Application/Services/TokenService.cs  
**Description:** Implementation of ITokenService. Contains the logic for building and signing JWTs using settings from configuration.  
**Template:** C# Microservice Template  
**Dependency Level:** 2  
**Name:** TokenService  
**Type:** Service  
**Relative Path:** Application/Services/TokenService.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _configuration  
**Type:** IConfiguration  
**Attributes:** private|readonly  
    - **Name:** _userManager  
**Type:** UserManager<ApplicationUser>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateAccessToken  
**Parameters:**
    
    - ApplicationUser user
    
**Return Type:** Task<string>  
**Attributes:** public  
    
**Implemented Features:**
    
    - JwtGeneration
    
**Requirement Ids:**
    
    
**Purpose:** To create and sign JSON Web Tokens for authenticated users.  
**Logic Description:** Reads JWT settings (secret key, issuer, audience, expiration) from IConfiguration. Fetches user roles and claims using UserManager. Creates a list of claims (sub, jti, email, roles, etc.). Creates a SymmetricSecurityKey from the secret. Defines a signing credential using the key and HmacSha256Signature. Creates and returns a signed token using JwtSecurityTokenHandler.  
**Documentation:**
    
    - **Summary:** Implements the token generation logic, responsible for constructing and signing JWTs with the correct claims and expiration.
    
**Namespace:** AuthService.Application.Services  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Application/External/Idp/IIdpIntegrationService.cs  
**Description:** Interface for handling the logic after a user successfully authenticates with an external Identity Provider.  
**Template:** C# Microservice Template  
**Dependency Level:** 1  
**Name:** IIdpIntegrationService  
**Type:** Interface  
**Relative Path:** Application/External/Idp/IIdpIntegrationService.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Interface
    
**Members:**
    
    
**Methods:**
    
    - **Name:** ProcessExternalLogin  
**Parameters:**
    
    - AuthenticateResult externalAuthResult
    
**Return Type:** Task<TokenResponse>  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To define a contract for processing external authentication results and provisioning local users.  
**Logic Description:** Defines a single method that takes the result from an external OIDC authentication flow. This service is responsible for finding or creating a local user linked to the external identity and then issuing the application's own JWT for that user.  
**Documentation:**
    
    - **Summary:** An abstraction for a service that handles user provisioning and token issuance following a successful external login.
    
**Namespace:** AuthService.Application.External.Idp  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Application/External/Idp/IdpIntegrationService.cs  
**Description:** Implementation of IIdpIntegrationService. Implements the IdpIntegrationHandler component.  
**Template:** C# Microservice Template  
**Dependency Level:** 2  
**Name:** IdpIntegrationService  
**Type:** Service  
**Relative Path:** Application/External/Idp/IdpIntegrationService.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _userManager  
**Type:** UserManager<ApplicationUser>  
**Attributes:** private|readonly  
    - **Name:** _tokenService  
**Type:** ITokenService  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ProcessExternalLogin  
**Parameters:**
    
    - AuthenticateResult externalAuthResult
    
**Return Type:** Task<TokenResponse>  
**Attributes:** public  
    
**Implemented Features:**
    
    - JustInTimeUserProvisioning
    
**Requirement Ids:**
    
    - REQ-3-004
    
**Purpose:** To link external identities with local user accounts and issue session tokens.  
**Logic Description:** Extracts claims (e.g., email, name identifier) from the external authentication result. Uses UserManager.FindByLoginAsync to check if a user with this external login already exists. If not, it creates a new ApplicationUser and links the external login using UserManager.AddLoginAsync. If a user exists or is created, it calls ITokenService to generate application-specific JWTs for the user.  
**Documentation:**
    
    - **Summary:** Contains the logic to find or create a local user corresponding to an external identity and then generate internal application tokens.
    
**Namespace:** AuthService.Application.External.Idp  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** src/Domain/Entities/ApplicationUser.cs  
**Description:** Domain entity representing a user in the system. It extends the base IdentityUser class to allow for custom properties if needed in the future.  
**Template:** C# Microservice Template  
**Dependency Level:** 0  
**Name:** ApplicationUser  
**Type:** Entity  
**Relative Path:** Domain/Entities/ApplicationUser.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** FullName  
**Type:** string?  
**Attributes:** public  
    - **Name:** IsActive  
**Type:** bool  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - UserRepresentation
    
**Requirement Ids:**
    
    - REQ-3-003
    - REQ-9-001
    
**Purpose:** To model a system user, including their authentication details and custom profile information.  
**Logic Description:** Inherits from Microsoft.AspNetCore.Identity.IdentityUser<Guid>. This provides standard properties like Id, UserName, Email, PasswordHash. Custom properties like FullName or IsActive can be added here.  
**Documentation:**
    
    - **Summary:** Represents a user account within the application's domain, extending the functionality of the default ASP.NET Core Identity user.
    
**Namespace:** AuthService.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Domain/Entities/ApplicationRole.cs  
**Description:** Domain entity representing a role in the RBAC system. It extends the base IdentityRole to add a description.  
**Template:** C# Microservice Template  
**Dependency Level:** 0  
**Name:** ApplicationRole  
**Type:** Entity  
**Relative Path:** Domain/Entities/ApplicationRole.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - Entity
    
**Members:**
    
    - **Name:** Description  
**Type:** string?  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    - RoleRepresentation
    
**Requirement Ids:**
    
    - REQ-3-003
    
**Purpose:** To model a user role, which is a collection of permissions.  
**Logic Description:** Inherits from Microsoft.AspNetCore.Identity.IdentityRole<Guid>. Adds a Description property to provide more context for what the role entails.  
**Documentation:**
    
    - **Summary:** Represents a user role within the application's RBAC system.
    
**Namespace:** AuthService.Domain.Entities  
**Metadata:**
    
    - **Category:** Domain
    
- **Path:** src/Infrastructure/Persistence/AuthDbContext.cs  
**Description:** The Entity Framework Core DbContext for the service. It defines the database schema for identity tables (Users, Roles, Claims, etc.) by inheriting from IdentityDbContext.  
**Template:** C# Microservice Template  
**Dependency Level:** 1  
**Name:** AuthDbContext  
**Type:** DbContext  
**Relative Path:** Infrastructure/Persistence/AuthDbContext.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    - UnitOfWork
    
**Members:**
    
    
**Methods:**
    
    - **Name:** OnModelCreating  
**Parameters:**
    
    - ModelBuilder builder
    
**Return Type:** void  
**Attributes:** protected|override  
    
**Implemented Features:**
    
    - DataAccess
    - DatabaseSchemaDefinition
    
**Requirement Ids:**
    
    
**Purpose:** To manage the database connection and map domain entities to database tables for persistence.  
**Logic Description:** Inherits from IdentityDbContext<ApplicationUser, ApplicationRole, Guid>. In the constructor, it accepts DbContextOptions. The OnModelCreating method is used to apply any custom entity configurations and to seed initial data like default roles.  
**Documentation:**
    
    - **Summary:** Represents a session with the database, allowing querying and saving of identity-related data.
    
**Namespace:** AuthService.Infrastructure.Persistence  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** src/Infrastructure/Persistence/Seed/DefaultRolesAndPermissionsSeeder.cs  
**Description:** A utility class to seed the database with predefined default roles (Administrator, Engineer, Operator, Viewer) as required by the business logic.  
**Template:** C# Microservice Template  
**Dependency Level:** 2  
**Name:** DefaultRolesAndPermissionsSeeder  
**Type:** Seeder  
**Relative Path:** Infrastructure/Persistence/Seed/DefaultRolesAndPermissionsSeeder.cs  
**Repository Id:** REPO-SAP-008  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** SeedRolesAsync  
**Parameters:**
    
    - RoleManager<ApplicationRole> roleManager
    
**Return Type:** Task  
**Attributes:** public|static  
    
**Implemented Features:**
    
    - DatabaseSeeding
    
**Requirement Ids:**
    
    - REQ-3-003
    
**Purpose:** To ensure that essential, predefined roles are available in the system upon initial setup.  
**Logic Description:** Contains a static method that accepts a RoleManager. It defines a list of default role names. For each name, it checks if the role already exists using roleManager.RoleExistsAsync. If it doesn't exist, it creates the new ApplicationRole using roleManager.CreateAsync.  
**Documentation:**
    
    - **Summary:** Populates the database with a set of default roles required for the application to function correctly.
    
**Namespace:** AuthService.Infrastructure.Persistence.Seed  
**Metadata:**
    
    - **Category:** DataAccess
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableAzureAdLogin
  - EnableOktaLogin
  - EnableInternalRegistration
  
- **Database Configs:**
  
  - ConnectionStrings:AuthServiceDb
  


---

