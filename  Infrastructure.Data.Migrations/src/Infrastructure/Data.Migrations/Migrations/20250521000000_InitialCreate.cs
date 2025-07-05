using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opc.System.Infrastructure.Data.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainTransaction",
                columns: table => new
                {
                    transactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    dataHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    blockchainNetwork = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainTransaction", x => x.transactionId);
                    table.UniqueConstraint("uq_blockchaintransaction_datahash", x => x.dataHash);
                });

            migrationBuilder.CreateTable(
                name: "DataRetentionPolicy",
                columns: table => new
                {
                    policyId = table.Column<Guid>(type: "uuid", nullable: false),
                    dataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    retentionPeriod = table.Column<int>(type: "integer", nullable: false),
                    archiveLocation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRetentionPolicy", x => x.policyId);
                });

            migrationBuilder.CreateTable(
                name: "MigrationStrategy",
                columns: table => new
                {
                    strategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    sourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mappingRules = table.Column<string>(type: "jsonb", nullable: false),
                    validationProcedure = table.Column<string>(type: "text", nullable: false),
                    lastExecuted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationStrategy", x => x.strategyId);
                });

            migrationBuilder.CreateTable(
                name: "OpcServer",
                columns: table => new
                {
                    serverId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    endpointUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    securityPolicy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    certificate = table.Column<string>(type: "text", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcServer", x => x.serverId);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    permissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.permissionId);
                    table.UniqueConstraint("uq_permission_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "PredictiveMaintenanceModel",
                columns: table => new
                {
                    modelId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    framework = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deploymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictiveMaintenanceModel", x => x.modelId);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplate",
                columns: table => new
                {
                    templateId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dataSources = table.Column<string>(type: "jsonb", nullable: false),
                    format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    schedule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplate", x => x.templateId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    roleId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isSystemRole = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.roleId);
                    table.UniqueConstraint("uq_role_name", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passwordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    lastLogin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    createdAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    externalIdpId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.userId);
                    table.UniqueConstraint("uq_user_email", x => x.email);
                    table.UniqueConstraint("uq_user_username", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "OpcTag",
                columns: table => new
                {
                    tagId = table.Column<Guid>(type: "uuid", nullable: false),
                    serverId = table.Column<Guid>(type: "uuid", nullable: false),
                    nodeId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    dataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    isWritable = table.Column<bool>(type: "boolean", nullable: false),
                    validationRules = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcTag", x => x.tagId);
                    table.UniqueConstraint("uq_opctag_serverid_nodeid", x => new { x.serverId, x.nodeId });
                    table.ForeignKey(
                        name: "FK_OpcTag_OpcServer_serverId",
                        column: x => x.serverId,
                        principalTable: "OpcServer",
                        principalColumn: "serverId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    subscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    serverId = table.Column<Guid>(type: "uuid", nullable: false),
                    publishingInterval = table.Column<int>(type: "integer", nullable: false),
                    samplingInterval = table.Column<int>(type: "integer", nullable: false),
                    queueSize = table.Column<int>(type: "integer", nullable: false),
                    deadbandType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    deadbandValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.subscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscription_OpcServer_serverId",
                        column: x => x.serverId,
                        principalTable: "OpcServer",
                        principalColumn: "serverId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    roleId = table.Column<Guid>(type: "uuid", nullable: false),
                    permissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.roleId, x.permissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_permissionId",
                        column: x => x.permissionId,
                        principalTable: "Permission",
                        principalColumn: "permissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_roleId",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    logId = table.Column<Guid>(type: "uuid", nullable: false),
                    eventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: false),
                    ipAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.logId);
                    table.ForeignKey(
                        name: "FK_AuditLog_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Dashboard",
                columns: table => new
                {
                    dashboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    layoutConfig = table.Column<string>(type: "jsonb", nullable: false),
                    isDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboard", x => x.dashboardId);
                    table.ForeignKey(
                        name: "FK_Dashboard_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    roleId = table.Column<Guid>(type: "uuid", nullable: false),
                    assignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.userId, x.roleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_roleId",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlarmEvent",
                columns: table => new
                {
                    alarmId = table.Column<Guid>(type: "uuid", nullable: false),
                    tagId = table.Column<Guid>(type: "uuid", nullable: false),
                    eventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    occurrenceTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    acknowledgedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmEvent", x => x.alarmId);
                    table.ForeignKey(
                        name: "FK_AlarmEvent_OpcTag_tagId",
                        column: x => x.tagId,
                        principalTable: "OpcTag",
                        principalColumn: "tagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataLog",
                columns: table => new
                {
                    logId = table.Column<Guid>(type: "uuid", nullable: false),
                    tagId = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: true),
                    oldValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    newValue = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    operationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataLog", x => x.logId);
                    table.ForeignKey(
                        name: "FK_DataLog_OpcTag_tagId",
                        column: x => x.tagId,
                        principalTable: "OpcTag",
                        principalColumn: "tagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataLog_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalData",
                columns: table => new
                {
                    dataId = table.Column<Guid>(type: "uuid", nullable: false),
                    tagId = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    quality = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aggregationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalData", x => x.dataId);
                    table.ForeignKey(
                        name: "FK_HistoricalData_OpcTag_tagId",
                        column: x => x.tagId,
                        principalTable: "OpcTag",
                        principalColumn: "tagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_alarmevent_tagid_occurrencetime",
                table: "AlarmEvent",
                columns: new[] { "tagId", "occurrenceTime" });

            migrationBuilder.CreateIndex(
                name: "idx_alarmevent_unack_occurrencetime",
                table: "AlarmEvent",
                column: "occurrenceTime")
                .Annotation("Npgsql:IndexInclude", new string[0])
                .Annotation("Npgsql:IndexFilter", "\"acknowledgedTime\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_userId",
                table: "AuditLog",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "idx_auditlog_timestamp_eventtype",
                table: "AuditLog",
                columns: new[] { "timestamp", "eventType" });

            migrationBuilder.CreateIndex(
                name: "IX_Dashboard_userId",
                table: "Dashboard",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_DataLog_tagId",
                table: "DataLog",
                column: "tagId");

            migrationBuilder.CreateIndex(
                name: "IX_DataLog_userId",
                table: "DataLog",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "idx_datalog_tagid_timestamp",
                table: "DataLog",
                columns: new[] { "tagId", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalData_tagId",
                table: "HistoricalData",
                column: "tagId");

            migrationBuilder.CreateIndex(
                name: "idx_historicaldata_tagid_timestamp",
                table: "HistoricalData",
                columns: new[] { "tagId", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OpcTag_serverId",
                table: "OpcTag",
                column: "serverId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_permissionId",
                table: "RolePermission",
                column: "permissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_serverId",
                table: "Subscription",
                column: "serverId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_roleId",
                table: "UserRole",
                column: "roleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlarmEvent");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BlockchainTransaction");

            migrationBuilder.DropTable(
                name: "Dashboard");

            migrationBuilder.DropTable(
                name: "DataLog");

            migrationBuilder.DropTable(
                name: "DataRetentionPolicy");

            migrationBuilder.DropTable(
                name: "HistoricalData");

            migrationBuilder.DropTable(
                name: "MigrationStrategy");

            migrationBuilder.DropTable(
                name: "PredictiveMaintenanceModel");

            migrationBuilder.DropTable(
                name: "ReportTemplate");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "OpcTag");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "OpcServer");
        }
    }
}