# Specification

# 1. Files

- **Path:** services/licensing-service/LicensingService.sln  
**Description:** Visual Studio Solution file for the Licensing Service, grouping all related projects.  
**Template:** C# Solution File  
**Dependency Level:** 0  
**Name:** LicensingService  
**Type:** Solution  
**Relative Path:**   
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the collection of projects that constitute the Licensing Service microservice.  
**Logic Description:**   
**Documentation:**
    
    - **Summary:** The solution file that ties together the Api, Application, Domain, and Infrastructure projects for the microservice.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/licensing-service/src/LicensingService.Api/LicensingService.Api.csproj  
**Description:** The project file for the API layer, defining dependencies on other projects and NuGet packages for ASP.NET Core.  
**Template:** C# Project File  
**Dependency Level:** 3  
**Name:** LicensingService.Api  
**Type:** Project  
**Relative Path:** src/LicensingService.Api  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the build configuration and dependencies for the API project.  
**Logic Description:** This project will reference the LicensingService.Application and LicensingService.Infrastructure projects to wire up dependencies and services.  
**Documentation:**
    
    - **Summary:** Manages the compilation and dependencies for the presentation layer of the microservice.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/licensing-service/src/LicensingService.Api/Program.cs  
**Description:** The main entry point for the Licensing Service application. Configures the ASP.NET Core host, registers services for dependency injection, and sets up the HTTP request pipeline.  
**Template:** C# ASP.NET Core Entry Point  
**Dependency Level:** 4  
**Name:** Program  
**Type:** ApplicationEntry  
**Relative Path:** src/LicensingService.Api  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** Main  
**Parameters:**
    
    - string[] args
    
**Return Type:** void  
**Attributes:** public static  
    
**Implemented Features:**
    
    - Dependency Injection Setup
    - Middleware Configuration
    - API Endpoint Routing
    
**Requirement Ids:**
    
    - REQ-9-006
    - REQ-9-007
    - REQ-9-008
    
**Purpose:** Initializes and runs the web application, configuring all services and middleware.  
**Logic Description:** This file will bootstrap the application. It will call extension methods from the Application and Infrastructure layers to register their services. It will configure middleware for exception handling, authentication, authorization, and routing. It will also set up Swagger/OpenAPI for API documentation.  
**Documentation:**
    
    - **Summary:** The bootstrap file for the microservice. It sets up the dependency injection container, the HTTP pipeline, and starts the Kestrel web server.
    
**Namespace:** LicensingService.Api  
**Metadata:**
    
    - **Category:** ApplicationHost
    
- **Path:** services/licensing-service/src/LicensingService.Api/Controllers/LicensesController.cs  
**Description:** Exposes public-facing REST endpoints for license activation and validation.  
**Template:** C# ASP.NET Core Controller  
**Dependency Level:** 4  
**Name:** LicensesController  
**Type:** Controller  
**Relative Path:** src/LicensingService.Api/Controllers  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - REST
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** ActivateOnline  
**Parameters:**
    
    - ActivateLicenseRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** Validate  
**Parameters:**
    
    - string licenseKey
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** CheckFeatureEntitlement  
**Parameters:**
    
    - string licenseKey
    - string featureCode
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GenerateOfflineActivationRequest  
**Parameters:**
    
    - OfflineActivationRequestDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** CompleteOfflineActivation  
**Parameters:**
    
    - CompleteOfflineActivationRequestDto request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - Online License Activation
    - License Validation
    - Feature Entitlement Check
    - Offline Activation Flow
    
**Requirement Ids:**
    
    - REQ-9-006
    - REQ-9-007
    - REQ-9-008
    
**Purpose:** Handles HTTP requests related to standard license operations.  
**Logic Description:** This controller will be lean. Each method will create a command or query object using the request data and send it to the MediatR pipeline using the ISender interface. It will then translate the result from the handler into an appropriate HTTP response (e.g., Ok, NotFound, BadRequest).  
**Documentation:**
    
    - **Summary:** Provides API endpoints for activating, validating, and querying licenses and their feature entitlements.
    
**Namespace:** LicensingService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** services/licensing-service/src/LicensingService.Api/Controllers/AdminController.cs  
**Description:** Exposes administrative REST endpoints for license generation and management.  
**Template:** C# ASP.NET Core Controller  
**Dependency Level:** 4  
**Name:** AdminController  
**Type:** Controller  
**Relative Path:** src/LicensingService.Api/Controllers  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - REST
    
