# Software Design Specification (SDS) for Licensing Service

## 1. Introduction

### 1.1. Purpose

This document provides a detailed software design for the **Licensing Service** (`services.licensing-service`). This microservice is the central authority for managing the entire lifecycle of software licenses for the platform. Its responsibilities include license generation, activation (online and offline), validation, and enforcement of commercial terms such as feature entitlements, usage models, and grace periods.

### 1.2. Scope

The scope of this document is limited to the design of the `LicensingService` microservice. It covers the internal architecture, API design, domain logic, data persistence, and infrastructure integrations required to fulfill its responsibilities as defined by requirements `REQ-9-006`, `REQ-9-007`, and `REQ-9-008`.

## 2. System Architecture & Design Principles

The service will be implemented using a **Clean Architecture** approach, ensuring a clear separation of concerns, maintainability, and testability.

-   **Domain Layer:** Contains the core business logic, entities, and rules. It is completely independent of any infrastructure concerns.
-   **Application Layer:** Orchestrates the use cases of the application. It contains command and query handlers, DTOs, and interfaces for infrastructure services. It depends only on the Domain layer.
-   **Infrastructure Layer:** Contains concrete implementations of the interfaces defined in the Application layer, such as database repositories and cryptographic services. It depends on the Application layer.
-   **Presentation (API) Layer:** Exposes the application's functionality via a RESTful API. It depends on the Application layer to execute use cases.

The service will also adhere to the following design patterns:

-   **CQRS (Command Query Responsibility Segregation):** The application logic is separated into Commands (actions that change state) and Queries (actions that read state). This is implemented using the **MediatR** library.
-   **Repository Pattern:** Abstracts the data persistence mechanism from the rest of the application.
-   **Domain-Driven Design (DDD):** The `License` entity will be modeled as an Aggregate Root to encapsulate business rules and ensure consistency.

## 3. Domain Layer Design (`LicensingService.Domain`)

This layer contains the core business logic and is the heart of the service.

### 3.1. Aggregates

#### 3.1.1. `License` Aggregate Root

This is the central entity, representing a single software license. It enforces all invariants and business rules related to its lifecycle.

-   **Properties:**
    -   `Guid Id { get; private set; }`: The unique identifier.
    -   `LicenseKey Key { get; private set; }`: The public-facing license key (Value Object).
    -   `LicenseStatus Status { get; private set; }`: The current status (`Pending`, `Active`, `Expired`, `Revoked`).
    -   `LicenseType Type { get; private set; }`: The commercial model (`PerUser`, `PerSite`, `Subscription`).
    -   `LicenseTier Tier { get; private set; }`: The feature tier (`Basic`, `Pro`, `Enterprise`).
    -   `Guid CustomerId { get; private set; }`: The ID of the customer this license belongs to.
    -   `DateTime CreatedAt { get; private set; }`: Timestamp of creation.
    -   `DateTime? ExpirationDate { get; private set; }`: Expiration date for subscription-based licenses.
    -   `DateTime? ActivatedOn { get; private set; }`: Timestamp of first activation.
    -   `DateTime? LastValidatedOn { get; private set; }`: Timestamp of the last successful validation check.
    -   `Dictionary<string, string> ActivationMetadata { get; private set; }`: Metadata captured on activation (e.g., Machine ID).
    -   `IReadOnlyCollection<LicensedFeature> Features { get; }`: A collection of features enabled by this license's tier.

