# Software Design Specification (SDS): Infrastructure.Data.Migrations

## 1. Introduction

### 1.1 Purpose
This document provides the detailed software design for the `Opc.System.Infrastructure.Data.Migrations` repository. This project is exclusively responsible for defining, versioning, and applying the relational database schema for the entire system using Entity Framework (EF) Core. It ensures a consistent and repeatable process for database creation and evolution across all deployment environments.

### 1.2 Scope
The scope of this project is strictly limited to the management of database migrations. It will contain:
- The EF Core design-time tools configuration.
- A design-time factory for instantiating the `ApplicationDbContext`.
- All generated migration files that represent the history of schema changes.

This project **does not** contain any business logic, data access repositories, or the `DbContext` class itself. It has a direct dependency on the `Opc.System.Infrastructure.Data` project, from which it sources the entity models and `DbContext` definition.

### 1.3 Audience
This document is intended for software developers and DevOps engineers responsible for developing, maintaining, and deploying the system's database.

## 2. System Overview & Design Principles

### 2.1 Architecture
This project adheres to a **Layered Architecture**, functioning as a dedicated infrastructure layer for database schema management. It is a "headless" project, meaning it is not a runnable application in a traditional sense but rather a toolset invoked by developers and CI/CD pipelines.

### 2.2 Core Technology
- **Framework:** .NET 8
- **ORM:** Entity Framework Core 8
- **Database Provider:** PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Tooling:** `dotnet-ef` CLI

### 2.3 Design Principles
- **Single Responsibility:** The project's sole responsibility is to generate and contain database migration scripts. All entity definitions, configurations, and the `DbContext` are defined in the upstream `Opc.System.Infrastructure.Data` project.
- **Schema as Code:** All database schema changes are captured as C# code within migration files, stored in version control (Git). This ensures full traceability and reproducibility.
- **Idempotency:** EF Core migrations are inherently idempotent. Applying the same set of migrations to a database multiple times will result in the same final schema state.
- **Environment Agnostic:** The migration files contain no environment-specific configuration (e.g., connection strings). Connection details are supplied externally by the `dotnet-ef` tool at design time or by the deployment pipeline at release time.

## 3. Detailed Component Design

### 3.1 Project File (`Opc.System.Infrastructure.Data.Migrations.csproj`)
This is the .NET 8 project file. It will be configured to support the EF Core tools and reference the data project.

xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Opc.System.Infrastructure.Data.Migrations</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- EF Core tools for design-time operations like adding migrations -->
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.x">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.x">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    
    <!-- Database provider -->
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.x" />
    
    <!-- For reading appsettings.json at design time -->
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.x" />
  </ItemGroup>

  <ItemGroup>
    <!-- Project reference to the data project where DbContext and entities are defined -->
    <ProjectReference Include="..\Data\Opc.System.Infrastructure.Data.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Include appsettings for design-time connection string -->
    <Content Include="appsettings.Development.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>


### 3.2 Design-Time DbContext Factory (`DbContextFactory.cs`)
This class is essential for the `dotnet-ef` tools to create an instance of the `ApplicationDbContext` (defined in the referenced project) when this migrations project is the startup project.

**File:** `src/Infrastructure/Data.Migrations/DbContextFactory.cs`
**Namespace:** `Opc.System.Infrastructure.Data.Migrations`

csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Opc.System.Infrastructure.Data; // Reference to the project with the DbContext

namespace Opc.System.Infrastructure.Data.Migrations;