**Members:**
    
    - **Name:** _sender  
**Type:** ISender  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateLicense  
**Parameters:**
    
    - GenerateLicenseRequest request
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** RevokeLicense  
**Parameters:**
    
    - string licenseKey
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    - **Name:** GetLicenseDetails  
**Parameters:**
    
    - string licenseKey
    
**Return Type:** Task<IActionResult>  
**Attributes:** public  
    
**Implemented Features:**
    
    - License Generation
    - License Revocation
    
**Requirement Ids:**
    
    - REQ-9-007
    
**Purpose:** Handles secure, administrative HTTP requests for managing the lifecycle of licenses.  
**Logic Description:** This controller will be protected by an authorization policy restricting access to administrators. Similar to the LicensesController, it will delegate all business logic to the application layer by creating and sending commands/queries via MediatR.  
**Documentation:**
    
    - **Summary:** Provides administrative API endpoints for creating, revoking, and inspecting licenses. Access is restricted to authorized personnel.
    
**Namespace:** LicensingService.Api.Controllers  
**Metadata:**
    
    - **Category:** Presentation
    
- **Path:** services/licensing-service/src/LicensingService.Application/LicensingService.Application.csproj  
**Description:** Project file for the Application layer, defining its dependencies, including MediatR.  
**Template:** C# Project File  
**Dependency Level:** 2  
**Name:** LicensingService.Application  
**Type:** Project  
**Relative Path:** src/LicensingService.Application  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the build configuration and dependencies for the application logic project.  
**Logic Description:** This project will reference the LicensingService.Domain project. It will contain no references to infrastructure concerns like EF Core or specific web frameworks.  
**Documentation:**
    
    - **Summary:** Manages the compilation and dependencies for the application layer, which contains the use case logic.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/licensing-service/src/LicensingService.Application/Features/Licenses/Commands/ActivateLicense/ActivateLicenseCommand.cs  
**Description:** Represents the command to activate a license key with associated metadata.  
**Template:** C# CQRS Command  
**Dependency Level:** 2  
**Name:** ActivateLicenseCommand  
**Type:** Command  
**Relative Path:** src/LicensingService.Application/Features/Licenses/Commands/ActivateLicense  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** LicenseKey  
**Type:** string  
**Attributes:** public  
    - **Name:** ActivationMetadata  
**Type:** Dictionary<string, string>  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-9-007
    
**Purpose:** A data-carrying object that encapsulates all necessary information to perform the license activation use case.  
**Logic Description:** This is a simple record or class that implements MediatR's IRequest<TResponse> interface. It holds the license key and any metadata required for activation, such as a machine ID or user ID.  
**Documentation:**
    
    - **Summary:** Defines the input parameters for the use case of activating a license.
    
**Namespace:** LicensingService.Application.Features.Licenses.Commands.ActivateLicense  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Application/Features/Licenses/Commands/ActivateLicense/ActivateLicenseCommandHandler.cs  
**Description:** Handles the logic for the ActivateLicenseCommand use case.  
**Template:** C# CQRS Command Handler  
**Dependency Level:** 3  
**Name:** ActivateLicenseCommandHandler  
**Type:** CommandHandler  
**Relative Path:** src/LicensingService.Application/Features/Licenses/Commands/ActivateLicense  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _licenseRepository  
**Type:** ILicenseRepository  
**Attributes:** private|readonly  
    - **Name:** _unitOfWork  
**Type:** IUnitOfWork  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ActivateLicenseCommand request
    - CancellationToken cancellationToken
    
**Return Type:** Task<ValidationResultDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - License Activation Logic
    
**Requirement Ids:**
    
    - REQ-9-007
    
**Purpose:** Orchestrates the activation of a license by interacting with the domain model and persistence.  
**Logic Description:** The handler will first retrieve the license aggregate from the repository using the license key. It will then call the Activate method on the domain object, passing the activation metadata. The domain object will enforce all business rules (e.g., check if already activated, check expiry). Finally, the handler will save the changes to the database via the unit of work.  
**Documentation:**
    
    - **Summary:** Implements the core workflow for activating a license, including finding the license, invoking domain logic, and persisting the result.
    
**Namespace:** LicensingService.Application.Features.Licenses.Commands.ActivateLicense  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Application/Features/Licenses/Queries/ValidateLicense/ValidateLicenseQuery.cs  
**Description:** Represents the query to validate a license and get its status and entitlements.  
**Template:** C# CQRS Query  
**Dependency Level:** 2  
**Name:** ValidateLicenseQuery  
**Type:** Query  
**Relative Path:** src/LicensingService.Application/Features/Licenses/Queries/ValidateLicense  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** LicenseKey  
**Type:** string  
**Attributes:** public  
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-9-007
    - REQ-9-008
    
