# Software Design Specification (SDS): services.auth-service

## 1. Introduction

### 1.1. Purpose
This document provides a detailed software design specification for the **Authentication Service (`services.auth-service`)**. This microservice is the central authority for all authentication and authorization concerns within the system. It is responsible for managing user identities, roles, and permissions, integrating with external identity providers (IdPs), and issuing JSON Web Tokens (JWTs) to secure the microservices ecosystem.

### 1.2. Scope
The scope of this document is limited to the `services.auth-service` repository. It covers the design of its RESTful API, internal application logic, domain model, data persistence, and integration points for security. It is designed to fulfill the requirements `REQ-3-003`, `REQ-3-004`, and `REQ-9-001`.

## 2. System Overview

The Authentication Service is a .NET 8 microservice built on ASP.NET Core, employing ASP.NET Core Identity for user management. It follows the principles of Clean Architecture to ensure separation of concerns, maintainability, and testability.

### 2.1. Key Responsibilities
- **User Authentication:** Validating user credentials for both internal and external identity systems.
- **Token Issuance:** Generating, validating, and refreshing JWTs for authenticated users.
- **User & Role Management:** Providing administrative capabilities to manage users, roles, and their assignments.
- **Permission Management:** Managing permissions as claims associated with roles (RBAC).
- **External IdP Integration:** Acting as a client for OAuth 2.0/OpenID Connect (OIDC) providers like Azure AD and Okta, enabling Single Sign-On (SSO).

### 2.2. Technology Stack
- **Language:** C# 12
- **Framework:** .NET 8, ASP.NET Core 8
- **ORM:** Entity Framework Core 8
- **Identity Management:** ASP.NET Core Identity
- **Authentication:** JWT Bearer Tokens, OpenID Connect
- **Database:** PostgreSQL (or other EF Core compatible DB)

## 3. Architectural Design

The service implements a Clean Architecture style, logically separating the project into distinct layers.

- **Domain:** Contains the core business entities (`ApplicationUser`, `ApplicationRole`). It has no external dependencies.
- **Application:** Contains the business logic, services, commands, queries (CQRS with MediatR), and interfaces. It depends only on the Domain layer.
- **Infrastructure:** Contains implementations of application layer interfaces, primarily for data access (EF Core `AuthDbContext`) and external services.
- **Web (API):** The entry point of the application. Contains API controllers, middleware, and startup configuration. It depends on all other layers.

This design promotes loose coupling and high cohesion, making the system easier to test, maintain, and evolve.

## 4. Data Model

The data model is built upon ASP.NET Core Identity and persisted using Entity Framework Core.

### 4.1. Entities
- **`ApplicationUser` (`Domain/Entities/ApplicationUser.cs`)**:
  - Inherits from `Microsoft.AspNetCore.Identity.IdentityUser<Guid>`. Using `Guid` as the primary key type.
  - Custom Properties:
    - `public string? FullName { get; set; }`
    - `public bool IsActive { get; set; } = true;`

- **`ApplicationRole` (`Domain/Entities/ApplicationRole.cs`)**:
  - Inherits from `Microsoft.AspNetCore.Identity.IdentityRole<Guid>`.
  - Custom Properties:
    - `public string? Description { get; set; }`

### 4.2. Database Context
- **`AuthDbContext` (`Infrastructure/Persistence/AuthDbContext.cs`)**:
  - Inherits from `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`.
  - It is responsible for mapping these entities to the database schema.
  - The `OnModelCreating` method will be overridden to apply any custom configurations and to call the database seeder for default roles.

## 5. Core Features and Implementation Details

### 5.1. Startup and Configuration (`Web/Program.cs`)
The application's entry point will orchestrate the configuration of all services and middleware.
csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Register Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// ... other configurations

// 2. Add Infrastructure & Application Services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// 3. Add Identity, Authentication & Authorization
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddExternalIdentityProviders(builder.Configuration); // For OIDC
builder.Services.AddAuthorization();

// 4. Add Web/API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // With JWT support configuration

// ... Middleware Pipeline Configuration ...
var app = builder.Build();

// Seed database with default roles
app.SeedDatabase(); 

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


### 5.2. Internal Authentication and JWT Generation

#### 5.2.1. Login Flow (`Api/Controllers/AuthController.cs`)
- **Endpoint:** `POST /api/auth/login`
- **Request DTO:** `LoginRequest(string Username, string Password)`
- **Logic:**
  1. The controller receives the `LoginRequest`.
  2. It dispatches a `LoginCommand` via MediatR.
  3. The `LoginCommandHandler` uses `UserManager<ApplicationUser>` to find the user and `CheckPasswordAsync` to validate credentials.
  4. If successful, it calls `ITokenService.GenerateAccessToken(user)` to create a JWT.
  5. It returns a `TokenResponse(string AccessToken, DateTime ExpiresAt)` DTO.

#### 5.2.2. Token Service (`Application/Services/TokenService.cs`)
- **Interface:** `ITokenService`
- **Implementation:** `TokenService`
- **`GenerateAccessToken(ApplicationUser user)` Logic:**
  1. Fetch user roles using `_userManager.GetRolesAsync(user)`.
  2. Fetch user claims using `_userManager.GetClaimsAsync(user)`.
  3. Create a list of `Claim` objects, including `JwtRegisteredClaimNames.Sub` (user ID), `JwtRegisteredClaimNames.Email`, `JwtRegisteredClaimNames.Jti` (unique token ID), and a `ClaimTypes.Role` for each role.
  4. Read JWT settings (`Secret`, `Issuer`, `Audience`, `ExpiryInMinutes`) from `IOptions<JwtSettings>`.
  5. Create `SymmetricSecurityKey` from the secret.
  6. Create `SigningCredentials`.
  7. Create a `JwtSecurityToken` with all claims, issuer, audience, expiration, and signing credentials.
  8. Use `JwtSecurityTokenHandler` to write the token into a string.
  9. Return the token string.

