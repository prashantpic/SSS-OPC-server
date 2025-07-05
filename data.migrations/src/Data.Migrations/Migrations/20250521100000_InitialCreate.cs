using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSS.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _20250521100000_InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpcServers",
                columns: table => new
                {
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EndpointUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SecurityPolicy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Certificate = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcServers", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastLogin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalIdpId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
            
            migrationBuilder.CreateTable(
                name: "OpcTags",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsWritable = table.Column<bool>(type: "boolean", nullable: false),
                    ValidationRules = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcTags", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_OpcTags_OpcServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "OpcServers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishingInterval = table.Column<int>(type: "integer", nullable: false),
                    SamplingInterval = table.Column<int>(type: "integer", nullable: false),
                    QueueSize = table.Column<int>(type: "integer", nullable: false),
                    DeadbandType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DeadbandValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_OpcServers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "OpcServers",
                        principalColumn: "ServerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    DashboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LayoutConfig = table.Column<string>(type: "jsonb", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.DashboardId);
                    table.ForeignKey(
                        name: "FK_Dashboards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Raw SQL for Partitioned Tables and other DB objects as specified
            
            // Partitioned Table: AuditLog
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
            migrationBuilder.Sql(@"CREATE INDEX ""IX_AuditLogs_Timestamp_EventType"" ON ""AuditLogs"" (""Timestamp"", ""EventType"");");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_AuditLogs_UserId"" ON ""AuditLogs"" (""UserId"");");
            migrationBuilder.Sql(@"ALTER TABLE ""AuditLogs"" ADD CONSTRAINT ""FK_AuditLogs_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL;");
            migrationBuilder.Sql(@"CREATE TABLE auditlogs_y2025_q1 PARTITION OF ""AuditLogs"" FOR VALUES FROM ('2025-01-01') TO ('2025-04-01');");
            migrationBuilder.Sql(@"CREATE TABLE auditlogs_y2025_q2 PARTITION OF ""AuditLogs"" FOR VALUES FROM ('2025-04-01') TO ('2025-07-01');");


            // Partitioned Table: DataLog
            migrationBuilder.Sql(@"
                CREATE TABLE ""DataLogs"" (
                    ""LogId"" uuid NOT NULL,
                    ""TagId"" uuid NOT NULL,
                    ""UserId"" uuid NULL,
                    ""OldValue"" character varying(255) NULL,
                    ""NewValue"" character varying(255) NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""OperationStatus"" character varying(20) NOT NULL,
                    CONSTRAINT ""PK_DataLogs"" PRIMARY KEY (""LogId"", ""Timestamp"")
                ) PARTITION BY RANGE (""Timestamp"");
            ");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_DataLogs_TagId_Timestamp"" ON ""DataLogs"" (""TagId"", ""Timestamp"");");
            migrationBuilder.Sql(@"ALTER TABLE ""DataLogs"" ADD CONSTRAINT ""FK_DataLogs_OpcTags_TagId"" FOREIGN KEY (""TagId"") REFERENCES ""OpcTags"" (""TagId"") ON DELETE CASCADE;");
            migrationBuilder.Sql(@"ALTER TABLE ""DataLogs"" ADD CONSTRAINT ""FK_DataLogs_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""UserId"") ON DELETE SET NULL;");
            migrationBuilder.Sql(@"CREATE TABLE datalogs_y2025_m01 PARTITION OF ""DataLogs"" FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');");
            migrationBuilder.Sql(@"CREATE TABLE datalogs_y2025_m02 PARTITION OF ""DataLogs"" FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');");

            // Partitioned Table: HistoricalData
            migrationBuilder.Sql(@"
                CREATE TABLE ""HistoricalData"" (
                    ""DataId"" uuid NOT NULL,
                    ""TagId"" uuid NOT NULL,
                    ""Timestamp"" timestamp with time zone NOT NULL,
                    ""Value"" character varying(255) NOT NULL,
                    ""Quality"" character varying(50) NOT NULL,
                    ""AggregationType"" character varying(50) NULL,
                    CONSTRAINT ""PK_HistoricalData"" PRIMARY KEY (""DataId"", ""Timestamp"")
                ) PARTITION BY RANGE (""Timestamp"");
            ");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_HistoricalData_TagId_Timestamp"" ON ""HistoricalData"" (""TagId"", ""Timestamp"");");
            migrationBuilder.Sql(@"ALTER TABLE ""HistoricalData"" ADD CONSTRAINT ""FK_HistoricalData_OpcTags_TagId"" FOREIGN KEY (""TagId"") REFERENCES ""OpcTags"" (""TagId"") ON DELETE CASCADE;");
            migrationBuilder.Sql(@"CREATE TABLE historicaldata_y2025_q1 PARTITION OF ""HistoricalData"" FOR VALUES FROM ('2025-01-01') TO ('2025-04-01');");
            migrationBuilder.Sql(@"CREATE TABLE historicaldata_y2025_q2 PARTITION OF ""HistoricalData"" FOR VALUES FROM ('2025-04-01') TO ('2025-07-01');");


            // Partitioned Table: AlarmEvent
            migrationBuilder.Sql(@"
                CREATE TABLE ""AlarmEvents"" (
                    ""AlarmId"" uuid NOT NULL,
                    ""TagId"" uuid NOT NULL,
                    ""EventType"" character varying(50) NOT NULL,
                    ""Severity"" integer NOT NULL,
                    ""Message"" character varying(500) NOT NULL,
                    ""OccurrenceTime"" timestamp with time zone NOT NULL,
                    ""AcknowledgedTime"" timestamp with time zone NULL,
                    CONSTRAINT ""PK_AlarmEvents"" PRIMARY KEY (""AlarmId"", ""OccurrenceTime"")
                ) PARTITION BY RANGE (""OccurrenceTime"");
            ");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_AlarmEvents_TagId_OccurrenceTime"" ON ""AlarmEvents"" (""TagId"", ""OccurrenceTime"");");
            migrationBuilder.Sql(@"CREATE INDEX ""IX_AlarmEvents_Unack_OccurrenceTime"" ON ""AlarmEvents"" (""OccurrenceTime"") WHERE ""AcknowledgedTime"" IS NULL;");
            migrationBuilder.Sql(@"ALTER TABLE ""AlarmEvents"" ADD CONSTRAINT ""FK_AlarmEvents_OpcTags_TagId"" FOREIGN KEY (""TagId"") REFERENCES ""OpcTags"" (""TagId"") ON DELETE CASCADE;");
            migrationBuilder.Sql(@"CREATE TABLE alarmevents_y2025_m01 PARTITION OF ""AlarmEvents"" FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');");
            migrationBuilder.Sql(@"CREATE TABLE alarmevents_y2025_m02 PARTITION OF ""AlarmEvents"" FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');");
            
            // Add other standard tables
             migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataSources = table.Column<string>(type: "jsonb", nullable: false),
                    Format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Schedule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "DataRetentionPolicies",
                columns: table => new
                {
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RetentionPeriod = table.Column<int>(type: "integer", nullable: false),
                    ArchiveLocation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRetentionPolicies", x => x.PolicyId);
                });

            migrationBuilder.CreateTable(
                name: "PredictiveMaintenanceModels",
                columns: table => new
                {
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Framework = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeploymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictiveMaintenanceModels", x => x.ModelId);
                });

            migrationBuilder.CreateTable(
                name: "BlockchainTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BlockchainNetwork = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainTransactions", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "MigrationStrategies",
                columns: table => new
                {
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MappingRules = table.Column<string>(type: "jsonb", nullable: false),
                    ValidationProcedure = table.Column<string>(type: "text", nullable: false),
                    LastExecuted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationStrategies", x => x.StrategyId);
                });

            // Unique Constraints and Indexes
            migrationBuilder.AddUniqueConstraint(
                name: "uq_permission_code",
                table: "Permissions",
                column: "Code");
            
            migrationBuilder.AddUniqueConstraint(
                name: "uq_role_name",
                table: "Roles",
                column: "Name");

            migrationBuilder.AddUniqueConstraint(
                name: "uq_user_username",
                table: "Users",
                column: "Username");
            
            migrationBuilder.AddUniqueConstraint(
                name: "uq_user_email",
                table: "Users",
                column: "Email");

            migrationBuilder.AddUniqueConstraint(
                name: "uq_opctag_serverid_nodeid",
                table: "OpcTags",
                columns: new[] { "ServerId", "NodeId" });

            migrationBuilder.AddUniqueConstraint(
                name: "uq_blockchaintransaction_datahash",
                table: "BlockchainTransactions",
                column: "DataHash");

            migrationBuilder.CreateIndex(
                name: "IX_OpcTags_ServerId",
                table: "OpcTags",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ServerId",
                table: "Subscriptions",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");
            
            migrationBuilder.CreateIndex(
                name: "IX_Dashboards_UserId",
                table: "Dashboards",
                column: "UserId");
            
            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserRoles");
            migrationBuilder.DropTable(name: "RolePermissions");
            migrationBuilder.DropTable(name: "Dashboards");
            migrationBuilder.DropTable(name: "Subscriptions");
            migrationBuilder.DropTable(name: "ReportTemplates");
            migrationBuilder.DropTable(name: "DataRetentionPolicies");
            migrationBuilder.DropTable(name: "PredictiveMaintenanceModels");
            migrationBuilder.DropTable(name: "BlockchainTransactions");
            migrationBuilder.DropTable(name: "MigrationStrategies");

            migrationBuilder.DropTable(name: "AlarmEvents"); // Drops parent partitioned table and all its partitions
            migrationBuilder.DropTable(name: "HistoricalData");
            migrationBuilder.DropTable(name: "DataLogs");
            migrationBuilder.DropTable(name: "AuditLogs");
            
            migrationBuilder.DropTable(name: "OpcTags");
            migrationBuilder.DropTable(name: "OpcServers");
            migrationBuilder.DropTable(name: "Permissions");
            migrationBuilder.DropTable(name: "Roles");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}