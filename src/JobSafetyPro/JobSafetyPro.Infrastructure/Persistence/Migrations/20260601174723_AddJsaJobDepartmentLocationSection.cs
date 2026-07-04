using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddJsaJobDepartmentLocationSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "JobSafetyAssessments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "JobSafetyAssessments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "JobSafetyAssessments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_Department",
                table: "JobSafetyAssessments",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_Location",
                table: "JobSafetyAssessments",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_Section",
                table: "JobSafetyAssessments",
                column: "Section");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_Department",
                table: "JobSafetyAssessments");

            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_Location",
                table: "JobSafetyAssessments");

            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_Section",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "JobSafetyAssessments");
        }
    }
}
