using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientVersionStatuses",
                columns: table => new
                {
                    ClientVersionStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentAppVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentSchemaVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AutoUpdateEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdateCheckAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateResult = table.Column<int>(type: "int", nullable: true),
                    LastUpdateErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientVersionStatuses", x => x.ClientVersionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Logger = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContextJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "SchemaVersions",
                columns: table => new
                {
                    SchemaVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableForUpgrade = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableForDowngrade = table.Column<bool>(type: "bit", nullable: false),
                    DevApproved = table.Column<bool>(type: "bit", nullable: false),
                    DevApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DevApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserApproved = table.Column<bool>(type: "bit", nullable: false),
                    UserApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MigrationScriptUpPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MigrationScriptDownPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaVersions", x => x.SchemaVersionId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValueEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    AccountStatus = table.Column<int>(type: "int", nullable: false),
                    LockoutCount = table.Column<int>(type: "int", nullable: false),
                    LockoutUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientVersionStatuses_MachineName",
                table: "ClientVersionStatuses",
                column: "MachineName");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Level",
                table: "Logs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaVersions_IsCurrent",
                table: "SchemaVersions",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaVersions_Version",
                table: "SchemaVersions",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ScopeType_UserId_Category_Key",
                table: "Settings",
                columns: new[] { "ScopeType", "UserId", "Category", "Key" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LoginId",
                table: "Users",
                column: "LoginId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientVersionStatuses");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "SchemaVersions");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
