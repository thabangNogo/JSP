using JobSafetyPro.Domain.Entities.MasterData;

namespace JobSafetyPro.Infrastructure.Persistence.Seed;

public static class WorkMasterDataSeed
{
    public static readonly (string Name, int SortOrder)[] Departments =
    {
        ("Machine Shop", 1),
        ("Fabrication (Welding & Boilermaking)", 2),
        ("Fitting", 3),
        ("Stripping", 4),
        ("Maintenance", 5),
        ("Warehouse", 6),
    };

    public static readonly (string Name, int SortOrder)[] Locations =
    {
        ("Bay 1", 1),
        ("Bay 2", 2),
        ("Bay 3", 3),
        ("Bay 4", 4),
        ("Bay 5", 5),
        ("Workshop Yard", 6),
        ("Maintenance Area", 7),
        ("Warehouse", 8),
        ("Fabrication Area", 9),
        ("Machine Shop", 10),
    };

    public static readonly (string Name, int SortOrder)[] Sections =
    {
        ("Mechanical Repairs", 1),
        ("Welding Area", 2),
        ("Cutting Area", 3),
        ("Assembly Area", 4),
        ("Pump Repairs", 5),
        ("Conveyor Repairs", 6),
        ("Inspection Area", 7),
        ("Stores Area", 8),
        ("Loading Area", 9),
        ("Breakdown Area", 10),
        ("General Maintenance", 11),
    };

    public static IReadOnlyList<WorkDepartment> CreateDepartments(DateTime utcNow) =>
        Departments.Select(d => new WorkDepartment
        {
            Id = WorkMasterDataIds.Department(d.Name),
            Name = d.Name,
            SortOrder = d.SortOrder,
            IsActive = true,
            CreatedDate = utcNow,
            CreatedBy = "system",
        }).ToList();

    public static IReadOnlyList<WorkLocation> CreateLocations(DateTime utcNow) =>
        Locations.Select(l => new WorkLocation
        {
            Id = WorkMasterDataIds.Location(l.Name),
            Name = l.Name,
            SortOrder = l.SortOrder,
            IsActive = true,
            CreatedDate = utcNow,
            CreatedBy = "system",
        }).ToList();

    public static IReadOnlyList<WorkSection> CreateSections(DateTime utcNow) =>
        Sections.Select(s => new WorkSection
        {
            Id = WorkMasterDataIds.Section(s.Name),
            Name = s.Name,
            SortOrder = s.SortOrder,
            IsActive = true,
            CreatedDate = utcNow,
            CreatedBy = "system",
        }).ToList();
}
