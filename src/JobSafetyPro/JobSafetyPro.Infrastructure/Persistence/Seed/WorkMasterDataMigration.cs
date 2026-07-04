using Microsoft.EntityFrameworkCore.Migrations;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

internal static class WorkMasterDataMigration
{
    public static void Seed(MigrationBuilder migrationBuilder)
    {
        var utcNow = new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc);

        foreach (var (name, sortOrder) in WorkMasterDataSeed.Departments)
        {
            InsertDepartment(migrationBuilder, name, sortOrder, utcNow);
        }

        foreach (var (name, sortOrder) in WorkMasterDataSeed.Locations)
        {
            InsertLocation(migrationBuilder, name, sortOrder, utcNow);
        }

        foreach (var (name, sortOrder) in WorkMasterDataSeed.Sections)
        {
            InsertSection(migrationBuilder, name, sortOrder, utcNow);
        }
    }

    public static Guid DefaultDepartmentId => WorkMasterDataIds.Department(WorkMasterDataSeed.Departments[0].Name);

    private static void InsertDepartment(MigrationBuilder migrationBuilder, string name, int sortOrder, DateTime utcNow)
    {
        var id = WorkMasterDataIds.Department(name);
        migrationBuilder.InsertData(
            table: "WorkDepartments",
            columns: new[] { "Id", "Name", "SortOrder", "IsActive", "CreatedDate", "CreatedBy", "IsDeleted" },
            values: new object[] { id, name, sortOrder, true, utcNow, "migration", false });
    }

    private static void InsertLocation(MigrationBuilder migrationBuilder, string name, int sortOrder, DateTime utcNow)
    {
        var id = WorkMasterDataIds.Location(name);
        migrationBuilder.InsertData(
            table: "WorkLocations",
            columns: new[] { "Id", "Name", "SortOrder", "IsActive", "CreatedDate", "CreatedBy", "IsDeleted" },
            values: new object[] { id, name, sortOrder, true, utcNow, "migration", false });
    }

    private static void InsertSection(MigrationBuilder migrationBuilder, string name, int sortOrder, DateTime utcNow)
    {
        var id = WorkMasterDataIds.Section(name);
        migrationBuilder.InsertData(
            table: "WorkSections",
            columns: new[] { "Id", "Name", "SortOrder", "IsActive", "CreatedDate", "CreatedBy", "IsDeleted" },
            values: new object[] { id, name, sortOrder, true, utcNow, "migration", false });
    }
}