-   **Methods (Business Logic):**
    -   `public static License Create(...)`: Factory method to create a new license in a `Pending` state, ensuring all required data is present.
    -   `public void Activate(Dictionary<string, string> metadata)`: Activates the license.
        -   *Logic:* Throws `DomainException` if `Status` is not `Pending`. Sets `Status` to `Active`, records `ActivatedOn`, and stores activation `metadata`.
    -   `public void Revoke()`: Revokes the license.
        -   *Logic:* Sets `Status` to `Revoked`.
    -   `public bool IsValid()`: Checks if the license is currently valid.
        -   *Logic:* Returns `true` only if `Status` is `Active` AND (`ExpirationDate` is null OR `ExpirationDate` > `DateTime.UtcNow`).
    -   `public bool IsInGracePeriod(TimeSpan gracePeriodDuration)`: Checks if the license is in a grace period after a validation failure.
        -   *Logic:* Returns `true` if `Status` is `Active` AND `LastValidatedOn` is not null AND (`DateTime.UtcNow` - `LastValidatedOn`) <= `gracePeriodDuration`.
    -   `public bool IsFeatureEnabled(string featureCode)`: Checks if a specific feature is enabled.
        -   *Logic:* Determines if the `featureCode` is included in the set of features associated with the license's `Tier`.
    -   `public void RecordValidation()`: Updates the timestamp of the last successful validation.
        -   *Logic:* Sets `LastValidatedOn` to `DateTime.UtcNow`.

### 3.2. Value Objects

#### 3.2.1. `LicenseKey`

A strongly-typed wrapper for the license key string to ensure validity and structure.

-   **Properties:** `public string Value { get; }`
-   **Logic:** The constructor will validate the key format (e.g., `CUST-XXXX-XXXX-XXXX-XXXX`). It implements equality based on its `Value`.

### 3.3. Entities

#### 3.3.1. `LicensedFeature`

Represents a feature that is enabled for a given license tier.

-   **Properties:**
    -   `string FeatureCode { get; }`: A unique code for the feature (e.g., "AI_ANOMALY_DETECTION").
    -   `string Description { get; }`: A user-friendly description.
    -   `LicenseTier RequiredTier { get; }`: The minimum tier required to enable this feature.

### 3.4. Enums

-   `LicenseStatus`: { `Pending`, `Active`, `Expired`, `Revoked` }
-   `LicenseType`: { `PerUser`, `PerSite`, `Subscription` }
-   `LicenseTier`: { `Basic`, `Pro`, `Enterprise` }

## 4. Application Layer Design (`LicensingService.Application`)

This layer orchestrates the domain logic through CQRS handlers.

### 4.1. Features (Use Cases)

#### `GenerateLicense` (Admin)
-   **Command:** `GenerateLicenseCommand(Guid CustomerId, LicenseType Type, LicenseTier Tier, DateTime? ExpirationDate)`
-   **Handler:** `GenerateLicenseCommandHandler`
    -   *Logic:*
        1.  Calls `_licenseKeyGenerator.GenerateKey()` to create a new `LicenseKey`.
        2.  Calls `License.Create()` domain factory method to instantiate a new `License` aggregate.
        3.  Calls `_licenseRepository.AddAsync()` to persist the new license.
        4.  Calls `_unitOfWork.SaveChangesAsync()`.
        5.  Returns the generated license key string.

#### `ActivateLicense` (Online)
-   **Command:** `ActivateLicenseCommand(string LicenseKey, Dictionary<string, string> ActivationMetadata)`
-   **Handler:** `ActivateLicenseCommandHandler`
    -   *Logic:*
        1.  Fetches the `License` aggregate from `_licenseRepository` using the `LicenseKey`. Throws `NotFoundException` if not found.
        2.  Calls the `license.Activate(metadata)` domain method.
        3.  Calls `_licenseRepository.UpdateAsync()` and `_unitOfWork.SaveChangesAsync()`.
        4.  Returns a success DTO.

#### `ValidateLicense`
-   **Query:** `ValidateLicenseQuery(string LicenseKey)`
-   **Handler:** `ValidateLicenseQueryHandler`
    -   *Logic:*
        1.  Fetches the `License` aggregate from `_licenseRepository`. Throws `NotFoundException` if not found.
        2.  Calls `license.IsValid()`.
        3.  If valid, calls `license.RecordValidation()` and persists the change. Returns a `LicenseValidationDto` with `IsValid = true`.
        4.  If invalid, calls `license.IsInGracePeriod()`.
        5.  If in grace period, returns a `LicenseValidationDto` with `IsValid = true` and a `GracePeriodWarning` message.
        6.  If not in grace period, returns a `LicenseValidationDto` with `IsValid = false`.

