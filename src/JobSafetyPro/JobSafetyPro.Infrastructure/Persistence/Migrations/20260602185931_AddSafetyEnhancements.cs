using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddSafetyEnhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "NearMisses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClosureNotes",
                table: "NearMisses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectiveActionPlan",
                table: "NearMisses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "NearMisses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InvestigationNotes",
                table: "NearMisses",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvestigatorUserId",
                table: "NearMisses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "NearMisses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsiblePersonUserId",
                table: "NearMisses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootCause",
                table: "NearMisses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RootCauseCategory",
                table: "NearMisses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "NearMisses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "NearMisses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDate",
                table: "NearMisses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SafetyNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StopUnsafeWorkReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImmediateRisk = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActionsTaken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopUnsafeWorkReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StopUnsafeWorkReports_Users_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FcmToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NearMisses_InvestigatorUserId",
                table: "NearMisses",
                column: "InvestigatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NearMisses_ResponsiblePersonUserId",
                table: "NearMisses",
                column: "ResponsiblePersonUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NearMisses_Status",
                table: "NearMisses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyNotifications_UserId_IsRead",
                table: "SafetyNotifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_StopUnsafeWorkReports_ReportedByUserId",
                table: "StopUnsafeWorkReports",
                column: "ReportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StopUnsafeWorkReports_Status",
                table: "StopUnsafeWorkReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_FcmToken",
                table: "UserDevices",
                column: "FcmToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NearMisses_Users_InvestigatorUserId",
                table: "NearMisses",
                column: "InvestigatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NearMisses_Users_ResponsiblePersonUserId",
                table: "NearMisses",
                column: "ResponsiblePersonUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NearMisses_Users_InvestigatorUserId",
                table: "NearMisses");

            migrationBuilder.DropForeignKey(
                name: "FK_NearMisses_Users_ResponsiblePersonUserId",
                table: "NearMisses");

            migrationBuilder.DropTable(
                name: "SafetyNotifications");

            migrationBuilder.DropTable(
                name: "StopUnsafeWorkReports");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_NearMisses_InvestigatorUserId",
                table: "NearMisses");

            migrationBuilder.DropIndex(
                name: "IX_NearMisses_ResponsiblePersonUserId",
                table: "NearMisses");

            migrationBuilder.DropIndex(
                name: "IX_NearMisses_Status",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "ClosureNotes",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "CorrectiveActionPlan",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "InvestigationNotes",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "InvestigatorUserId",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonUserId",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "RootCause",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "RootCauseCategory",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "NearMisses");

            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "NearMisses");
        }
    }
}