**Purpose:** A data-carrying object that encapsulates the information needed to perform the license validation use case.  
**Logic Description:** This is a simple record or class implementing IRequest<TResponse> for MediatR, carrying the license key to be validated.  
**Documentation:**
    
    - **Summary:** Defines the input parameters for the use case of validating a license.
    
**Namespace:** LicensingService.Application.Features.Licenses.Queries.ValidateLicense  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Application/Features/Licenses/Queries/ValidateLicense/ValidateLicenseQueryHandler.cs  
**Description:** Handles the logic for the ValidateLicenseQuery use case, including grace period logic.  
**Template:** C# CQRS Query Handler  
**Dependency Level:** 3  
**Name:** ValidateLicenseQueryHandler  
**Type:** QueryHandler  
**Relative Path:** src/LicensingService.Application/Features/Licenses/Queries/ValidateLicense  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - CQRS
    
**Members:**
    
    - **Name:** _licenseRepository  
**Type:** ILicenseRepository  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Handle  
**Parameters:**
    
    - ValidateLicenseQuery request
    - CancellationToken cancellationToken
    
**Return Type:** Task<LicenseValidationDto>  
**Attributes:** public  
    
**Implemented Features:**
    
    - License Validation Logic
    - Grace Period Handling
    
**Requirement Ids:**
    
    - REQ-9-007
    - REQ-9-008
    
**Purpose:** Retrieves license details and determines its validity, applying grace period rules if necessary.  
**Logic Description:** The handler will fetch the license from the repository. It will then call a validation method on the domain object. This domain method will check the status, expiration date, etc. If the license is valid, it updates the LastValidatedOn timestamp. If it's invalid (e.g., expired), but within the grace period, it returns a valid status with a grace period warning. Otherwise, it returns an invalid status.  
**Documentation:**
    
    - **Summary:** Implements the logic to check a license's validity. It accounts for status, expiration, and any applicable grace periods for temporary validation failures.
    
**Namespace:** LicensingService.Application.Features.Licenses.Queries.ValidateLicense  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Application/Contracts/Persistence/ILicenseRepository.cs  
**Description:** Defines the contract for data access operations related to the License aggregate.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** ILicenseRepository  
**Type:** RepositoryInterface  
**Relative Path:** src/LicensingService.Application/Contracts/Persistence  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GetByKeyAsync  
**Parameters:**
    
    - LicenseKey licenseKey
    - CancellationToken cancellationToken
    
**Return Type:** Task<License?>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - License license
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - License license
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** To abstract the persistence mechanism from the application and domain layers, allowing for easier testing and technology swapping.  
**Logic Description:** This interface defines the methods needed by the application layer to interact with the persistence store for License aggregates. It adheres to the Dependency Inversion Principle.  
**Documentation:**
    
    - **Summary:** An abstraction for the data storage of License entities, defining the CRUD and query operations required by the application.
    
**Namespace:** LicensingService.Application.Contracts.Persistence  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/licensing-service/src/LicensingService.Application/Contracts/Infrastructure/ILicenseKeyGenerator.cs  
**Description:** Defines the contract for a service that generates new, cryptographically secure license keys.  
**Template:** C# Interface  
**Dependency Level:** 1  
**Name:** ILicenseKeyGenerator  
**Type:** ServiceInterface  
**Relative Path:** src/LicensingService.Application/Contracts/Infrastructure  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    - **Name:** GenerateKey  
**Parameters:**
    
    - GenerateLicenseCommand command
    
**Return Type:** LicenseKey  
**Attributes:** public  
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-9-007
    
**Purpose:** To abstract the specific algorithm for license key generation from the application logic.  
**Logic Description:** This interface defines a single method responsible for creating a new LicenseKey value object based on the details of the license to be created.  
**Documentation:**
    
    - **Summary:** An abstraction for the service that creates new license keys.
    
