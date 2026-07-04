using System;
using JobSafetyPro.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSafetyPro.Infrastructure.Persistence.Migrations
{
    public partial class AddWorkMasterDataAndProfileDepartment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkDepartments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkDepartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSections", x => x.Id);
                });

            WorkMasterDataMigration.Seed(migrationBuilder);

            migrationBuilder.CreateIndex(
                name: "IX_WorkDepartments_Name",
                table: "WorkDepartments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkLocations_Name",
                table: "WorkLocations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSections_Name",
                table: "WorkSections",
                column: "Name",
                unique: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkDepartmentId",
                table: "EmployeeProfiles",
                type: "uniqueidentifier",
                nullable: true);

            var defaultDepartmentId = WorkMasterDataMigration.DefaultDepartmentId;
            migrationBuilder.Sql(
                $"UPDATE EmployeeProfiles SET WorkDepartmentId = '{defaultDepartmentId}' WHERE WorkDepartmentId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkDepartmentId",
                table: "EmployeeProfiles",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_WorkDepartmentId",
                table: "EmployeeProfiles",
                column: "WorkDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeProfiles_WorkDepartments_WorkDepartmentId",
                table: "EmployeeProfiles",
                column: "WorkDepartmentId",
                principalTable: "WorkDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeProfiles_WorkDepartments_WorkDepartmentId",
                table: "EmployeeProfiles");

            migrationBuilder.DropTable(
                name: "WorkDepartments");

            migrationBuilder.DropTable(
                name: "WorkLocations");

            migrationBuilder.DropTable(
                name: "WorkSections");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeProfiles_WorkDepartmentId",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "WorkDepartmentId",
                table: "EmployeeProfiles");
        }
    }
}
