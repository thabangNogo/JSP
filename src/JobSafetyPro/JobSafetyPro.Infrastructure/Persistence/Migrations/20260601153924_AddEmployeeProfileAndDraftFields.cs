using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddEmployeeProfileAndDraftFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "JobSafetyAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "JobSafetyAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSavedAt",
                table: "JobSafetyAssessments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffCompanyNumber",
                table: "JobSafetyAssessments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffName",
                table: "JobSafetyAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffOccupation",
                table: "JobSafetyAssessments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignOffSurname",
                table: "JobSafetyAssessments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureStoragePath",
                table: "JobSafetyAssessments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowDataJson",
                table: "JobSafetyAssessments",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_CreatedByUserId",
                table: "JobSafetyAssessments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_UserId",
                table: "EmployeeProfiles",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_CreatedByUserId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "LastSavedAt",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "SignOffCompanyNumber",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "SignOffName",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "SignOffOccupation",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "SignOffSurname",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "SignatureStoragePath",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "WorkflowDataJson",
                table: "JobSafetyAssessments");
        }
    }
}