#### `CheckFeatureEntitlement`
-   **Query:** `CheckFeatureEntitlementQuery(string LicenseKey, string FeatureCode)`
-   **Handler:** `CheckFeatureEntitlementQueryHandler`
    -   *Logic:*
        1.  Executes the `ValidateLicenseQuery` to check overall license validity first. If invalid, returns `IsEnabled = false`.
        2.  Fetches the `License` aggregate.
        3.  Calls `license.IsFeatureEnabled(featureCode)`.
        4.  Returns a DTO with the boolean result.

#### `GenerateOfflineActivationRequest`
-   **Command:** `GenerateOfflineActivationRequestCommand(string LicenseKey, string MachineId)`
-   **Handler:** `GenerateOfflineActivationRequestCommandHandler`
    -   *Logic:*
        1.  Calls `_offlineActivationService.GenerateSignedActivationFile(licenseKey, machineId)`.
        2.  Returns the signed file content as a string.

#### `CompleteOfflineActivation`
-   **Command:** `CompleteOfflineActivationCommand(string SignedVendorResponse)`
-   **Handler:** `CompleteOfflineActivationCommandHandler`
    -   *Logic:*
        1.  Calls `_offlineActivationService.VerifyAndExtractActivationData(signedVendorResponse)` to get the license key and metadata.
        2.  Fetches the corresponding `License` aggregate.
        3.  Calls `license.Activate(metadata)`.
        4.  Persists the changes.
        5.  Returns a success DTO.

### 4.2. Contracts (Interfaces)

-   `ILicenseRepository`: Defines methods like `GetByKeyAsync`, `AddAsync`, `UpdateAsync`.
-   `IUnitOfWork`: Defines `Task<int> SaveChangesAsync(CancellationToken)`.
-   `ILicenseKeyGenerator`: Defines `LicenseKey GenerateKey(...)`.
-   `IOfflineActivationService`: Defines `GenerateSignedActivationFile(...)` and `VerifyAndExtractActivationData(...)`.

## 5. Infrastructure Layer Design (`LicensingService.Infrastructure`)

This layer provides concrete implementations for the application layer's contracts.

### 5.1. Persistence

-   **`ApplicationDbContext` (EF Core):**
    -   Contains a `DbSet<License>`.
    -   Configures the mapping for the `License` aggregate, including its value objects (`LicenseKey` using a converter) and private fields (`_features`).
-   **`LicenseRepository`:**
    -   Implements `ILicenseRepository` using the `ApplicationDbContext`.
-   **`UnitOfWork`:**
    -   Implements `IUnitOfWork` by wrapping `ApplicationDbContext.SaveChangesAsync()`.

### 5.2. Services

-   **`LicenseKeyGenerator`:**
    -   Implements `ILicenseKeyGenerator`.
    -   *Logic:* Generates a cryptographically random string or a structured key (e.g., `CUSTID-GUID`) and formats it according to business rules.
-   **`OfflineActivationService`:**
    -   Implements `IOfflineActivationService`.
    -   *Logic:*
        -   Loads a private key (e.g., RSA or ECDsa) securely from configuration (Azure Key Vault, etc.).
        -   `GenerateSignedActivationFile`: Creates a JSON payload with the request data, then signs the hash of the payload using the private key. Encodes the payload and signature into a Base64 string.
        -   `VerifyAndExtractActivationData`: Takes a signed response from the vendor, verifies its signature using a corresponding public key, and deserializes the payload if valid.

## 6. Presentation (API) Layer Design (`LicensingService.Api`)