/// <summary>
/// Factory for creating ApplicationDbContext instances at design time.
/// Used by EF Core tools to generate and apply migrations from this separate project.
/// </summary>
public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates a new instance of the ApplicationDbContext.
    /// </summary>
    /// <param name="args">Command-line arguments (not used).</param>
    /// <returns>A configured ApplicationDbContext instance.</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read the connection string from appsettings.Development.json
        // This file should NOT be checked into source control if it contains secrets.
        // Use user secrets for a more secure approach in a team environment.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DesignTimeConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Configure the DbContext to use PostgreSQL
        optionsBuilder.UseNpgsql(connectionString, b =>
        {
            // Specify that the migrations assembly is this current project
            b.MigrationsAssembly(typeof(DbContextFactory).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}


### 3.3 Initial Database Migration (`Migrations/20250521000000_InitialCreate.cs`)
This is the first migration file generated by EF Core. It programmatically defines the creation of the entire database schema as specified in the database design.

**File:** `src/Infrastructure/Data.Migrations/Migrations/20250521000000_InitialCreate.cs`
**Purpose:** To define the C# code that builds the initial database schema. The `Up` method will be executed to apply the migration, and the `Down` method to revert it.

**Logic Description of the `Up` method:**
The `Up(MigrationBuilder migrationBuilder)` method will contain a sequence of `migrationBuilder.CreateTable` calls. Each call will precisely define the schema for one table from the database design.

**Key Implementation Details:**
- **Tables:** All 18 tables from the database design will be created: `User`, `Role`, `Permission`, `OpcServer`, `OpcTag`, `Subscription`, `DataLog`, `HistoricalData`, `AlarmEvent`, `Dashboard`, `ReportTemplate`, `AuditLog`, `DataRetentionPolicy`, `PredictiveMaintenanceModel`, `BlockchainTransaction`, `MigrationStrategy`, `UserRole`, and `RolePermission`.
- **Columns:**
  - `UUID` types in the design will be mapped to `table.Column<Guid>(type: "uuid", nullable: false, ...)` for PostgreSQL.
  - `VARCHAR(size)` will be `table.Column<string>(type: "character varying(size)", maxLength: size, ...)`
  - `DateTimeOffset` will be `table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, ...)`
  - `BOOLEAN` will be `table.Column<bool>(type: "boolean", nullable: false, defaultValue: ...)`
  - `JSON` columns like `validationRules` and `layoutConfig` will be `table.Column<string>(type: "jsonb")`.
- **Primary Keys:** Defined within the `constraints` lambda of `CreateTable`, e.g., `table.PrimaryKey("PK_User", x => x.userId)`. Composite keys for junction tables (`UserRole`, `RolePermission`) will be defined similarly: `x => new { x.userId, x.roleId }`.
- **Foreign Keys:** Also defined in the `constraints` lambda, linking tables as specified in the ER diagram. Example:
  csharp
  table.ForeignKey(
      name: "FK_OpcTag_OpcServer_serverId",
      column: x => x.serverId,
      principalTable: "OpcServer",
      principalColumn: "serverId",
      onDelete: ReferentialAction.Cascade);
  
- **Unique Constraints:** Created using `migrationBuilder.CreateIndex` with `unique: true` or defined within the table creation for simple cases. Example: `migrationBuilder.CreateIndex(name: "uq_user_username", table: "User", column: "username", unique: true);`
- **Indexes:** Created using `migrationBuilder.CreateIndex` for performance-critical columns and queries. This includes:
  - `idx_datalog_tagid_timestamp` on `DataLog`
  - `idx_historicaldata_tagid_timestamp` on `HistoricalData`
  - `idx_alarmevent_tagid_occurrencetime` on `AlarmEvent`
  - `idx_auditlog_timestamp_eventtype` on `AuditLog`
  - A filtered index for unacknowledged alarms on `AlarmEvent`: `migrationBuilder.CreateIndex(name: "idx_alarmevent_unack_occurrencetime", table: "AlarmEvent", column: "occurrenceTime", filter: "\"acknowledgedTime\" IS NULL");`

**Logic Description of the `Down` method:**
The `Down(MigrationBuilder migrationBuilder)` method will contain a sequence of `migrationBuilder.DropTable` calls for all 18 tables, in the reverse order of creation to respect foreign key constraints.

## 4. Usage and Deployment Workflow

### 4.1 Developer Workflow
1.  **Modify Model:** A developer makes a change to an entity class or a `DbContext` configuration in the `Opc.System.Infrastructure.Data` project.
2.  **Add Migration:** From the solution root, the developer runs the following command:
    bash
    dotnet ef migrations add <MigrationName> --project src/Infrastructure/Data.Migrations
    
    This command uses the `DbContextFactory` in this project to scaffold a new migration file reflecting the changes.
3.  **Apply to Local DB:** To update their local development database, the developer runs:
    bash
    dotnet ef database update --project src/Infrastructure/Data.Migrations
    

### 4.2 CI/CD Deployment Workflow
1.  **Build:** The CI pipeline builds the entire solution, including this migrations project.
2.  **Deploy:** The CD pipeline, upon deploying to a new environment (e.g., Staging, Production), executes the migrations. This is typically done as a dedicated step in the release pipeline. The connection string for the target database is securely passed as an environment variable or command-line argument.
    bash
    dotnet ef database update --project src/Infrastructure/Data.Migrations --connection "Server=...;Port=...;Database=...;User Id=...;Password=...;"
    
    Alternatively, a SQL script can be generated for manual review and execution by a DBA:
    bash
    dotnet ef migrations script --project src/Infrastructure/Data.Migrations -o deploy.sql
    

## 5. Traceability
This repository directly implements the following requirements by providing the tooling and schema definitions necessary for data migration and management:
- **`REQ-DLP-004`, `REQ-DLP-007`, `REQ-CSVC-016`, `REQ-CSVC-022`:** The creation of the `MigrationStrategy` table within the initial migration provides the foundational schema to store and track the execution of data migration plans from legacy systems. The overall EF Core migrations process is the "tool" that supports these requirements.