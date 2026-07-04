using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddJsaWorkLocationSectionIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkLocationId",
                table: "JobSafetyAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkSectionId",
                table: "JobSafetyAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_WorkLocationId",
                table: "JobSafetyAssessments",
                column: "WorkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSafetyAssessments_WorkSectionId",
                table: "JobSafetyAssessments",
                column: "WorkSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobSafetyAssessments_WorkLocations_WorkLocationId",
                table: "JobSafetyAssessments",
                column: "WorkLocationId",
                principalTable: "WorkLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobSafetyAssessments_WorkSections_WorkSectionId",
                table: "JobSafetyAssessments",
                column: "WorkSectionId",
                principalTable: "WorkSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobSafetyAssessments_WorkLocations_WorkLocationId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobSafetyAssessments_WorkSections_WorkSectionId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_WorkLocationId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropIndex(
                name: "IX_JobSafetyAssessments_WorkSectionId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "WorkLocationId",
                table: "JobSafetyAssessments");

            migrationBuilder.DropColumn(
                name: "WorkSectionId",
                table: "JobSafetyAssessments");
        }
    }
}
