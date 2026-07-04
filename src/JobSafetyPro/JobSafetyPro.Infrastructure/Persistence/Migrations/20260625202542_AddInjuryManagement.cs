using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddInjuryManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Injuries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InjuryOccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InjuryFreeDaysResetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    InjuryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BodyPartInjured = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IncidentDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ImmediateActionTaken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LostTimeDays = table.Column<int>(type: "int", nullable: true),
                    Witnesses = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CapturedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Injuries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Injuries_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Injuries_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Injuries_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Injuries_Users_CapturedByUserId",
                        column: x => x.CapturedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Injuries_Users_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InjuryNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InjuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InjuryNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InjuryNotifications_Injuries_InjuryId",
                        column: x => x.InjuryId,
                        principalTable: "Injuries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InjuryNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InjuryPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InjuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InjuryPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InjuryPhotos_Injuries_InjuryId",
                        column: x => x.InjuryId,
                        principalTable: "Injuries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_CapturedByUserId",
                table: "Injuries",
                column: "CapturedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_CompanyId",
                table: "Injuries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_DepartmentId",
                table: "Injuries",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_EmployeeUserId",
                table: "Injuries",
                column: "EmployeeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_InjuryFreeDaysResetDate",
                table: "Injuries",
                column: "InjuryFreeDaysResetDate");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_PlantId",
                table: "Injuries",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_SubmittedAt",
                table: "Injuries",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InjuryNotifications_InjuryId",
                table: "InjuryNotifications",
                column: "InjuryId");

            migrationBuilder.CreateIndex(
                name: "IX_InjuryNotifications_UserId",
                table: "InjuryNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InjuryPhotos_InjuryId",
                table: "InjuryPhotos",
                column: "InjuryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InjuryNotifications");

            migrationBuilder.DropTable(
                name: "InjuryPhotos");

            migrationBuilder.DropTable(
                name: "Injuries");
        }
    }
}