**Namespace:** LicensingService.Application.Contracts.Infrastructure  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Domain/LicensingService.Domain.csproj  
**Description:** Project file for the Domain layer, containing no external dependencies other than the .NET standard library.  
**Template:** C# Project File  
**Dependency Level:** 0  
**Name:** LicensingService.Domain  
**Type:** Project  
**Relative Path:** src/LicensingService.Domain  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the build configuration for the domain model project.  
**Logic Description:** This project is the core of the service and will have zero dependencies on web frameworks, databases, or any other infrastructure concerns.  
**Documentation:**
    
    - **Summary:** Manages the compilation of the domain layer, which contains all core business entities, value objects, and rules.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/licensing-service/src/LicensingService.Domain/Aggregates/License.cs  
**Description:** The License aggregate root. Represents a single software license and enforces its lifecycle rules.  
**Template:** C# DDD Aggregate Root  
**Dependency Level:** 1  
**Name:** License  
**Type:** Aggregate  
**Relative Path:** src/LicensingService.Domain/Aggregates  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - DDD-Aggregate
    
**Members:**
    
    - **Name:** Id  
**Type:** Guid  
**Attributes:** public  
    - **Name:** Key  
**Type:** LicenseKey  
**Attributes:** public  
    - **Name:** Status  
**Type:** LicenseStatus  
**Attributes:** public  
    - **Name:** Type  
**Type:** LicenseType  
**Attributes:** public  
    - **Name:** Tier  
**Type:** LicenseTier  
**Attributes:** public  
    - **Name:** ExpirationDate  
**Type:** DateTime?  
**Attributes:** public  
    - **Name:** LastValidatedOn  
**Type:** DateTime?  
**Attributes:** public  
    - **Name:** _features  
**Type:** List<LicensedFeature>  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** Create  
**Parameters:**
    
    - ...
    
**Return Type:** License  
**Attributes:** public static  
    - **Name:** Activate  
**Parameters:**
    
    - Dictionary<string, string> metadata
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Revoke  
**Parameters:**
    
    
**Return Type:** void  
**Attributes:** public  
    - **Name:** Validate  
**Parameters:**
    
    
**Return Type:** bool  
**Attributes:** public  
    - **Name:** IsFeatureEnabled  
**Parameters:**
    
    - string featureCode
    
**Return Type:** bool  
**Attributes:** public  
    - **Name:** IsInGracePeriod  
**Parameters:**
    
    - TimeSpan gracePeriodDuration
    
**Return Type:** bool  
**Attributes:** public  
    
**Implemented Features:**
    
    - License Lifecycle Management
    - Feature Entitlement Logic
    - Grace Period Calculation
    
**Requirement Ids:**
    
    - REQ-9-006
    - REQ-9-007
    - REQ-9-008
    
**Purpose:** Encapsulates all data and business rules related to a single license, ensuring its state is always consistent.  
**Logic Description:** This class is the heart of the domain. The Activate method will check if the license is already active or revoked and throw domain exceptions if rules are violated. The Validate method checks status and expiry. IsFeatureEnabled checks the license tier against the requested feature. IsInGracePeriod contains the logic for REQ-9-008. All state changes are done through public methods, never by setting properties directly from outside.  
**Documentation:**
    
    - **Summary:** The core domain entity representing a software license. It contains all the business logic for its creation, activation, validation, revocation, and feature entitlement checks.
    
**Namespace:** LicensingService.Domain.Aggregates  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Domain/Enums/LicenseType.cs  
**Description:** Enumeration for the different commercial models of a license.  
**Template:** C# Enum  
**Dependency Level:** 0  
**Name:** LicenseType  
**Type:** Enum  
**Relative Path:** src/LicensingService.Domain/Enums  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** PerUser  
**Type:** enum  
**Attributes:**   
    - **Name:** PerSite  
**Type:** enum  
**Attributes:**   
    - **Name:** Subscription  
**Type:** enum  
**Attributes:**   
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    - REQ-9-006
    
**Purpose:** To provide a strongly-typed representation of the license models supported by the system.  
**Logic Description:** A simple C# enum to represent the different license models.  
**Documentation:**
    
    - **Summary:** Defines the supported license models, such as Per-User, Per-Site, or Subscription-based.
    
**Namespace:** LicensingService.Domain.Enums  
**Metadata:**
    
    - **Category:** BusinessLogic
    
- **Path:** services/licensing-service/src/LicensingService.Infrastructure/LicensingService.Infrastructure.csproj  
**Description:** Project file for the Infrastructure layer, defining dependencies on persistence libraries (EF Core), cryptography, etc.  
**Template:** C# Project File  
**Dependency Level:** 2  
**Name:** LicensingService.Infrastructure  
**Type:** Project  
**Relative Path:** src/LicensingService.Infrastructure  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    
**Methods:**
    
    
**Implemented Features:**
    
    
**Requirement Ids:**
    
    
**Purpose:** Defines the build configuration and dependencies for the infrastructure project.  
**Logic Description:** This project will reference the LicensingService.Application project to get access to the repository and service interfaces it needs to implement.  
**Documentation:**
    
    - **Summary:** Manages compilation and dependencies for the infrastructure layer, which contains concrete implementations for data persistence, external services, etc.
    