### 6.1. `Program.cs` Configuration

-   **Dependency Injection:**
    -   Register Application and Infrastructure layer services via extension methods.
    -   Register MediatR.
    -   Register the `DbContext`.
-   **Middleware:**
    -   Configure a global exception handler.
    -   `UseHttpsRedirection`, `UseRouting`.
    -   `UseAuthentication`, `UseAuthorization`.
    -   Map controllers (`MapControllers`).
-   **API Versioning & Documentation:**
    -   Configure Swagger/OpenAPI with security definitions (JWT Bearer).

### 6.2. Controllers

#### `LicensesController` (`api/v1/licenses`)

-   `POST /activate`: Accepts `ActivateLicenseCommand`. Returns `200 OK` or `400 Bad Request`.
-   `GET /{licenseKey}/validate`: Returns `200 OK` with `LicenseValidationDto`.
-   `GET /{licenseKey}/features/{featureCode}`: Returns `200 OK` with `FeatureEntitlementDto`.
-   `POST /offline/request`: Accepts `GenerateOfflineActivationRequestCommand`. Returns `200 OK` with the request file content.
-   `POST /offline/complete`: Accepts `CompleteOfflineActivationCommand`. Returns `200 OK` or `400 Bad Request`.

#### `AdminController` (`api/v1/admin/licenses`)

-   **Authorization:** All endpoints protected by an `[Authorize(Policy = "AdminOnly")]` attribute.
-   `POST /`: Accepts `GenerateLicenseCommand`. Returns `201 Created` with the new license key.
-   `DELETE /{licenseKey}`: Accepts a license key to revoke. Returns `204 No Content`.
-   `GET /{licenseKey}`: Returns `200 OK` with detailed license information.

## 7. API Contracts (DTOs)

| DTO Name | Purpose | Properties |
| :--- | :--- | :--- |
| `GenerateLicenseRequest` | Input for creating a license. | `CustomerId`, `LicenseType`, `LicenseTier`, `ExpirationDate?` |
| `ActivateLicenseRequest` | Input for online activation. | `LicenseKey`, `ActivationMetadata` |
| `LicenseValidationDto` | Output for validation query. | `LicenseKey`, `IsValid`, `Status`, `GracePeriodWarning?`, `Features[]` |
| `FeatureEntitlementDto` | Output for feature check. | `FeatureCode`, `IsEnabled` |
| `OfflineActivationRequestDto` | Input for starting offline activation. | `LicenseKey`, `MachineId` |
-   **Output:** a Base64 string representing the signed request file.
| `CompleteOfflineActivationRequestDto` | Input for completing offline activation. | `SignedVendorResponse` (Base64 string) |

## 8. Error Handling and Validation

-   **Validation:** FluentValidation will be used. Validators will be defined in the Application layer for each command/query. A MediatR pipeline behavior will automatically execute validators before the handler, throwing a `ValidationException` on failure.
-   **Exception Handling:** A global exception handling middleware in `LicensingService.Api` will:
    -   Catch `ValidationException` -> return `400 Bad Request` with a list of errors.
    -   Catch `NotFoundException` -> return `404 Not Found`.
    -   Catch `DomainException` -> return `400 Bad Request` with the business rule violation message.
    -   Catch any other `Exception` -> log it and return `500 Internal Server Error` with a generic message.

## 9. Configuration (`appsettings.json`)

json
{
  "ConnectionStrings": {
    "LicensingDb": "..."
  },
  "LicensingOptions": {
    "GracePeriodHours": 72
  },
  "Cryptography": {
    "OfflineActivationPrivateKey": "value-loaded-from-key-vault",
    "OfflineActivationVendorPublicKey": "..."
  },
  "JwtSettings": {
    // For validating admin tokens
    "Issuer": "...",
    "Audience": "...",
    "Key": "value-loaded-from-key-vault"
  }
}
