using JobSafetyPro.Domain.Common;
using JobSafetyPro.Domain.Entities.Identity;

namespace JobSafetyPro.Domain.Entities.MasterData;

public class WorkDepartment : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeProfile> EmployeeProfiles { get; set; } = new List<EmployeeProfile>();
}