### 5.3. External Identity Provider Integration (OIDC)

#### 5.3.1. Configuration (`Web/Extensions/ExternalAuthServiceExtensions.cs`)
- This extension method will read from `appsettings.json` to conditionally register OIDC providers.
- **Example `appsettings.json` section:**
  json
  "ExternalIdpSettings": {
    "AzureAd": {
      "Enabled": true,
      "ClientId": "...",
      "TenantId": "...",
      "Authority": "https://login.microsoftonline.com/{TenantId}"
    },
    "Okta": {
      "Enabled": false,
      // ... Okta specific settings
    }
  }
  
- **Logic:** The `AddExternalIdentityProviders` method will iterate through the configured providers. If `Enabled` is true, it will call `services.AddAuthentication().AddOpenIdConnect(...)` with the specific provider's settings.

#### 5.3.2. Authentication Flow
- **Endpoint:** `GET /api/external-auth/login/{provider}` (e.g., `/api/external-auth/login/AzureAd`)
- **Logic:** This endpoint initiates the OIDC challenge, redirecting the user to the external IdP's login page.
- **Callback Endpoint:** `GET /api/external-auth/callback`
- **Logic:**
  1. After successful external login, the IdP redirects back to this endpoint.
  2. The OIDC handler processes the response.
  3. The controller calls `IIdpIntegrationService.ProcessExternalLogin()`.
  4. `IdpIntegrationService` (`Application/External/Idp/IdpIntegrationService.cs`):
     - Finds the user via `_userManager.FindByLoginAsync(provider, providerKey)`.
     - If the user does not exist (first-time login), it creates a new `ApplicationUser` using claims from the external token (email, name) and links the external login via `_userManager.AddLoginAsync(user, info)`. This is Just-In-Time (JIT) provisioning.
     - Calls `ITokenService` to issue the application's internal JWT for the found/created user.
  5. The client application receives the internal JWT and uses it for all subsequent API calls, ensuring a consistent authorization model for all microservices.

### 5.4. RBAC and User/Role Management

#### 5.4.1. Seeding Default Roles (`Infrastructure/Persistence/Seed/DefaultRolesAndPermissionsSeeder.cs`)
- This static class will be called at application startup.
- It will ensure the following roles exist, creating them if they don't:
  - `Administrator`
  - `Engineer`
  - `Operator`
  - `Viewer`
- It will use `RoleManager<ApplicationRole>` to perform these checks and creations.

#### 5.4.2. User Management API (`Api/Controllers/UsersController.cs`)
- All endpoints will be protected with `[Authorize(Roles = "Administrator")]`.
- **`POST /api/users`**: Creates a new user using `UserManager.CreateAsync`.
- **`GET /api/users/{id}`**: Retrieves a user by their ID.
- **`POST /api/users/{id}/roles`**: Assigns a role to a user using `UserManager.AddToRoleAsync`.

#### 5.4.3. Role Management API (`Api/Controllers/RolesController.cs`)
- All endpoints will be protected with `[Authorize(Roles = "Administrator")]`.
- **`POST /api/roles`**: Creates a new role using `RoleManager.CreateAsync`.
- **`GET /api/roles`**: Lists all available roles.
- **`POST /api/roles/{roleName}/claims`**: Assigns a permission (as a claim) to a role using `RoleManager.AddClaimAsync`.

## 6. API Specification

| Endpoint | Verb | Description | Request Body | Response Body | Authorization |
|---|---|---|---|---|---|
| `/api/auth/login` | POST | Authenticates an internal user and returns a JWT. | `LoginRequest` | `TokenResponse` | Anonymous |
| `/api/external-auth/login/{provider}` | GET | Initiates OIDC login flow. | - | 302 Redirect | Anonymous |
| `/api/external-auth/callback` | GET | OIDC callback endpoint. | - | `TokenResponse` | Anonymous |
| `/api/users` | POST | Creates a new user. | `CreateUserRequest` | `UserDto` | Administrator |
| `/api/users/{id}` | GET | Gets a user by ID. | - | `UserDto` | Administrator |
| `/api/users/{id}/roles` | POST | Assigns a role to a user. | `AssignRoleRequest` | 204 No Content | Administrator |
| `/api/roles` | POST | Creates a new role. | `CreateRoleRequest` | `RoleDto` | Administrator |
| `/api/roles` | GET | Lists all roles. | - | `List<RoleDto>` | Administrator |

## 7. Error Handling

A custom exception handling middleware will be implemented to catch all unhandled exceptions.
- It will log the full exception details.
- It will return a standardized JSON error response to the client.
- **Example Response (`500 Internal Server Error`):**
  json
  {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "An unexpected error occurred.",
    "status": 500,
    "traceId": "..." 
  }
  
- For validation errors (e.g., from FluentValidation), it will return a `400 Bad Request` with a detailed list of validation failures.
- For authentication/authorization failures, the framework will automatically return `401 Unauthorized` or `403 Forbidden`.