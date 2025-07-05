# Software Design Specification (SDS) for `data.migrations`

## 1. Introduction

### 1.1 Purpose
This document provides a detailed software design specification for the `data.migrations` repository. The sole purpose of this repository is to manage the lifecycle of the relational database schema using Entity Framework (EF) Core Migrations. It provides a version-controlled, auditable, and automated way to evolve the database schema in lockstep with the application's feature development.

### 1.2 Scope
The scope of this repository is limited to:
- Defining the project structure necessary for a standalone EF Core migrations project.
- Providing a design-time factory for `DbContext` instantiation.
- Containing the C# code for all database migrations, starting with the initial schema creation.
- Documenting the workflow for creating and applying migrations.

This project does **not** contain any business logic, data access repositories, or domain entities. It is a utility project consumed by developers and CI/CD pipelines.

### 1.3 Dependencies
- **Internal:** This project has a critical dependency on the `data.access` project (or equivalent), which is assumed to contain all EF Core `DbSet<>` entity definitions and the `ApplicationDbContext` class.
- **External:**
    - .NET 8 SDK
    - Entity Framework Core 8
    - Npgsql.EntityFrameworkCore.PostgreSQL (PostgreSQL provider for EF Core)

## 2. System Overview and Design

### 2.1 Architectural Approach
The design follows the standard pattern for a dedicated migrations assembly in EF Core. This approach decouples schema management from the main application's startup and runtime logic, providing a clean separation of concerns. It allows developers and CI/CD systems to manage the database schema as an independent, deployable artifact.

### 2.2 Data Model
The database schema to be created by the initial migration is based on the provided `DatabaseDesign`. The migrations will programmatically define tables, columns, primary keys, foreign keys, indexes, and constraints that exactly match this design.

## 3. Component Specifications

### 3.1 `Data.Migrations.csproj`
This is the .NET project file that configures the migrations assembly.

- **Type:** C# Project File (XML)
- **Purpose:** To define project dependencies and settings, enabling the use of EF Core design-time tools.

**Specification:**
- **`TargetFramework`:** `net8.0`
- **`OutputType`:** `Library`
- **`PackageReferences`:** The project file shall include the following package references:
    - `Microsoft.EntityFrameworkCore.Design`: Contains the design-time logic for EF Core.
    - `Microsoft.EntityFrameworkCore.Tools`: Provides the command-line tools (`dotnet ef`).
    - `Npgsql.EntityFrameworkCore.PostgreSQL`: The database provider for PostgreSQL.
    - `Microsoft.Extensions.Configuration.Json`: To read `appsettings.json`.
- **`ProjectReference`:** The project shall include a `ProjectReference` to the `SSS.Data.Access.csproj` project (or its equivalent name), where the `ApplicationDbContext` and entities are defined.

### 3.2 `appsettings.json`
This file provides the connection string for use during design-time operations.

- **Type:** JSON Configuration File
- **Purpose:** To provide a database connection string for the `dotnet ef` tools without hardcoding it.

**Specification:**
The file shall contain the following JSON structure:
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sss_opc_dev;Username=postgres;Password=your_dev_password"
  }
}

> **Note:** The connection string value is an example and should be configured for the local development environment.

### 3.3 `DesignTimeDbContextFactory.cs`
A class that enables the creation of a `DbContext` instance by the design-time tools.

- **Namespace:** `SSS.Data.Migrations`
- **Class:** `DesignTimeDbContextFactory`
- **Purpose:** To provide an entry point for EF Core tools to instantiate `ApplicationDbContext`.

**Specification:**
- The class `DesignTimeDbContextFactory` **shall** implement the `IDesignTimeDbContextFactory<ApplicationDbContext>` interface.
- It **shall** have one public method: `CreateDbContext(string[] args)`.

**Method Logic: `CreateDbContext(string[] args)`**
1. Instantiate `IConfigurationRoot` by building a `ConfigurationBuilder`.
2. The builder **shall** set its base path to the current directory.
3. The builder **shall** add `appsettings.json` as a configuration source.
4. Build the configuration.
5. Retrieve the connection string named "DefaultConnection" from the configuration object.
6. Instantiate `DbContextOptionsBuilder<ApplicationDbContext>`.
7. Configure the options builder to use PostgreSQL by calling `UseNpgsql()` and passing the connection string.
8. Return a new instance of `ApplicationDbContext`, passing the configured `options.Options` to its constructor.

### 3.4 `Migrations/` Folder
This folder will contain all auto-generated migration files.

#### 3.4.1 `20250521100000_InitialCreate.cs`
This is the first migration file, responsible for creating the entire initial database schema.

- **Namespace:** `SSS.Data.Migrations.Migrations`
- **Class:** `_20250521100000_InitialCreate` (inherits from `Migration`)
- **Purpose:** To programmatically define the DDL for creating and dropping the initial database tables and relationships.