**Namespace:**   
**Metadata:**
    
    - **Category:** Build
    
- **Path:** services/licensing-service/src/LicensingService.Infrastructure/Persistence/Repositories/LicenseRepository.cs  
**Description:** Implements the ILicenseRepository interface using Entity Framework Core for data access.  
**Template:** C# Repository Implementation  
**Dependency Level:** 3  
**Name:** LicenseRepository  
**Type:** Repository  
**Relative Path:** src/LicensingService.Infrastructure/Persistence/Repositories  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    - RepositoryPattern
    
**Members:**
    
    - **Name:** _context  
**Type:** ApplicationDbContext  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GetByKeyAsync  
**Parameters:**
    
    - LicenseKey licenseKey
    - CancellationToken cancellationToken
    
**Return Type:** Task<License?>  
**Attributes:** public  
    - **Name:** AddAsync  
**Parameters:**
    
    - License license
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    - **Name:** UpdateAsync  
**Parameters:**
    
    - License license
    - CancellationToken cancellationToken
    
**Return Type:** Task  
**Attributes:** public  
    
**Implemented Features:**
    
    - License Data Persistence
    
**Requirement Ids:**
    
    
**Purpose:** Provides a concrete implementation for persisting and retrieving License aggregates to/from a relational database.  
**Logic Description:** This class will use the injected ApplicationDbContext to perform database operations. The GetByKeyAsync method will query the Licenses DbSet, including any related child entities like LicensedFeatures. AddAsync and UpdateAsync will add or update entities in the context. The actual `SaveChanges` call is handled by the Unit of Work pattern, not here.  
**Documentation:**
    
    - **Summary:** The concrete implementation of the license repository using Entity Framework Core to interact with the database.
    
**Namespace:** LicensingService.Infrastructure.Persistence.Repositories  
**Metadata:**
    
    - **Category:** DataAccess
    
- **Path:** services/licensing-service/src/LicensingService.Infrastructure/Services/Cryptography/OfflineActivationService.cs  
**Description:** Implements the logic for creating and verifying offline activation files using cryptographic signatures.  
**Template:** C# Service Implementation  
**Dependency Level:** 1  
**Name:** OfflineActivationService  
**Type:** Service  
**Relative Path:** src/LicensingService.Infrastructure/Services/Cryptography  
**Repository Id:** REPO-SAP-011  
**Pattern Ids:**
    
    
**Members:**
    
    - **Name:** _privateKey  
**Type:** RSA  
**Attributes:** private|readonly  
    
**Methods:**
    
    - **Name:** GenerateSignedActivationFile  
**Parameters:**
    
    - string licenseKey
    - string machineId
    
**Return Type:** string  
**Attributes:** public  
    - **Name:** VerifyAndExtractActivationData  
**Parameters:**
    
    - string signedResponseFile
    
**Return Type:** ActivationData  
**Attributes:** public  
    
**Implemented Features:**
    
    - Offline Activation File Generation
    - Offline Activation File Validation
    
**Requirement Ids:**
    
    - REQ-9-008
    
**Purpose:** Provides a secure mechanism for the offline activation workflow.  
**Logic Description:** This service will hold a private key (loaded securely from configuration). GenerateSignedActivationFile will create a data payload (e.g., JSON with license key, machine ID, nonce), sign it with the private key, and return a tamper-proof string (e.g., Base64 encoded). VerifyAndExtractActivationData will take a response, verify the signature using the corresponding public key, and extract the payload.  
**Documentation:**
    
    - **Summary:** Handles the cryptographic operations necessary for the secure offline activation process, including signing request files and verifying response files.
    
**Namespace:** LicensingService.Infrastructure.Services.Cryptography  
**Metadata:**
    
    - **Category:** Infrastructure
    


---

# 2. Configuration

- **Feature Toggles:**
  
  - EnableGracePeriod
  - AllowOfflineActivation
  
- **Database Configs:**
  
  - ConnectionStrings:LicensingDb
  - KeyVault:Uri
  - Cryptography:PrivateKey
  


---