**`Up(MigrationBuilder migrationBuilder)` Method Specification:**
The `Up` method **shall** use the `migrationBuilder.CreateTable` method to create each of the following tables as specified in the database design. Foreign keys, indexes, and constraints must be explicitly defined.

**Example Table Creation (`OpcTag`):**
csharp
migrationBuilder.CreateTable(
    name: "OpcTags", // Use pluralized name by convention
    columns: table => new
    {
        TagId = table.Column<Guid>(type: "uuid", nullable: false),
        ServerId = table.Column<Guid>(type: "uuid", nullable: false),
        NodeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
        DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
        Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
        IsWritable = table.Column<bool>(type: "boolean", nullable: false),
        ValidationRules = table.Column<string>(type: "jsonb", nullable: true) // Use 'jsonb' for PostgreSQL
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_OpcTags", x => x.TagId);
        table.ForeignKey(
            name: "FK_OpcTags_OpcServers_ServerId",
            column: x => x.ServerId,
            principalTable: "OpcServers",
            principalColumn: "ServerId",
            onDelete: ReferentialAction.Cascade); // Define delete behavior
    });

migrationBuilder.CreateIndex(
    name: "IX_OpcTags_ServerId",
    table: "OpcTags",
    column: "ServerId");

migrationBuilder.AddUniqueConstraint(
    name: "uq_opctag_serverid_nodeid",
    table: "OpcTags",
    columns: new[] { "ServerId", "NodeId" });

> **Note:** This process must be repeated for **all** tables defined in the `DatabaseDesign`, including `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `OpcServer`, `DataLog`, `HistoricalData`, `AlarmEvent`, `AuditLog`, etc., ensuring all columns, types, keys, constraints, and relationships are correctly translated.

**Partitioning Implementation:**
For tables specified with partitioning (`DataLog`, `HistoricalData`, `AlarmEvent`, `AuditLog`), the migration **shall** use `migrationBuilder.Sql()` to execute raw SQL commands to set up partitioning *after* the initial table definition.

**Example for `AuditLog` (PostgreSQL):**
csharp
// 1. Create the main partitioned table
migrationBuilder.Sql(@"
    CREATE TABLE ""AuditLogs"" (
        ""LogId"" uuid NOT NULL,
        ""EventType"" character varying(50) NOT NULL,
        ""UserId"" uuid NULL,
        ""Timestamp"" timestamp with time zone NOT NULL,
        ""Details"" jsonb NOT NULL,
        ""IpAddress"" character varying(45) NULL,
        CONSTRAINT ""PK_AuditLogs"" PRIMARY KEY (""LogId"", ""Timestamp"")
    ) PARTITION BY RANGE (""Timestamp"");
");

// 2. Create an index on the partitioned table
migrationBuilder.Sql(@"
    CREATE INDEX ""IX_AuditLogs_Timestamp_EventType"" ON ""AuditLogs"" (""Timestamp"", ""EventType"");
");

// 3. Create an initial partition for a specific time range
migrationBuilder.Sql(@"
    CREATE TABLE auditlogs_y2025_q1 PARTITION OF ""AuditLogs""
    FOR VALUES FROM ('2025-01-01') TO ('2025-04-01');
");


**`Down(MigrationBuilder migrationBuilder)` Method Specification:**
The `Down` method **shall** contain the corresponding `migrationBuilder.DropTable` calls for every table created in the `Up` method, in the reverse order of creation, to ensure the migration can be reverted cleanly.

#### 3.4.2 `ApplicationDbContextModelSnapshot.cs`
This file is a complete snapshot of the database model. It is auto-generated and managed by EF Core tools. It should **not** be manually edited. Its presence is required for the `dotnet ef migrations add` command to compute schema differences.

## 4. Migration Workflow

### 4.1 Development Workflow
1. **Modify Entities:** A developer modifies the entity classes or `DbContext` in the `data.access` project.
2. **Generate Migration:** From the command line, the developer runs the following command:
   `dotnet ef migrations add <MigrationName> --project src/Data.Migrations --startup-project src/Data.Migrations`
3. **Review Migration:** The developer reviews the auto-generated migration file to ensure it accurately reflects the intended schema changes.
4. **Apply to Local DB:** The developer updates their local database using:
   `dotnet ef database update --project src/Data.Migrations --startup-project src/Data.Migrations`
5. **Commit Changes:** The developer commits the new migration files and any entity changes to version control.

### 4.2 CI/CD Pipeline Integration
The deployment pipeline for any environment (testing, staging, production) **shall** include a step that applies database migrations. This step will execute the `dotnet ef database update` command against the target environment's database, ensuring the schema is always in the correct state before the application is deployed. The connection string for the target environment will be supplied to the command as an argument or environment variable.